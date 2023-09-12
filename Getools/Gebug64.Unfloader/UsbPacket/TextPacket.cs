using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Gebug64.Unfloader.UsbPacket
{
    public class TextPacket : Packet
    {
        public string? Content
        {
            get
            {
                if (!object.ReferenceEquals(null, _data))
                {
                    return Encoding.ASCII.GetString(_data);
                }

                return null;
            }

            set
            {
                if (value == null)
                {
                    _data = null;
                }
                else
                {
                    _data = Encoding.ASCII.GetBytes(value);
                }
            }
        }

        public TextPacket(byte[] data)
            : base(PacketType.Text, data)
        { }

        public override string ToString()
        {
            return $"size={Size}, type={DataType}, text={Content}";
        }
    }
}
