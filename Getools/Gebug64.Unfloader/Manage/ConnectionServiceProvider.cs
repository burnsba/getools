﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public class ConnectionServiceProvider : IConnectionServiceProvider
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

        public bool IsShutdown => object.ReferenceEquals(null, _thread) || !_thread.IsAlive;

        public TimeSpan SinceDataReceived => _flashcart?.SinceDataReceived ?? TimeSpan.MaxValue;

        /// <inheritdoc />
        public TimeSpan SinceRomMessageReceived => _flashcart?.SinceRomMessageReceived ?? TimeSpan.MaxValue;

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

            _receiveFlashcardPackets.Clear();
            _receiveUnfPackets.Clear();
            _receiveMessageFragments.Clear();
            _receiveMessages.Clear();
            _sendQueue.Clear();
        }

        public bool TestInMenu() { return _flashcart.TestInMenu(); }

        public bool TestInRom()
        {
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

                return message.Category == GebugMessageCategory.Meta
                    && message.Command == (int)GebugCmdMeta.Ping
                    && (firstPacket.Flags & (ushort)GebugMessageFlags.IsAck) > 0
                    && message.AckId == sendMessageId
                    ;
            };

            _messageBusGebug.Subscribe(callback, 1, filter);

            SendMessage(sendMesssage);

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

        public Guid Subscribe(Action<IGebugMessage> callback, int listenCount = 0, Func<IGebugMessage, bool>? filter = null)
        {
            return _messageBusGebug.Subscribe(callback, listenCount, filter);
        }

        public Guid Subscribe(Action<IUnfloaderPacket> callback, int listenCount = 0, Func<IUnfloaderPacket, bool>? filter = null)
        {
            return _messageBusUnfloader.Subscribe(callback, listenCount, filter);
        }

        public void GebugUnsubscribe(Guid id)
        {
            _messageBusGebug.Unsubscribe(id);
        }

        public void UnfloaderUnsubscribe(Guid id)
        {
            _messageBusUnfloader.Unsubscribe(id);
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
