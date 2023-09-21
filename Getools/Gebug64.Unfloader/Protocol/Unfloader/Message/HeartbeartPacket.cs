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
    internal class HeartbeartPacket : UnfloaderPacket
    {
        public int UsbProtocolVersion
        {
            get
            {
                if (object.ReferenceEquals(null, _data) || _data.Count() < 2)
                {
                    return 0;
                }

                int val = _data[0] << 8;
                val |= _data[1];

                return val;
            }
        }

        public int HeartbeatVersion
        {
            get
            {
                if (object.ReferenceEquals(null, _data) || _data.Count() < 4)
                {
                    return 0;
                }

                int val = _data[2] << 8;
                val |= _data[3];

                return val;
            }
        }

        public HeartbeartPacket(byte[] data)
            : base(UnfloaderMessageType.HeartBeat, data)
        { }

        public override string ToString()
        {
            return $"size={Size}, type={MessageType}, protocol={UsbProtocolVersion}, heartbeat={HeartbeatVersion}";
        }
    }
}
