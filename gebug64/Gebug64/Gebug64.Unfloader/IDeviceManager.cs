
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message;

namespace Gebug64.Unfloader
{
    public interface IDeviceManager : IDisposable
    {
        IFlashcart? Flashcart { get; }

        void EnqueueMessage(IGebugMessage message);

        ConcurrentQueue<IGebugMessage> MessagesFromConsole { get; }

        void Init(string portName);

        void Start();

        void Stop();

        void SendRom(string path);
    }
}
