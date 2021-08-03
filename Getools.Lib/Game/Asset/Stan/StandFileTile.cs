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
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{Flags:x1},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{R:x1}, 0x{G:x1}, 0x{B:x1},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{PointCount},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{HeaderC:x1}, 0x{HeaderD:x1}, 0x{HeaderE:x1}");
            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        public static StandFileTile ReadFromBinFile(BinaryReader br, int tileIndex)
        {
            var result = new StandFileTile();

            Byte b;

            b = br.ReadByte();
            result.InternalName = b << 16;
            b = br.ReadByte();
            result.InternalName |= b << 8;
            b = br.ReadByte();
            result.InternalName |= b;

            result.Room = br.ReadByte();

            // "Tile beginning with room 0 is the true way the file format ends, engine does not check for unstric string"
            if (result.Room == 0)
            {
                br.BaseStream.Seek(-4, SeekOrigin.Current);
                throw new Error.ExpectedStreamEndException();
            }

            b = br.ReadByte();
            result.Flags = (byte)((b >> 4) & 0xf);
            result.R = (byte)((b) & 0xf);

            b = br.ReadByte();
            result.G = (byte)((b >> 4) & 0xf);
            result.B = (byte)((b) & 0xf);

            b = br.ReadByte();
            result.PointCount = (byte)((b >> 4) & 0xf);
            result.HeaderC = (byte)((b) & 0xf);

            if (result.PointCount < 1)
            {
                throw new Exception("Tile is defined with zero points");
            }

            b = br.ReadByte();
            result.HeaderD = (byte)((b >> 4) & 0xf);
            result.HeaderE = (byte)((b) & 0xf);

            result.Name = $"{Config.Stan.DefaultDeclarationName_StandFileTile}_{tileIndex:X}";
            result.OrderId = tileIndex;

            return result;
        }
    }
}
