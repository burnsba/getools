using Gebug64.Unfloader.Message;
using Gebug64.Unfloader.UsbPacket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader
{
    public class DeviceManager : IDeviceManager
    {
        private bool _isInit = false;
        private object _lock = new object();
        private Thread? _thread = null;
        private readonly IFlashcart? _flashcart = null;
        private bool _stop = true;
        private ConcurrentQueue<IGebugMessage> _sendToConsoleQueue = new ConcurrentQueue<IGebugMessage>();
        private ConcurrentQueue<IGebugMessage> _receiveFromConsoleQueue = new ConcurrentQueue<IGebugMessage>();
        private ConcurrentQueue<IGebugMessage> _emptyQueue = new ConcurrentQueue<IGebugMessage>();

        private bool _priorityLock = false;

        public IFlashcart? Flashcart => _flashcart;

        public ConcurrentQueue<IGebugMessage> MessagesFromConsole => _priorityLock ? _emptyQueue : _receiveFromConsoleQueue;

        public ConcurrentQueue<IGebugMessage> SendToConsole => _sendToConsoleQueue;

        public bool IsShutdown => object.ReferenceEquals(null, _thread) || !_thread.IsAlive;

        public DeviceManager(IFlashcart flashcart)
        {
            if (object.ReferenceEquals(null, flashcart))
            {
                throw new NullReferenceException(nameof(flashcart));
            }

            _flashcart = flashcart;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _stop = true;
            }

            if (!object.ReferenceEquals(null, _flashcart))
            {
                _flashcart!.Disconnect();
                _flashcart.Dispose();
            }
        }

        public void EnqueueMessage(IGebugMessage message)
        {
            _sendToConsoleQueue.Enqueue(message);
        }

        public void Init(string portName)
        {
            _flashcart!.Init(portName);
        }

        public bool TestRomConnected()
        {
            _priorityLock = true;

            // Send the test command.
            var sendMsg = new RomMetaMessage(Unfloader.Message.MessageType.GebugCmdMeta.Ping) { Source = CommunicationSource.Pc };
            _flashcart.Send(sendMsg);

            // Wait a short while for the device to respond.
            System.Threading.Thread.Sleep(500);

            IGebugMessage msg;

            // If there's no response, the test failed.
            if (_receiveFromConsoleQueue.IsEmpty)
            {
                _priorityLock = false;
                return false;
            }

            // Else, get the next message response.
            while (!_receiveFromConsoleQueue.TryDequeue(out msg))
            {
                ;
            }

            if (msg != null && msg is RomMessage romMessage)
            {
                if (romMessage.Category == Unfloader.Message.MessageType.GebugMessageCategory.Ack)
                {
                    var ackMessage = (RomAckMessage)romMessage;

                    if (ackMessage.AckCategory == Unfloader.Message.MessageType.GebugMessageCategory.Meta
                        && ackMessage.AckCommand == (int)Unfloader.Message.MessageType.GebugCmdMeta.Ping)
                    {
                        // success
                        _priorityLock = false;
                        return true;
                    }
                }
            }

            // Otherwise, put the message back on the queue for anyone else.
            _receiveFromConsoleQueue.Enqueue(msg);
            _priorityLock = false;
            return false;
        }

        public bool TestFlashcartConnected()
        {
            _priorityLock = true;

            // Send the test command.
            ((Flashcart.FlashcartBase)_flashcart)!.SendTest();

            // Wait a short while for the device to respond.
            System.Threading.Thread.Sleep(100);

            IGebugMessage msg;

            // If there's no response, the test failed.
            if (_receiveFromConsoleQueue.IsEmpty)
            {
                _priorityLock = false;
                return false;
            }

            // Else, get the next message response.
            while (!_receiveFromConsoleQueue.TryDequeue(out msg))
            {
                ;
            }

            // Check the device specific response format. If this is valid,
            // then we're done.
            if (((Flashcart.FlashcartBase)_flashcart)!.IsTestCommandResponse(msg.GetUsbPacket()))
            {
                _priorityLock = false;
                return true;
            }

            // Otherwise, put the message back on the queue for anyone else.
            _receiveFromConsoleQueue.Enqueue(msg);
            _priorityLock = false;
            return false;
        }

        public void Start()
        {
            lock (_lock)
            {
                _stop = false;
            }

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

        public void Stop()
        {
            lock (_lock)
            {
                _stop = true;
            }

            _flashcart!.Disconnect();
        }

        public void SendRom(string path)
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

            _flashcart!.SendRom(filedata);
        }

        private void ThreadMain()
        {
            while (true)
            {
                if (_stop)
                {
                    break;
                }

                if (_flashcart!.HasReadData)
                {
                    var bytes = _flashcart.Read()!;
                    int offset = 0;

                    while (true)
                    {
                        if (_stop)
                        {
                            break;
                        }

                        Packet? pp = null;
                        var parseResult = Packet.Unwrap(bytes, out pp);

                        if (parseResult == PacketParseResult.Success)
                        {
                            var packetBytes = pp.GetData();

                            // try to parse as gebug rom message.
                            RomMessage romMessage;
                            var romMessageParseResult = RomMessage.Parse(packetBytes, out romMessage);

                            if (romMessageParseResult == RomMessageParseResult.Success)
                            {
                                romMessage!.Source = CommunicationSource.N64;
                                romMessage.InstantiateTime = DateTime.Now;

                                _receiveFromConsoleQueue.Enqueue(romMessage);
                            }
                            else
                            {
                                var msg = new DebugMessage(pp!)
                                {
                                    Source = CommunicationSource.N64,
                                    InstantiateTime = DateTime.Now,
                                };

                                _receiveFromConsoleQueue.Enqueue(msg);
                            }

                            offset += pp!.Size + Packet.ProtocolByteLength;
                        }
                        else if (!object.ReferenceEquals(null, pp))
                        {
                            var packetBytes = pp.GetData();

                            if (packetBytes == null || packetBytes!.All(x => x == 0))
                            {
                                // ignore this, zero pad ending of packet.
                            }
                            else
                            {
                                // try to parse as gebug rom message.
                                RomMessage romMessage;
                                var romMessageParseResult = RomMessage.Parse(packetBytes, out romMessage);

                                if (romMessageParseResult == RomMessageParseResult.Success)
                                {
                                    romMessage!.Source = CommunicationSource.N64;
                                    romMessage.InstantiateTime = DateTime.Now;

                                    _receiveFromConsoleQueue.Enqueue(romMessage);
                                }
                                else
                                {
                                    var msg = new DebugMessage(pp)
                                    {
                                        Source = CommunicationSource.N64,
                                        InstantiateTime = DateTime.Now,
                                    };

                                    _receiveFromConsoleQueue.Enqueue(msg);
                                }

                                offset += pp.Size;
                            }
                        }
                        else
                        {
                            // Should only happen with a zero length packet.
                            break;
                        }

                        if (offset >= bytes.Length)
                        {
                            break;
                        }

                        bytes = bytes.Skip(offset).ToArray();
                    }
                }

                while (!_sendToConsoleQueue.IsEmpty)
                {
                    IGebugMessage msg;
                    while (!_sendToConsoleQueue.TryDequeue(out msg))
                    {
                        if (_stop)
                        {
                            break;
                        }
                    }

                    if (object.ReferenceEquals(null, msg))
                    {
                        throw new NullReferenceException();
                    }

                    _flashcart.Send(msg);

                    if (_stop)
                    {
                        break;
                    }
                }
            }
        }
    }
}
