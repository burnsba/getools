using Gebug64.Unfloader.Message;
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
        private object _lock = new object();
        private Thread? _thread = null;
        private IFlashcart? _flashcart = null;
        private bool _stop = true;
        private ConcurrentQueue<IGebugMessage> _sendToConsoleQueue = new ConcurrentQueue<IGebugMessage>();
        private ConcurrentQueue<IGebugMessage> _receiveFromConsoleQueue = new ConcurrentQueue<IGebugMessage>();

        public IFlashcart? Flashcart => _flashcart;

        public ConcurrentQueue<IGebugMessage> MessagesFromConsole => _receiveFromConsoleQueue;

        public DeviceManager(IFlashcart flashcart)
        {
            _flashcart = flashcart;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void EnqueueMessage(IGebugMessage message)
        {
            _sendToConsoleQueue.Enqueue(message);
        }

        public void Init()
        {
            _flashcart.Init();
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

            _flashcart.Disconnect();
        }

        private void ThreadMain()
        {
            while (true)
            {
                if (_stop)
                {
                    break;
                }

                _flashcart.Read();
            }
        }
    }
}
