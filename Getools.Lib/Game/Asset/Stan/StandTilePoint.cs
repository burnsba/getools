using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    public class StandTilePoint
    {
        // in bytes
        public const int SizeOf = 8;
        public const int BetaSizeOf = 16;

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public int Link { get; set; }

        public string ToCInlineDeclaration(string prefix = "")
        {
            return $"{prefix}{{{(short)X}, {(short)Y}, {(short)Z}, 0x{(short)Link:x4}}}";
        }

        public byte[] ToByteArray()
        {
            var results = new byte[SizeOf];

            BitUtility.InsertShortBig(results, 0, (short)X);
            BitUtility.InsertShortBig(results, 2, (short)Y);
            BitUtility.InsertShortBig(results, 4, (short)Z);
            BitUtility.InsertShortBig(results, 6, (short)Link);

            return results;
        }

        public byte[] ToBetaByteArray()
        {
            var results = new byte[BetaSizeOf];

            BitUtility.Insert32Big(results, 0, (int)X);
            BitUtility.Insert32Big(results, 4, (int)Y);
            BitUtility.Insert32Big(results, 8, (int)Z);
            BitUtility.Insert32Big(results, 12, (int)Link);

            return results;
        }

        public void AppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToByteArray();
            stream.Write(bytes);
        }

        public static StandTilePoint ReadFromBinFile(BinaryReader br)
        {
            var result = new StandTilePoint();

            result.X = BitUtility.Read16Big(br);
            result.Y = BitUtility.Read16Big(br);
            result.Z = BitUtility.Read16Big(br);
            result.Link = BitUtility.Read16Big(br);

            return result;
        }

        public static StandTilePoint ReadFromBetaBinFile(BinaryReader br)
        {
            var result = new StandTilePoint();

            result.X = BitUtility.Read32Big(br);
            result.Y = BitUtility.Read32Big(br);
            result.Z = BitUtility.Read32Big(br);
            result.Link = BitUtility.Read32Big(br);

            return result;
        }
    }
}
