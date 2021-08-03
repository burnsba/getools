using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace Getools.Lib.Game.Asset.Stan
{
    public class StandFileTile
    {
        /// <summary>
        /// 24 bits.
        /// </summary>
        public int InternalName { get; set; }
        public byte Room { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte Flags { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte R { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte G { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte B { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte PointCount { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte HeaderC { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte HeaderD { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte HeaderE { get; set; }

        /// <summary>
        /// C declaration name.
        /// </summary>
        public string Name { get; set; }

        public int OrderId { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public byte[] ToByteArray()
        {
            var results = new byte[8];

            BitUtility.InsertLower24Big(results, 0, InternalName);
            results[3] = Room;

            results[4] = (byte)(((Flags & 0xf) << 4) | (R & 0xf));
            results[5] = (byte)(((G & 0xf) << 4) | (B & 0xf));
            results[6] = (byte)(((PointCount & 0xf) << 4) | (HeaderC & 0xf));
            results[7] = (byte)(((HeaderD & 0xf) << 4) | (HeaderE & 0xf));

            return results;
        }

        public void AppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToByteArray();
            stream.Write(bytes);
        }

        public string ToCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{Config.Stan.TileCTypeName} {Name} = {{");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{InternalName:x6}, 0x{Room:x2},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{Flags:x2},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{R:x2}, 0x{G:x2}, 0x{B:x2},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{PointCount:x2},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{HeaderC:x2}, 0x{HeaderD:x2}, 0x{HeaderE:x2}");
            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }
    }
}
