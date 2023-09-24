using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Parameter
{
    public class ParameterInfo
    {
        public const byte Protocol_VariableParameterLength_U8Prefix = 0xff;
        public const byte Protocol_VariableParameterLength_U16Prefix = 0xfe;
        public const byte Protocol_VariableParameterLength_U32Prefix = 0xfd;

        public static byte[] GetParameterPrefix(object value)
        {
            var length = (UInt32)GetCollectionLength(value);

            if (length <= byte.MaxValue)
            {
                return new byte[]
                {
                    Protocol_VariableParameterLength_U8Prefix,
                    (byte)length
                };
            }
            else if (length <= ushort.MaxValue)
            {
                return new byte[]
                {
                    Protocol_VariableParameterLength_U16Prefix,
                    (byte)(length >> 8),
                    (byte)(length >> 0),
                };
            }
            else
            {
                return new byte[]
                {
                    Protocol_VariableParameterLength_U32Prefix,
                    (byte)(length >> 24),
                    (byte)(length >> 16),
                    (byte)(length >> 8),
                    (byte)(length >> 0),
                };
            }
        }

        private static int GetCollectionLength(object value)
        {
            var type = value.GetType();

            if (type == typeof(byte[]))
            {
                return ((byte[])value).Length;
            }
            else if (type == typeof(List<byte>))
            {
                return ((List<byte>)value).Count;
            }
            else if (type == typeof(IList<byte>))
            {
                return ((IList<byte>)value).Count;
            }
            else if (type == typeof(IEnumerable<byte>))
            {
                return ((IEnumerable<byte>)value).Count();
            }

            throw new NotSupportedException();
        }
    }
}
