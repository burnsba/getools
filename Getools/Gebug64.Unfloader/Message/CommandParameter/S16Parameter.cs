using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib;
using Getools.Lib.Architecture;

namespace Gebug64.Unfloader.Message.CommandParameter
{
    public class S16Parameter : CommandParameterBase<Int16>
    {
        public S16Parameter()
        {
            Size = 2;
            UnderlyingType = typeof(Int16);
        }

        public S16Parameter(UInt16 value)
            : this()
        {
            Value = (Int16)value;
        }

        public S16Parameter(Int16 value)
            : this()
        {
            Value = value;
        }

        public override byte[] GetBytes(ByteOrder endienness)
        {
            var result = new byte[Size];

            if (endienness == ByteOrder.LittleEndien)
            {
                BitUtility.Insert16Little(result, 0, Value);
            }
            else
            {
                BitUtility.Insert16Big(result, 0, Value);
            }

            return result;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
