using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    public class StandFilePoint
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }

        public short Link { get; set; }

        public string ToCInlineDeclaration(string prefix = "")
        {
            return $"{prefix}{{{X}, {Y}, {Z}, 0x{Link:x4}}}";
        }

        public byte[] ToByteArray()
        {
            var results = new byte[8];

            BitUtility.InsertShortBig(results, 0, X);
            BitUtility.InsertShortBig(results, 2, Y);
            BitUtility.InsertShortBig(results, 4, Z);
            BitUtility.InsertShortBig(results, 6, Link);

            return results;
        }

        public void AppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToByteArray();
            stream.Write(bytes);
        }
    }
}
