using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Parse;
using Gebug64.Unfloader.Protocol.Unfloader;
using Gebug64.Unfloader.Protocol.Unfloader.Message;
using Gebug64.Unfloader.Protocol.Unfloader.Message.MessageType;

namespace Gebug64.Unfloader.Manage
{
    public class ConnectionServiceProvider
    {
        private readonly IFlashcart _flashcart;
        private bool _stop = false;
        private Thread? _thread = null;

        private List<IFlashcartPacket> _receiveFlashcardPackets = new();
        private List<IUnfloaderPacket> _receiveUnfPackets = new();
        private List<GebugPacket> _receiveMessageFragments = new();
        private List<IGebugMessage> _receiveMessages = new();

        private ConcurrentQueue<IUnfloaderPacket> _sendQueue = new ConcurrentQueue<IUnfloaderPacket>();

        private MessageBus<IGebugMessage> _messageBusGebug = new();
        private MessageBus<IUnfloaderPacket> _messageBusUnfloader = new();

        public bool ManagerActive { get; set; } = true;

        public ConnectionServiceProvider(IFlashcart device)
        {
            _flashcart = device;
        }

        public void Start(string port)
        {
            if (_flashcart.IsConnected)
            {
                Stop();
            }

            _stop = false;
            _flashcart.Connect(port);

            _thread = new Thread(ThreadMain);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Stop()
        {
            _stop = true;
            _flashcart.Disconnect();

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
        }

        public bool TestInMenu() { return _flashcart.TestInMenu(); }

        public bool TestInRom() { return false; }

        public void SendMessage(IGebugMessage msg)
        {
            var multiPackets = msg.ToSendPackets(Unfloader.Protocol.Gebug.Parameter.ParameterUseDirection.PcToConsole);
            foreach (var p in multiPackets)
            {
                IUnfloaderPacket unfpacket = new BinaryPacket(p.ToByteArray());
                _sendQueue.Enqueue(unfpacket);
            }
        }

        public void SendMessage(IUnfloaderPacket packet)
        {
            _sendQueue.Enqueue(packet);
        }

        public Guid Subscribe(Action<IGebugMessage> callback, int listenCount = 0, Func<IGebugMessage, bool>? filter = null)
        {
            return _messageBusGebug.Subscribe(callback, listenCount, filter);
        }

        public Guid Subscribe(Action<IUnfloaderPacket> callback, int listenCount = 0, Func<IUnfloaderPacket, bool>? filter = null)
        {
            return _messageBusUnfloader.Subscribe(callback, listenCount, filter);
        }

        private void ThreadMain()
        {
            while (true)
            {
                bool anyWork = false;

                if (_stop)
                {
                    break;
                }

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

                    _flashcart.Send(sendPacket!.GetOuterPacket());
                }

                if (_stop)
                {
                    break;
                }

                while (ManagerActive && !_flashcart.ReadPackets.IsEmpty)
                {
                    IFlashcartPacket? receivePacket = null;

                    anyWork = true;

                    while (!_flashcart.ReadPackets.TryDequeue(out receivePacket))
                    {
                        if (_stop || _flashcart.ReadPackets.IsEmpty)
                        {
                            break;
                        }
                    }

                    if (_stop || object.ReferenceEquals(null, receivePacket))
                    {
                        break;
                    }

                    if (typeof(IUnfloaderPacket).IsAssignableFrom(receivePacket.InnerType))
                    {
                        IUnfloaderPacket unfPacket = (IUnfloaderPacket)receivePacket.InnerData!;

                        // if this is gebug ROM message, don't care about UNFLoader wrapper any more
                        var unfInnerData = unfPacket.GetInnerPacket();
                        var gebugParse = GebugPacket.TryParse(unfInnerData.ToList());

                        if (gebugParse.ParseStatus == PacketParseStatus.Success)
                        {
                            var packet = gebugParse.Packet;
                            if (packet!.TotalNumberPackets > 1)
                            {
                                _receiveMessageFragments.Add(packet);

                                if (_receiveMessageFragments.Count >= packet.TotalNumberPackets)
                                {
                                    var fragments = _receiveMessageFragments.Where(x =>
                                        x.Category == packet.Category
                                        && x.Command == packet.Command
                                        && x.TotalNumberPackets == packet.TotalNumberPackets)
                                    .OrderBy(x => x.PacketNumber)
                                    .ToList();

                                    if (fragments.Count == packet.TotalNumberPackets)
                                    {
                                        var gebugMessage = GebugMessage.FromPackets(fragments, Protocol.Gebug.Parameter.ParameterUseDirection.ConsoleToPc);
                                        _receiveMessages.Add(gebugMessage);

                                        foreach (var f in fragments)
                                        {
                                            _receiveMessageFragments.Remove(f);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var gebugMessage = GebugMessage.FromPacket(packet, Protocol.Gebug.Parameter.ParameterUseDirection.ConsoleToPc);
                                _receiveMessages.Add(gebugMessage);
                            }
                        }
                        else
                        {
                            _receiveUnfPackets.Add(unfPacket);
                        }
                    }
                    else
                    {
                        _receiveFlashcardPackets.Add(receivePacket);
                    }
                }

                if (_stop)
                {
                    break;
                }

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
