using Gebug64.Unfloader.Message;
using Gebug64.Unfloader.UsbPacket;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader
{
    /// <inheritdoc />
    public class DeviceManager : IDeviceManager
    {
        private const int TestCommandTimeoutMs = 1000;

        private const int SafeShutdownTimeoutMs = 5000;

        /// <summary>
        /// Worker thread to manage communcation.
        /// </summary>
        private Thread? _thread = null;

        /// <summary>
        /// Device.
        /// </summary>
        private readonly IFlashcart? _flashcart = null;

        /// <summary>
        /// Worker thread quit flag.
        /// </summary>
        private bool _stop = true;

        /// <summary>
        /// Messages to send to the console.
        /// </summary>
        private ConcurrentQueue<IGebugMessage> _sendToConsoleQueue = new();

        /// <summary>
        /// Full and complete messages received from the console.
        /// </summary>
        private ConcurrentQueue<IGebugMessage> _receiveFromConsoleQueue = new();

        /// <summary>
        /// Multi part messages will be considered "fragments". Store incoming fragments
        /// in this collection while the message is being received.
        /// </summary>
        private List<RomAckMessage> _receiveFragments = new();

        /// <summary>
        /// Certain operations should send a message and then immediately receive
        /// a response, without the worker thread removing the message from <see cref="_receiveFromConsoleQueue"/>.
        /// Use a "priority lock" to temporarily take control of <see cref="_receiveFromConsoleQueue"/>.
        /// </summary>
        private Mutex _priorityLock = new();

        /// <summary>
        /// Message bus to notify subscribers.
        /// </summary>
        private MessageBus<IGebugMessage> _messageBus = new();

        /// <inheritdoc />
        public IFlashcart? Flashcart => _flashcart;

        /// <inheritdoc />
        public ConcurrentQueue<IGebugMessage> SendToConsole => _sendToConsoleQueue;

        /// <inheritdoc />
        public bool IsShutdown => object.ReferenceEquals(null, _thread) || !_thread.IsAlive;

        /// <inheritdoc />
        public TimeSpan SinceDataReceived => _flashcart?.SinceDataReceived ?? TimeSpan.MaxValue;

        /// <inheritdoc />
        public TimeSpan SinceRomMessageReceived => _flashcart?.SinceRomMessageReceived ?? TimeSpan.MaxValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceManager"/> class.
        /// </summary>
        /// <param name="flashcart">Device.</param>
        public DeviceManager(IFlashcart flashcart)
        {
            if (object.ReferenceEquals(null, flashcart))
            {
                throw new NullReferenceException(nameof(flashcart));
            }

            _flashcart = flashcart;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _stop = true;

            if (!object.ReferenceEquals(null, _flashcart))
            {
                _flashcart!.Disconnect();
                _flashcart.Dispose();
            }
        }

        /// <inheritdoc />
        public void EnqueueMessage(IGebugMessage message)
        {
            _sendToConsoleQueue.Enqueue(message);
        }

        /// <inheritdoc />
        public void Init(string portName)
        {
            _flashcart!.Init(portName);
        }

        /// <inheritdoc />
        public bool TestRomConnected()
        {
            // Try to take control of the receive message queue, otherwise fail.
            if (!_priorityLock.WaitOne(TestCommandTimeoutMs))
            {
                return false;
            }

            if (object.ReferenceEquals(null, _flashcart))
            {
                _priorityLock.ReleaseMutex();
                return false;
            }

            // Send the test command.
            var sendMsg = new RomMetaMessage(Unfloader.Message.MessageType.GebugCmdMeta.Ping) { Source = CommunicationSource.Pc };
            _flashcart.Send(sendMsg);

            // Wait a short while for the device to respond.
            System.Threading.Thread.Sleep(100);

            IGebugMessage? msg;

            // If there's no response, the test failed.
            if (_flashcart.MessagesFromConsole.IsEmpty)
            {
                _priorityLock.ReleaseMutex();
                return false;
            }

            var tryDequeueTimer = Stopwatch.StartNew();

            // Else, get the next message response.
            while (!_flashcart!.MessagesFromConsole.TryDequeue(out msg))
            {
                if (tryDequeueTimer.ElapsedMilliseconds > TestCommandTimeoutMs)
                {
                    _priorityLock.ReleaseMutex();
                    return false;
                }
            }

            // Expecting an Ack Meta Ping response.
            if (msg != null && msg is RomMessage romMessage)
            {
                if (romMessage.Category == Unfloader.Message.MessageType.GebugMessageCategory.Ack)
                {
                    var ackMessage = (RomAckMessage)romMessage;

                    if (ackMessage?.Reply?.Category == Unfloader.Message.MessageType.GebugMessageCategory.Meta)
                    {
                        var metaMessage = (RomMetaMessage)ackMessage.Reply;
                        if (metaMessage.Command == Unfloader.Message.MessageType.GebugCmdMeta.Ping)
                        {
                            // success
                            _priorityLock.ReleaseMutex();
                            return true;
                        }
                    }
                }
            }

            // Otherwise, put the message back on the queue for anyone else.
            if (msg != null)
            {
                _flashcart!.MessagesFromConsole.Enqueue(msg);
            }

            _priorityLock.ReleaseMutex();
            return false;
        }

        /// <inheritdoc />
        public bool TestFlashcartConnected()
        {
            // Try to take control of the receive message queue, otherwise fail.
            if (!_priorityLock.WaitOne(TestCommandTimeoutMs))
            {
                return false;
            }

            if (object.ReferenceEquals(null, _flashcart))
            {
                _priorityLock.ReleaseMutex();
                return false;
            }

            // Send the test command.
            ((Flashcart.FlashcartBase)_flashcart)!.SendTest();

            // Wait a short while for the device to respond.
            System.Threading.Thread.Sleep(100);

            IGebugMessage? msg;

            // If there's no response, the test failed.
            if (_flashcart.MessagesFromConsole.IsEmpty)
            {
                _priorityLock.ReleaseMutex();
                return false;
            }

            var tryDequeueTimer = Stopwatch.StartNew();

            // Else, get the next message response.
            while (!_flashcart!.MessagesFromConsole.TryDequeue(out msg))
            {
                if (tryDequeueTimer.ElapsedMilliseconds > TestCommandTimeoutMs)
                {
                    _priorityLock.ReleaseMutex();
                    return false;
                }
            }

            // Check the device specific response format. If this is valid,
            // then we're done.
            if (((Flashcart.FlashcartBase)_flashcart)!.IsTestCommandResponse(msg.GetUsbPacket()))
            {
                _priorityLock.ReleaseMutex();
                return true;
            }

            // Otherwise, put the message back on the queue for anyone else.
            _flashcart!.MessagesFromConsole.Enqueue(msg);
            _priorityLock.ReleaseMutex();
            return false;
        }

        /// <inheritdoc />
        public void Start()
        {
            _stop = false;

            if (object.ReferenceEquals(null, _thread))
            {
                _thread = new Thread(new ThreadStart(ThreadMain));
                _thread.IsBackground = true;

                _thread.Start();

                return;
            }

            if (_thread.IsAlive)
            {
                return;
            }

            throw new InvalidOperationException("DeviceManager thread cannot be restarted.");
        }

        /// <inheritdoc />
        public void Stop()
        {
            _stop = true;

            _flashcart!.Disconnect();

            var sw = Stopwatch.StartNew();

            if (!object.ReferenceEquals(null, _thread))
            {
                while (_thread.IsAlive)
                {
                    System.Threading.Thread.Sleep(10);

                    if (sw.ElapsedMilliseconds > SafeShutdownTimeoutMs)
                    {
                        throw new Exception("Could not safely terminate device manager thread.");
                    }
                }
            }

            _thread = null;

            _receiveFromConsoleQueue.Clear();
            _sendToConsoleQueue.Clear();
            _receiveFragments.Clear();
        }

        /// <inheritdoc />
        public void SendRom(string path, Nullable<CancellationToken> token = null)
        {
            if (!System.IO.File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find file: {path}");
            }

            var filedata = System.IO.File.ReadAllBytes(path);

            // Read the ROM header to check if its byteswapped
            if (!(filedata[0] == 0x80 && filedata[1] == 0x37 && filedata[2] == 0x12 && filedata[3] == 0x40))
            {
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
            return _messageBus.Subscribe(callback, listenCount, filter);
        }

        /// <inheritdoc />
        public void Unsubscribe(Guid id)
        {
            _messageBus.Unsubscribe(id);
        }

        /// <summary>
        /// Core functionality. Background worker thread.
        /// Executes in three phases.
        /// Phase 1: Receive messages from console.
        /// Phase 2: Publish messages to subscribers.
        /// Phase 3: Send out going messages to console.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        private void ThreadMain()
        {
            // Run forever until stop flag.
            while (true)
            {
                if (_stop)
                {
                    break;
                }

                // Phase 1: Receive messages from console.
                if (_flashcart!.MessagesFromConsole.Any() && _priorityLock.WaitOne(10))
                {
                    // Run until thread shutdown, or incoming queue is empty.
                    while (true)
                    {
                        if (_stop || !_flashcart.MessagesFromConsole.Any())
                        {
                            break;
                        }

                        IGebugMessage? msg;
                        while (!_flashcart!.MessagesFromConsole.TryDequeue(out msg))
                        {
                            if (_stop || !_flashcart.MessagesFromConsole.Any())
                            {
                                break;
                            }
                        }

                        if (!object.ReferenceEquals(null, msg))
                        {
                            var pending = msg as PendingGebugMessage;

                            if (object.ReferenceEquals(null, pending))
                            {
                                throw new NullReferenceException($"{nameof(IFlashcart.MessagesFromConsole)} should contain {nameof(PendingGebugMessage)}");
                            }

                            // try to parse as gebug rom message.
                            RomMessage? romMessage = null;
                            var romMessageParseResult = RomMessageParseResult.Error;

                            try
                            {
                                romMessageParseResult = RomMessage.Parse(pending.GetUsbPacket()!.GetInnerData()!, out romMessage);
                            }
                            catch
                            {
                                romMessageParseResult = RomMessageParseResult.Error;
                            }

                            if (romMessageParseResult == RomMessageParseResult.Success)
                            {
                                bool queued = false;

                                romMessage!.Source = CommunicationSource.N64;

                                // This is a RomMessage. If this is a multi part Ack then save as a fragment.
                                // Otherwise treat as complete message.
                                if (romMessage.Category == Message.MessageType.GebugMessageCategory.Ack)
                                {
                                    var ackMessage = (RomAckMessage)romMessage;
                                    if (ackMessage.TotalNumberPackets > 1)
                                    {
                                        queued = true;
                                        _receiveFragments.Add(ackMessage);
                                    }

                                    // Once all fragments have arrived, assembly into a single message.
                                    if (_receiveFragments.Count >= ackMessage.TotalNumberPackets)
                                    {
                                        RomMultiAckMessage multiMessage = new RomMultiAckMessage()
                                        {
                                            Source = CommunicationSource.N64,
                                            InstantiateTime = romMessage.InstantiateTime,
                                            PacketNumber = 1,
                                            TotalNumberPackets = ackMessage.TotalNumberPackets,
                                        };

                                        foreach (var f in _receiveFragments)
                                        {
                                            multiMessage.Fragments.Add(f);
                                        }

                                        multiMessage.UnwrapFragments();

                                        _receiveFragments.Clear();
                                        _receiveFromConsoleQueue.Enqueue(multiMessage);
                                    }
                                }

                                if (!queued)
                                {
                                    _receiveFromConsoleQueue.Enqueue(romMessage);
                                }
                            }
                            else
                            {
                                // else: raw text string (osSyncPrintf),
                                // Unfloader heartbeat message,
                                // etc.

                                bool queueDone = false;
                                GebugMessageBase? gebug = msg as GebugMessageBase;

                                if (!object.ReferenceEquals(null, gebug) && msg.PacketDataSet)
                                {
                                    IPacket packet = msg.GetUsbPacket();

                                    if (packet.DataType == PacketType.HeartBeat
                                        || packet.DataType == PacketType.Text)
                                    {
                                        var transform = new DebugMessage(gebug);
                                        _receiveFromConsoleQueue.Enqueue(transform);
                                        queueDone = true;
                                    }
                                }

                                // Anything else.
                                if (!queueDone)
                                {
                                    _receiveFromConsoleQueue.Enqueue(msg);
                                }
                            }
                        }
                    }

                    _priorityLock.ReleaseMutex();
                }

                // Phase 2: Publish messages to subscribers.
                if (_priorityLock.WaitOne(10))
                {
                    while (!_receiveFromConsoleQueue.IsEmpty)
                    {
                        IGebugMessage? msg;
                        while (!_receiveFromConsoleQueue.TryDequeue(out msg))
                        {
                            if (_stop || _receiveFromConsoleQueue.IsEmpty)
                            {
                                break;
                            }
                        }

                        if (!object.ReferenceEquals(null, msg))
                        {
                            _messageBus.Publish(msg);
                        }

                        if (_stop)
                        {
                            break;
                        }
                    }

                    _priorityLock.ReleaseMutex();
                }

                // Phase 3: Send out going messages to console.
                while (!_sendToConsoleQueue.IsEmpty)
                {
                    IGebugMessage? msg;
                    while (!_sendToConsoleQueue.TryDequeue(out msg))
                    {
                        if (_stop || _sendToConsoleQueue.IsEmpty)
                        {
                            break;
                        }
                    }

                    if (!object.ReferenceEquals(null, msg))
                    {
                        _flashcart.Send(msg);
                    }

                    if (_stop)
                    {
                        break;
                    }
                }
            }
        }
    }
}
