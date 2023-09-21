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
            var multiPackets = msg.ToSendPackets();
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

        Guid Subscribe(Action<IGebugMessage> callback, int listenCount = 0, Func<IGebugMessage, bool>? filter = null)
        {
            return _messageBusGebug.Subscribe(callback, listenCount, filter);
        }

        Guid Subscribe(Action<IUnfloaderPacket> callback, int listenCount = 0, Func<IUnfloaderPacket, bool>? filter = null)
        {
            return _messageBusUnfloader.Subscribe(callback, listenCount, filter);
        }

        private void ThreadMain()
        {
            while (true)
            {
                if (_stop)
                {
                    break;
                }

                while (false && !_sendQueue.IsEmpty)
                {
                    IUnfloaderPacket sendPacket;

                    while (!_sendQueue.TryDequeue(out sendPacket))
                    {
                        if (_stop || _sendQueue.IsEmpty)
                        {
                            break;
                        }
                    }

                    if (_stop || _sendQueue.IsEmpty)
                    {
                        break;
                    }

                    _flashcart.Send(sendPacket.GetOuterPacket());
                }

                if (_stop)
                {
                    break;
                }

                while (false && !_flashcart.ReadPackets.IsEmpty)
                {
                    IFlashcartPacket receivePacket;

                    while (!_flashcart.ReadPackets.TryDequeue(out receivePacket))
                    {
                        if (_stop || _flashcart.ReadPackets.IsEmpty)
                        {
                            break;
                        }
                    }

                    if (_stop || _flashcart.ReadPackets.IsEmpty)
                    {
                        break;
                    }

                    // don't care about flashcart wrapper any more
                    var flashcartInnerData = receivePacket.GetInnerPacket();
                    var unfParse = UnfloaderPacket.TryParse(flashcartInnerData.ToList());

                    if (unfParse.ParseStatus == PacketParseStatus.Success)
                    {
                        // if this is rom message, don't care about UNFLoader wrapper any more
                        var unfInnerData = unfParse.Packet!.GetInnerPacket();
                        var gebugParse = GebugPacket.TryParse(unfInnerData.ToList());

                        if (gebugParse.ParseStatus == PacketParseStatus.Success)
                        {
                            var packet = gebugParse.Packet;
                            if (packet.TotalNumberPackets > 1)
                            {
                                _receiveMessageFragments.Add(packet);

                                if (_receiveMessageFragments.Count >= packet.TotalNumberPackets)
                                {
                                    var gebugMessage = GebugMessage.FromPackets(_receiveMessageFragments);
                                    _receiveMessages.Add(gebugMessage);

                                    _receiveMessageFragments.Clear();
                                }
                            }
                            else
                            {
                                var gebugMessage = GebugMessage.FromPacket(packet);
                                _receiveMessages.Add(gebugMessage);
                            }
                        }
                        else
                        {
                            _receiveUnfPackets.Add(unfParse.Packet!);
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

                foreach (var message in _receiveUnfPackets)
                {
                    _messageBusUnfloader.Publish(message);

                    if (_stop)
                    {
                        break;
                    }
                }

                _receiveUnfPackets.Clear();

                foreach (var message in _receiveMessages)
                {
                    _messageBusGebug.Publish(message);

                    if (_stop)
                    {
                        break;
                    }
                }

                _receiveMessages.Clear();
            }
        }
    }
}
