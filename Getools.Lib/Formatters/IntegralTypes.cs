using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Getools.Lib.Formatters
{
    public static class IntegralTypes
    {
        public static string ToCPointerString(int? i)
        {
            if (i.HasValue)
            {
                var ui = (UInt32)i;
                return $"0x{i:x8}";
            }

            return "NULL";
        }

        public static string ToCInlineByteArray(IEnumerable<byte> bytes)
        {
            var byteString = string.Join(", ", bytes.Select(x => "0x" + x.ToString("x2")));

            return "{" + byteString + "}";
        }

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
