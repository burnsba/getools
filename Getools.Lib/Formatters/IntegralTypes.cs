using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Getools.Lib.Formatters
{
    /// <summary>
    /// Format converters for integers, pointers, and bytes.
    /// </summary>
    public static class IntegralTypes
    {
        /// <summary>
        /// Converts value as it would be printed in c source.
        /// Converted to 8-digit hex value, prepended with "0x" if value is not null.
        /// Otherwise returns "NULL".
        /// </summary>
        /// <param name="i">Value to use.</param>
        /// <returns>Value as hex or "NULL".</returns>
        public static string ToCPointerString(int? i)
        {
            if (i.HasValue)
            {
                var ui = (UInt32)i;
                return $"0x{i:x8}";
            }

            return "NULL";
        }

        /// <summary>
        /// Converts array of bytes to c byte array inline declaraction.
        /// Each value is converted to hex string.
        /// </summary>
        /// <param name="bytes">Bytes to use in array.</param>
        /// <returns>Comma seperated list of values within brackets.</returns>
        public static string ToCInlineByteArray(IEnumerable<byte> bytes)
        {
            var byteString = string.Join(", ", bytes.Select(x => "0x" + x.ToString("x2")));

            return "{" + byteString + "}";
        }

        /// <summary>
        /// Converts single byte to c char inline declaration.
        /// If the byte is outside the printable ASCII range (32-127 inclusive),
        /// the byte is returned as hex value.
        /// </summary>
        /// <param name="b">Byte to convert.</param>
        /// <returns>Single quoted byte value if within printable ASCII range,
        /// otherwise hex value.</returns>
        public static string ByteAsCharStringOrHex(byte b)
        {
            if (b >= 32 && b <= 127)
            {
                return $"'{(char)b}'";
            }
            else
            {
                return $"0x{b:x2}";
            }
        }

        /// <summary>
        /// Converts a string to a fixed length array of single characters
        /// as it would appear in an inline c declaration.
        /// If the array length exceeds the string length, null characters are
        /// used to fill the remainder of the array. The string is truncated
        /// if it exceeds array length, without consideration for terminating
        /// null character.
        /// </summary>
        /// <param name="s">String to convert to char array.</param>
        /// <param name="arrLen">Length of array to create.</param>
        /// <returns>Inline c declaration text.</returns>
        public static string StringToCInlineFixedLengthCharArray(string s, int arrLen)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(s);
            var fixedArr = new byte[arrLen];
            Array.Copy(bytes, fixedArr, Math.Min(arrLen, bytes.Length));
            var charTexts = fixedArr.Select(x => ByteAsCharStringOrHex(x));

            return "{" + string.Join(", ", charTexts) + "}";
        }
    }
}
