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
        void Init();

        void Send(IGebugMessage message);

        void ProcessData();

        void Disconnect();

        Queue<IGebugMessage> MessagesFromConsole { get; }
    }
}
