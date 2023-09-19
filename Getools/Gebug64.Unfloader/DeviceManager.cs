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
    public class DeviceManager : IDeviceManager
    {
        private object _lock = new object();
        private Thread? _thread = null;
        private readonly IFlashcart? _flashcart = null;
        private bool _stop = true;
        private ConcurrentQueue<IGebugMessage> _sendToConsoleQueue = new ConcurrentQueue<IGebugMessage>();
        private ConcurrentQueue<IGebugMessage> _receiveFromConsoleQueue = new ConcurrentQueue<IGebugMessage>();

        private List<RomAckMessage> _receiveFragments = new List<RomAckMessage>();

        private Mutex _priorityLock = new Mutex();

        private MessageBus<IGebugMessage> _messageBus = new MessageBus<IGebugMessage>();

        public IFlashcart? Flashcart => _flashcart;

        public ConcurrentQueue<IGebugMessage> SendToConsole => _sendToConsoleQueue;

        public bool IsShutdown => object.ReferenceEquals(null, _thread) || !_thread.IsAlive;

        public TimeSpan SinceDataReceived => _flashcart?.SinceDataReceived ?? TimeSpan.MaxValue;

        public TimeSpan SinceRomMessageReceived => _flashcart?.SinceRomMessageReceived ?? TimeSpan.MaxValue;

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
            if (!_priorityLock.WaitOne(1000))
            {
                return false;
            }

            // Send the test command.
            var sendMsg = new RomMetaMessage(Unfloader.Message.MessageType.GebugCmdMeta.Ping) { Source = CommunicationSource.Pc };
            _flashcart.Send(sendMsg);

            // Wait a short while for the device to respond.
            System.Threading.Thread.Sleep(100);

            IGebugMessage msg;

            // If there's no response, the test failed.
            if (_flashcart!.MessagesFromConsole.IsEmpty)
            {
                _priorityLock.ReleaseMutex();
                return false;
            }

            // Else, get the next message response.
            while (!_flashcart!.MessagesFromConsole.TryDequeue(out msg))
            {
                ;
            }

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
            _flashcart!.MessagesFromConsole.Enqueue(msg);
            _priorityLock.ReleaseMutex();
            return false;
        }

        public bool TestFlashcartConnected()
        {
            if (!_priorityLock.WaitOne(1000))
            {
                return false;
            }

            // Send the test command.
            ((Flashcart.FlashcartBase)_flashcart)!.SendTest();

            // Wait a short while for the device to respond.
            System.Threading.Thread.Sleep(100);

            IGebugMessage msg;

            // If there's no response, the test failed.
            if (_flashcart!.MessagesFromConsole.IsEmpty)
            {
                _priorityLock.ReleaseMutex();
                return false;
            }

            // Else, get the next message response.
            while (!_flashcart!.MessagesFromConsole.TryDequeue(out msg))
            {
                ;
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

            var sw = Stopwatch.StartNew();

            if (!object.ReferenceEquals(null, _thread))
            {
                while (_thread.IsAlive)
                {
                    System.Threading.Thread.Sleep(10);

                    if (sw.Elapsed.TotalSeconds > 5)
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
            return _messageBus.Subscribe(callback, listenCount, filter);
        }

        public void Unsubscribe(Guid id)
        {
            _messageBus.Unsubscribe(id);
        }

        private void ThreadMain()
        {
            while (true)
            {
                if (_stop)
                {
                    break;
                }

                if (_flashcart!.MessagesFromConsole.Any() && _priorityLock.WaitOne(10))
                {
                    while (true)
                    {
                        if (_stop || !_flashcart!.MessagesFromConsole.Any())
                        {
                            break;
                        }

                        IGebugMessage msg;
                        while (!_flashcart!.MessagesFromConsole.TryDequeue(out msg))
                        {
                            if (_stop || !_flashcart!.MessagesFromConsole.Any())
                            {
                                break;
                            }
                        }

                        if (!object.ReferenceEquals(null, msg))
                        {
                            var pending = msg as PendingGebugMessage;

                            // try to parse as gebug rom message.
                            RomMessage romMessage = null;
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

                                if (romMessage.Category == Message.MessageType.GebugMessageCategory.Ack)
                                {
                                    var ackMessage = (RomAckMessage)romMessage;
                                    if (ackMessage.TotalNumberPackets > 1)
                                    {
                                        queued = true;
                                        _receiveFragments.Add(ackMessage);
                                    }

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

                                if (!queueDone)
                                {
                                    _receiveFromConsoleQueue.Enqueue(msg);
                                }
                            }
                        }
                    }

                    _priorityLock.ReleaseMutex();
                }

                if (_priorityLock.WaitOne(10))
                {
                    while (!_receiveFromConsoleQueue.IsEmpty)
                    {
                        IGebugMessage msg;
                        while (!_receiveFromConsoleQueue.TryDequeue(out msg))
                        {
                            if (_stop)
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
