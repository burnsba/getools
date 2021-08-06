using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Only have one set of properties on the point, if it's a beta
    /// format than the int is actually the interal bit values of the float.
    /// In that case, call BitUtility.CastToInt32() to set,
    /// and BitUtility.CastToFloat to get the value.
    /// </remarks>
    public class StandTilePoint
    {
        // in bytes
        public const int SizeOf = 8;
        public const int BetaSizeOf = 16;

        /// <summary>
        /// Gets or sets point X coordinate. This property is used for "standard" points (not debug/beta).
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets point Y coordinate. This property is used for "standard" points (not debug/beta).
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Gets or sets point Z coordinate. This property is used for "standard" points (not debug/beta).
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// Gets or sets point X coordinate. This property is used for debug/beta points.
        /// </summary>
        public Single FloatX { get; set; }

        /// <summary>
        /// Gets or sets point Y coordinate. This property is used for debug/beta points.
        /// </summary>
        public Single FloatY { get; set; }

        /// <summary>
        /// Gets or sets point Z coordinate. This property is used for debug/beta points.
        /// </summary>
        public Single FloatZ { get; set; }

        public int Link { get; set; }

        /// <summary>
        /// Gets or sets explanation for how object should be serialized to JSON.
        /// </summary>
        internal TypeFormat SerializeFormat { get; set; }

        public void SetFormat(TypeFormat format)
        {
            SerializeFormat = format;
        }

        public void DeserializeFix()
        {
            if (SerializeFormat == TypeFormat.Normal)
            {
                FloatX = (Single)X;
                FloatY = (Single)Y;
                FloatZ = (Single)Z;
            }
            else if (SerializeFormat == TypeFormat.Beta)
            {
                X = (int)FloatX;
                Y = (int)FloatY;
                Z = (int)FloatZ;
            }
        }

        public string ToCInlineDeclaration(string prefix = "")
        {
            return $"{prefix}{{{(short)X}, {(short)Y}, {(short)Z}, 0x{(short)Link:x4}}}";
        }

        public string ToBetaCInlineDeclaration(string prefix = "")
        {
            return $"{prefix}{{{FloatX}, {FloatY}, {FloatZ}, 0x{(int)Link:x8}}}";
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

            var fx = BitUtility.CastToInt32(FloatX);
            var fy = BitUtility.CastToInt32(FloatY);
            var fz = BitUtility.CastToInt32(FloatZ);

            BitUtility.Insert32Big(results, 0, (int)fx);
            BitUtility.Insert32Big(results, 4, (int)fy);
            BitUtility.Insert32Big(results, 8, (int)fz);
            BitUtility.Insert32Big(results, 12, (int)Link);

            return results;
        }

        internal void AppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToByteArray();
            stream.Write(bytes);
        }

        internal void BetaAppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToBetaByteArray();
            stream.Write(bytes);
        }

        internal static StandTilePoint ReadFromBinFile(BinaryReader br)
        {
            var result = new StandTilePoint();

            result.X = BitUtility.Read16Big(br);
            result.Y = BitUtility.Read16Big(br);
            result.Z = BitUtility.Read16Big(br);

            result.FloatX = (Single)result.X;
            result.FloatY = (Single)result.Y;
            result.FloatZ = (Single)result.Z;

            result.Link = BitUtility.Read16Big(br);

            return result;
        }

        internal static StandTilePoint ReadFromBetaBinFile(BinaryReader br)
        {
            var result = new StandTilePoint();

            var ix = BitUtility.Read32Big(br);
            var iy = BitUtility.Read32Big(br);
            var iz = BitUtility.Read32Big(br);

            result.FloatX = BitUtility.CastToFloat(ix);
            result.FloatY = BitUtility.CastToFloat(iy);
            result.FloatZ = BitUtility.CastToFloat(iz);

            result.X = (int)result.FloatX;
            result.Y = (int)result.FloatY;
            result.Z = (int)result.FloatZ;

            result.Link = BitUtility.Read32Big(br);

            return result;
        }
    }
}
