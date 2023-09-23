using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Unfloader.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Unfloader
{
    public interface IUnfloaderPacket : IEncapsulate
    {
        UnfloaderMessageType MessageType { get; }
        int Size { get; }

        void SetContent(byte[] body);

        byte[] GetInnerPacket();
        byte[] GetOuterPacket();
    }
}
