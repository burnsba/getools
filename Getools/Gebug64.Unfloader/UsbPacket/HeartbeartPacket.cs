using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.UsbPacket
{
    public class HeartbeartPacket : Packet
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
            : base(PacketType.HeartBeat, data)
        { }

        public override string ToString()
        {
            return $"size={Size}, type={DataType}, protocol={UsbProtocolVersion}, heartbeat={HeartbeatVersion}";
        }
    }
}
