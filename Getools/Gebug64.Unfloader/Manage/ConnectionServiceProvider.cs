using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.Protocol.Flashcart.Message;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Parse;
using Gebug64.Unfloader.Protocol.Unfloader;
using Gebug64.Unfloader.Protocol.Unfloader.Message;
using Gebug64.Unfloader.Protocol.Unfloader.Message.MessageType;

namespace Gebug64.Unfloader.Manage
{
    /// <summary>
    /// This is the core communication provider for Gebug64.
    /// This runs a background thread to send and receive messages
    /// from the gebug romhack. High level gebug messages are supported, as well
    /// as lower level UNFLoader packets (osSyncPrintf text).
    /// Receiving messages is handled asynchronously. In order to receive a reply
    /// to a specific message an appropriate callback and filter should be passed
    /// to the correct Subscribe method.
    /// </summary>
    public class ConnectionServiceProvider : IConnectionServiceProvider
    {
        /// <summary>
        /// The flashcart managed by this service provider.
        /// </summary>
        private readonly IFlashcart _flashcart;

        /// <summary>
        /// Thread shutdown flag.
        /// </summary>
        private bool _stop = false;

        /// <summary>
        /// Main background worker thread.
        /// </summary>
        private Thread? _thread = null;

        /// <summary>
        /// Incoming data was evaluated and contained UNFLoader packet, but nothing higher.
        /// </summary>
        private List<IUnfloaderPacket> _receiveUnfPackets = new();

        /// <summary>
        /// Incoming data was evaluated, and it's a Gebug packet that is a part of a multi-packet message.
        /// </summary>
        private List<GebugPacket> _receiveMessageFragments = new();

        /// <summary>
        /// Incoming data was evaluated, and --
        /// 1) It was a single packet message.
        /// 2) It was the last remaining packet in a multi-packet message, which
        /// has now been compiled into a single message.
        /// </summary>
        private List<IGebugMessage> _receiveMessages = new();

        /// <summary>
        /// This is the outgoing queue.
        /// <see cref="IUnfloaderPacket"/> are simply placed in the queue.
        /// <see cref="IGebugMessage"/> are first converted to <see cref="IUnfloaderPacket"/> then placed
        /// in the queue.
        /// </summary>
        private ConcurrentQueue<IUnfloaderPacket> _sendQueue = new ConcurrentQueue<IUnfloaderPacket>();

        /// <summary>
        /// Message bus for incoming <see cref="IGebugMessage"/>.
        /// </summary>
        private MessageBus<IGebugMessage> _messageBusGebug = new();

        /// <summary>
        /// Message bus for incoming <see cref="IUnfloaderPacket"/>.
        /// </summary>
        private MessageBus<IUnfloaderPacket> _messageBusUnfloader = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionServiceProvider"/> class.
        /// </summary>
        /// <param name="flashcart">Flashcart to manage.</param>
        public ConnectionServiceProvider(IFlashcart flashcart)
        {
            _flashcart = flashcart;
        }

        /// <inheritdoc />
        public bool ManagerActive { get; set; } = true;

        /// <inheritdoc />
        public bool IsShutdown => object.ReferenceEquals(null, _thread) || !_thread.IsAlive;

        /// <inheritdoc />
        public TimeSpan SinceDataReceived => _flashcart?.SinceDataReceived ?? TimeSpan.MaxValue;

        /// <inheritdoc />
        public TimeSpan SinceRomMessageReceived => _flashcart?.SinceRomMessageReceived ?? TimeSpan.MaxValue;

        /// <inheritdoc />
        public void Start(string port)
        {
            if (_flashcart.IsConnected)
            {
                Stop();
            }

            // Allow background thread to work.
            _stop = false;

            _flashcart.Connect(port);

            // There's no pause/resume, so make sure existing queues
            // are clear on connect.
            _receiveUnfPackets.Clear();
            _receiveMessageFragments.Clear();
            _receiveMessages.Clear();
            _sendQueue.Clear();

            _thread = new Thread(ThreadMain);
            _thread.IsBackground = true;
            _thread.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            // Notify background thread to shutdown.
            _stop = true;

            _flashcart.Disconnect();

            // Wait for background thread to quit, or throw fatal exception.
            if (!Object.ReferenceEquals(null, _thread))
            {
                var timeout = Stopwatch.StartNew();
                while (_thread.IsAlive)
                {
                    if (timeout.ElapsedMilliseconds > 5000)
                    {
                        throw new Exception("Could not safely shutdown worker thread");
                    }
                }

                timeout.Stop();

                _thread = null;
            }

            _receiveUnfPackets.Clear();
            _receiveMessageFragments.Clear();
            _receiveMessages.Clear();
            _sendQueue.Clear();
        }

        /// <inheritdoc />
        public bool TestInMenu() => _flashcart.TestInMenu();

