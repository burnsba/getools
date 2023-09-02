using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message;

namespace Gebug64.Unfloader
{
    public interface IFlashcart : IDisposable
    {
        void Init(string portName);

        void Send(IGebugMessage message);

        void ProcessData();

        void Disconnect();

        bool Test();

        byte[]? Read();

        void SendRom(byte[] filedata);

        Queue<IGebugMessage> MessagesFromConsole { get; }

        bool HasReadData { get; }
    }
}
