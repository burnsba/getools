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
    }
}
