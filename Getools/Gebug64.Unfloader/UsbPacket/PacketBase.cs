using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.UsbPacket
{
    public abstract class PacketBase : IPacket
    {
        // data without protocol info
        protected byte[]? _data;

        public PacketType DataType { get; set; }

        public int Size { get; set; }

        public PacketBase() { }

        public PacketBase(PacketType dataType, byte[] data)
        {
            DataType = dataType;
            Size = data?.Length ?? 0;
            _data = data;
        }

        public byte[]? GetInnerData() => _data;

        public virtual byte[]? GetOuterData() => _data;

        public override string ToString()
        {
            return $"size={Size}, type={DataType}";
        }
    }
}
