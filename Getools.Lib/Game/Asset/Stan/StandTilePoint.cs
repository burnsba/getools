using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.Error;

namespace Getools.Lib.Game.Asset.Stan
{
    /// <summary>
    /// Point used by stan.
    /// Subset of <see cref="StandTile"/> within <see cref="StandFile"/>.
    /// </summary>
    public class StandTilePoint
    {
        /// <summary>
        /// Size of the point struct in bytes (non-beta).
        /// </summary>
        public const int SizeOf = 8;

        /// <summary>
        /// Size of the beta point struct in bytes.
        /// </summary>
        public const int BetaSizeOf = 16;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandTilePoint"/> class.
        /// </summary>
        public StandTilePoint()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandTilePoint"/> class.
        /// </summary>
        /// <param name="format">Stan format.</param>
        public StandTilePoint(TypeFormat format)
        {
            Format = format;
        }

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

        /// <summary>
        /// Gets or sets "link" property of point.
        /// </summary>
        public int Link { get; set; }

        /// <summary>
        /// Gets or sets explanation for how object should be serialized.
        /// </summary>
        internal TypeFormat Format { get; set; }

        /// <summary>
        /// Sets the format used to serialize the data.
        /// </summary>
        /// <param name="format">Format to use.</param>
        public void SetFormat(TypeFormat format)
        {
            Format = format;
        }

        /// <summary>
        /// Should be called after deserializing. Cleans up values/properties
        /// based on the known format.
        /// </summary>
        public void DeserializeFix()
        {
            if (Format == TypeFormat.Normal)
            {
                FloatX = (Single)X;
                FloatY = (Single)Y;
                FloatZ = (Single)Z;
            }
            else if (Format == TypeFormat.Beta)
            {
                X = (int)FloatX;
                Y = (int)FloatY;
                Z = (int)FloatZ;
            }
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs.
        /// Does not include type, variable name, or trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToCInlineDeclaration(string prefix = "")
        {
            return $"{prefix}{{{(short)X}, {(short)Y}, {(short)Z}, 0x{(short)Link:x4}}}";
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using beta structs.
        /// Does not include type, variable name, or trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToBetaCInlineDeclaration(string prefix = "")
        {
            return $"{prefix}{{{FloatX}, {FloatY}, {FloatZ}, 0x{(int)Link:x8}}}";
        }

        /// <summary>
        /// Converts the current object to a byte array. The size will vary based on <see cref="Format"/>.
        /// </summary>
        /// <returns>Byte array of object.</returns>
        public byte[] ToByteArray()
        {
            if (Format == TypeFormat.Normal)
            {
                var results = new byte[SizeOf];

                BitUtility.InsertShortBig(results, 0, (short)X);
                BitUtility.InsertShortBig(results, 2, (short)Y);
                BitUtility.InsertShortBig(results, 4, (short)Z);
                BitUtility.InsertShortBig(results, 6, (short)Link);

                return results;
            }
            else if (Format == TypeFormat.Beta)
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
            else
            {
                throw new InvalidStateException("Format not set.");
            }
        }

        /// <summary>
        /// Returns the binary size of the object in bytes,
        /// according to the current format.
        /// </summary>
        /// <returns>Size in bytes.</returns>
        public int GetSizeOf()
        {
            if (Format == TypeFormat.Normal)
            {
                return SizeOf;
            }
            else if (Format == TypeFormat.Beta)
            {
                return BetaSizeOf;
            }
            else
            {
                throw new InvalidStateException("Format not set.");
            }
        }

        /// <summary>
        /// Reads from current position in stream. Loads object from
        /// stream as it would be read from a binary file using beta structs.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        /// <returns>New object.</returns>
        internal static StandTilePoint ReadFromBetaBinFile(BinaryReader br)
        {
            var result = new StandTilePoint(TypeFormat.Beta);

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
