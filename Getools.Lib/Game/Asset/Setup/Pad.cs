using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pad.
    /// </summary>
    public class Pad
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct pad";

        /// <summary>
        /// Size of the point struct in bytes.
        /// </summary>
        public const int SizeOf = 44;

        public Pad()
        {
        }

        public Coord3df Position { get; set; }

        public Coord3df Up { get; set; }

        public Coord3df Look { get; set; }

        public StringPointer Name { get; set; }

        public int NameRodataOffset { get; set; }

        public int Unknown { get; set; }

        public static Pad ReadFromBinFile(BinaryReader br)
        {
            var result = new Pad();

            result.Position = Coord3df.ReadFromBinFile(br);
            result.Up = Coord3df.ReadFromBinFile(br);
            result.Look = Coord3df.ReadFromBinFile(br);

            result.NameRodataOffset = BitUtility.Read16Big(br);
            result.Unknown = BitUtility.Read32Big(br);

            return result;
        }
    }
}