        /// <inheritdoc />
        public bool TestInRom()
        {
            // Instantiate a message to identify the message id.
            GebugMessage sendMesssage = new GebugMetaPingMessage();
            ushort sendMessageId = sendMesssage.MessageId;
            bool receivedPingResponse = false;

            Action<IGebugMessage> callback = message =>
            {
                receivedPingResponse = true;
            };

            Func<IGebugMessage, bool> filter = message =>
            {
                var firstPacket = message.FirstPacket!;

                //// Looking for a Ping message,
                return message.Category == GebugMessageCategory.Meta
                    && message.Command == (int)GebugCmdMeta.Ping
                    //// that's a reply,
                    && (firstPacket.Flags & (ushort)GebugMessageFlags.IsAck) > 0
                    //// to the message we sent.
                    && message.AckId == sendMessageId
                    ;
            };

            // Subscribe for 1 message that matches the filter.
            _messageBusGebug.Subscribe(callback, 1, filter);

            SendMessage(sendMesssage);

            // Wait up to 1 second for response.
            int timeoutMs = 1000;
            var sw = Stopwatch.StartNew();

            while (receivedPingResponse != true)
            {
                System.Threading.Thread.Sleep(10);
                if (sw.Elapsed.TotalMilliseconds > timeoutMs)
                {
                    return false;
                }
            }

            return receivedPingResponse;
        }

        /// <inheritdoc />
        public void SendMessage(IGebugMessage msg)
        {
            // Split message into lower level UNFLoader packets.
            var multiPackets = msg.ToSendPackets(Unfloader.Protocol.Gebug.Parameter.ParameterUseDirection.PcToConsole);
            foreach (var p in multiPackets)
            {
                IUnfloaderPacket unfpacket = new BinaryPacket(p.ToByteArray());
                _sendQueue.Enqueue(unfpacket);
            }
        }

        /// <inheritdoc />
        public void SendMessage(IUnfloaderPacket packet)
        {
            _sendQueue.Enqueue(packet);
        }

        /// <inheritdoc />
        public void SendRom(string path, CancellationToken? token = null)
        {
            if (!System.IO.File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find file: {path}");
            }

            var filedata = System.IO.File.ReadAllBytes(path);

            // Read the ROM header to check if its byteswapped
            if (!(filedata[0] == 0x80 && filedata[1] == 0x37 && filedata[2] == 0x12 && filedata[3] == 0x40))
            {
                // Swap all bytes in ROM.
                for (var j = 0; j < filedata.Length; j += 2)
                {
                    filedata[j] ^= filedata[j + 1];
                    filedata[j + 1] ^= filedata[j];
                    filedata[j] ^= filedata[j + 1];
                }
            }

            _flashcart!.SendRom(filedata, token);
        }

        /// <inheritdoc />
        public Guid Subscribe(Action<IGebugMessage> callback, int listenCount = 0, Func<IGebugMessage, bool>? filter = null)
        {
            return _messageBusGebug.Subscribe(callback, listenCount, filter);
        }

        /// <inheritdoc />
        public Guid Subscribe(Action<IUnfloaderPacket> callback, int listenCount = 0, Func<IUnfloaderPacket, bool>? filter = null)
        {
            return _messageBusUnfloader.Subscribe(callback, listenCount, filter);
        }

        /// <inheritdoc />
        public void GebugUnsubscribe(Guid id)
        {
            _messageBusGebug.Unsubscribe(id);
        }

        /// <inheritdoc />
        public void UnfloaderUnsubscribe(Guid id)
        {
            _messageBusUnfloader.Unsubscribe(id);
        }

