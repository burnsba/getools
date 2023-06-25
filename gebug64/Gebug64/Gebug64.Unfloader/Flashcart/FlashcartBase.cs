using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message;

namespace Gebug64.Unfloader.Flashcart
{
    public abstract class FlashcartBase : IFlashcart
    {
        protected SerialPort? _serialPort = null;
        protected Queue<byte> _readQueue = new Queue<byte>();

        public Queue<IGebugMessage> MessagesFromConsole => throw new NotImplementedException();

        public abstract void ProcessData();

        public abstract void Send(IGebugMessage message);

        public void Init()
        {
            _serialPort = new SerialPort();
            _serialPort.DataReceived += DataReceived;
            _serialPort.Open();
        }

        public void Disconnect()
        {
            if (!object.ReferenceEquals(null, _serialPort))
            {
                _serialPort.DataReceived -= DataReceived;

                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }

            _serialPort = null;
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void DataReceived(object s, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[_serialPort.BytesToRead];
            _serialPort.Read(data, 0, data.Length);
            data.ToList().ForEach(b => _readQueue.Enqueue(b));
        }
    }
}
