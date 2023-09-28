using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gebug64.Unfloader.Protocol.Unfloader.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Unfloader.Message
{
    public class BinaryPacket : UnfloaderPacket
    {
        public BinaryPacket(byte[] data)
            : base(UnfloaderMessageType.Binary, data)
        { }

        public override string ToString()
        {
            return $"size={Size}, type={MessageType}";
        }
    }
}
