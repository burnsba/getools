using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pad3d
    /// </summary>
    public class Pad3d : Pad
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public new const string CTypeName = "struct pad3d";

        /// <summary>
        /// Size of the point struct in bytes.
        /// </summary>
        public new const int SizeOf = Pad.SizeOf + BoundingBoxf.SizeOf;

        public Pad3d()
        {
        }

        public BoundingBoxf BoundingBox { get; set; }

        public static Pad3d ReadFromBinFile(BinaryReader br)
        {
            var result = new Pad3d();

            result.Position = Coord3df.ReadFromBinFile(br);
            result.Up = Coord3df.ReadFromBinFile(br);
            result.Look = Coord3df.ReadFromBinFile(br);

            result.NameRodataOffset = BitUtility.Read16Big(br);
            result.Unknown = BitUtility.Read32Big(br);

            result.BoundingBox = BoundingBoxf.ReadFromBinFile(br);

            return result;
        }
    }
}
