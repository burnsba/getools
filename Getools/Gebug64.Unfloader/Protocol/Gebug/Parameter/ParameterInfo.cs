using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Parameter
{
    /// <summary>
    /// Meta data to be included with parameter in message body.
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// Magic byte escape value for variable length parameters,
        /// where the length can be desribed in one byte.
        /// </summary>
        public const byte Protocol_VariableParameterLength_U8Prefix = 0xff;

        /// <summary>
        /// Magic byte escape value for variable length parameters,
        /// where the length can be desribed in two bytes.
        /// </summary>
        public const byte Protocol_VariableParameterLength_U16Prefix = 0xfe;

        /// <summary>
        /// Magic byte escape value for variable length parameters,
        /// where the length can be desribed in four bytes.
        /// </summary>
        public const byte Protocol_VariableParameterLength_U32Prefix = 0xfd;

        /// <summary>
        /// For a given parameter, gets the length prefix according to the spec.
        /// </summary>
        /// <param name="value">Variable length parameter (property).</param>
        /// <returns>Byte array containing length prefix (length is in big endien format).</returns>
        public static byte[] GetParameterPrefix(object value)
        {
            var length = (UInt32)GetCollectionLength(value);

            if (length <= byte.MaxValue)
            {
                return new byte[]
                {
                    Protocol_VariableParameterLength_U8Prefix,
                    (byte)length,
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

        /// <summary>
        /// Converts parameter to explicit type and gets the collection length.
        /// </summary>
        /// <param name="value">Parameter value (property instance).</param>
        /// <returns>Length of collection.</returns>
        /// <exception cref="NotSupportedException">Throws if can't resolve to known type.</exception>
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