        /// <summary>
        /// Main worker thread.
        /// The work is divided into three phases.
        ///
        /// Phase 1) Send all queued messages.
        /// Phase 2) Check the flashcart for any incoming messages.
        /// Phase 3) Publish notifications about incoming messages.
        /// </summary>
        private void ThreadMain()
        {
            while (true)
            {
                // Simple flag to track whether "anything" happened this loop.
                // If there's nothing going on, then the background worker
                // can sleep for a bit.
                bool anyWork = false;

                if (_stop)
                {
                    break;
                }

                // Phase 1) Send all queued messages.
                while (ManagerActive && !_sendQueue.IsEmpty)
                {
                    IUnfloaderPacket? sendPacket;

                    anyWork = true;

                    while (!_sendQueue.TryDequeue(out sendPacket))
                    {
                        if (_stop || _sendQueue.IsEmpty)
                        {
                            break;
                        }
                    }

                    if (_stop)
                    {
                        break;
                    }

                    // Convert UNFloader packet to raw bytes, flashcart instance will convert (wrap)
                    // to flashcart specific packet.
                    _flashcart.Send(sendPacket!.GetOuterPacket());
                }

                if (_stop)
                {
                    break;
                }

                // Phase 2) Check the flashcart for any incoming messages.
                // Since the flashcart is the lowest level protocol, the packets
                // will need to be parsed up the protocl "hierarchy" and evaulated.
                while (ManagerActive && !_flashcart.ReadPackets.IsEmpty)
                {
                    IFlashcartPacket? receivePacket = null;

                    anyWork = true;

                    while (!_flashcart.ReadPackets.TryDequeue(out receivePacket))
                    {
                        // Occassionally it has been possible to start this `while`
                        // loop after the queue became empty, so check for that.
                        if (_stop || _flashcart.ReadPackets.IsEmpty)
                        {
                            break;
                        }
                    }

                    if (_stop || object.ReferenceEquals(null, receivePacket))
                    {
                        break;
                    }

                    // So actually the flashcart parsed the packet already to check if
                    // it's a `IUnfloaderPacket`, because the Everdrive protocol doesn't include
                    // data length of incoming data. The result is saved in the `InnerType`/`InnerData`
                    // properties, so we can just check those.
                    if (typeof(IUnfloaderPacket).IsAssignableFrom(receivePacket.InnerType))
                    {
                        IUnfloaderPacket unfPacket = (IUnfloaderPacket)receivePacket.InnerData!;

                        // if this is gebug ROM message, don't care about UNFLoader wrapper any more
                        var unfInnerData = unfPacket.GetInnerPacket();
                        var gebugParse = GebugPacket.TryParse(unfInnerData.ToList());

                        if (gebugParse.ParseStatus == PacketParseStatus.Success)
                        {
                            var packet = gebugParse.Packet;

                            // If the packet is part of a multi-packet message, save this in the
                            // fragments bucket.
                            if (packet!.TotalNumberPackets > 1)
                            {
                                _receiveMessageFragments.Add(packet);

                                // We just added a new packet. Count the current number of fragments to determine
                                // if this could potentially be the last packet in a multi-packet message.
                                if (_receiveMessageFragments.Count >= packet.TotalNumberPackets)
                                {
                                    // The `count` test passed, so now filter out all the fragments
                                    // that belong to this message.
                                    var fragments = _receiveMessageFragments.Where(x =>
                                        x.Category == packet.Category
                                        && x.Command == packet.Command
                                        && x.TotalNumberPackets == packet.TotalNumberPackets)
                                    .OrderBy(x => x.PacketNumber)
                                    .ToList();

                                    // One last sanity check, make sure the number of packets completes a message.
                                    if (fragments.Count == packet.TotalNumberPackets)
                                    {
                                        // All packets received, convert to single message.
                                        var gebugMessage = GebugMessage.FromPackets(fragments, Protocol.Gebug.Parameter.ParameterUseDirection.ConsoleToPc);
                                        _receiveMessages.Add(gebugMessage);

                                        // And remove all the associated fragments.
                                        foreach (var f in fragments)
                                        {
                                            _receiveMessageFragments.Remove(f);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Else, this is a single packet message, so convert to complete message accordingly.
                                var gebugMessage = GebugMessage.FromPacket(packet, Protocol.Gebug.Parameter.ParameterUseDirection.ConsoleToPc);
                                _receiveMessages.Add(gebugMessage);
                            }
                        }
                        else
                        {
                            // Not a gebug message.
                            _receiveUnfPackets.Add(unfPacket);
                        }
                    }
                    else
                    {
                        // We got a flashcart packet, but it's not a UNFLoader packet or higher.
                        // If it was an everdrive test command response, that should have been
                        // handled already. And there's no message bus to publish just flashcart
                        // packets.
                        // Nothing really to do at this point, so just drop the packet.
                    }
                }

                if (_stop)
                {
                    break;
                }

                /***
                 * Phase 3) Publish notifications about incoming messages.
                 */

                int removeCounter;

                // Read the number of receive packets.
                // Dequeue this number of packets and publish them, unles it's time to stop or disable.
                removeCounter = _receiveUnfPackets.Count;

                while (ManagerActive && !_stop && removeCounter > 0)
                {
                    anyWork = true;
                    _messageBusUnfloader.Publish(_receiveUnfPackets[0]);
                    _receiveUnfPackets.RemoveAt(0);
                    removeCounter--;
                }

                // Read the number of receive packets.
                // Dequeue this number of packets and publish them, unles it's time to stop or disable.
                removeCounter = _receiveMessages.Count;

                while (ManagerActive && !_stop && removeCounter > 0)
                {
                    anyWork = true;
                    _messageBusGebug.Publish(_receiveMessages[0]);
                    _receiveMessages.RemoveAt(0);
                    removeCounter--;
                }

                // if the service provider is disabled, sleep for a bit.
                if (!ManagerActive && !_stop)
                {
                    Thread.Sleep(100);
                }
                else if (anyWork == false && !_stop)
                {
                    // else, if nothing happened this loop iteration, sleep for a bit.
                    Thread.Sleep(1);
                }
            }
        }
    }
}
