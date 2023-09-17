using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.CommandParameter
{
    public class U8ArrayParameter : CommandParameterBase<byte[]>
    {
        public U8ArrayParameter()
        {
            Size = 0;
            UnderlyingType = typeof(byte[]);
        }

        public U8ArrayParameter(byte[] value)
        {
            Value = new byte[value.Length];
            Array.Copy(value, Value, value.Length);

            Size = value.Length;
            UnderlyingType = typeof(byte[]);
        }

        public override byte[] GetBytes(Endianness endianness)
        {
            return Value;
        }

        public override string ToString()
        {
            return "byte[]";
        }
    }
}
