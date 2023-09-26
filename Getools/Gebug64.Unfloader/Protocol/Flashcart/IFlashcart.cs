using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    public interface IFlashcart : IDisposable
    {
        bool IsConnected { get; }
        ConcurrentQueue<IFlashcartPacket> ReadPackets { get; }

        void Connect(string port);
        void Disconnect();
        void SendRom(byte[] filedata, Nullable<CancellationToken> token = null);
        void Send(byte[] data);
        void Send(IFlashcartPacket packet);
        bool TestInMenu();
    }
}
