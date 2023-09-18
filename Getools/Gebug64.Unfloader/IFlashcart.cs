using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message;

namespace Gebug64.Unfloader
{
    public interface IFlashcart : IDisposable
    {
        TimeSpan SinceDataReceived { get; }

        TimeSpan SinceRomMessageReceived { get; }

        ConcurrentQueue<IGebugMessage> MessagesFromConsole { get; }

        void Init(string portName);

        void Send(IGebugMessage message);

        void Disconnect();

        void SendRom(byte[] filedata, Nullable<CancellationToken> token = null);
    }
}
