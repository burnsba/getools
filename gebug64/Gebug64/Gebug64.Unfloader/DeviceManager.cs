﻿using Gebug64.Unfloader.Message;
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

        public IFlashcart? Flashcart => _flashcart;

        public ConcurrentQueue<IGebugMessage> MessagesFromConsole => _receiveFromConsoleQueue;

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
            throw new NotImplementedException();
        }

        public void EnqueueMessage(IGebugMessage message)
        {
            _sendToConsoleQueue.Enqueue(message);
        }

        public void Init(string portName)
        {
            _flashcart!.Init(portName);
        }

        public bool Test()
        {
            return _flashcart!.Test();
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

                //_flashcart!.Read();
            }
        }
    }
}
