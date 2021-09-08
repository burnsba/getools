using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Error;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Stan
{
    /// <summary>
    /// Point used by stan.
    /// Subset of <see cref="StandTile"/> within <see cref="StandFile"/>.
    /// </summary>
    public class StandTilePoint : IBinData
    {
        /// <summary>
        /// Size of the point struct in bytes (non-beta).
        /// </summary>
        public const int SizeOf = 8;

        /// <summary>
        /// Size of the beta point struct in bytes.
        /// </summary>
        public const int BetaSizeOf = 16;

        private Guid _metaId = Guid.NewGuid();

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

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId => _metaId;

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

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataSize => GetSizeOf();

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
        /// <param name="prependBytesCount">Optional. If set, will prepend this many bytes at the start of the object.</param>
        /// <param name="appendBytesCount">Optional. If set, will append this many bytes after the object. Used for alignment.</param>
        /// <returns>Byte array of object.</returns>
        public byte[] ToByteArray(int? prependBytesCount = null, int? appendBytesCount = null)
        {
            int prepend = prependBytesCount ?? 0;
            var resultLength = prepend + (appendBytesCount ?? 0);
            int resultIndex = prepend;

            if (Format == TypeFormat.Normal)
            {
                resultLength += SizeOf;

                var results = new byte[resultLength];

                BitUtility.InsertShortBig(results, resultIndex, (short)X);
                resultIndex += 2;
                BitUtility.InsertShortBig(results, resultIndex, (short)Y);
                resultIndex += 2;
                BitUtility.InsertShortBig(results, resultIndex, (short)Z);
                resultIndex += 2;
                BitUtility.InsertShortBig(results, resultIndex, (short)Link);
                resultIndex += 2;

                return results;
            }
            else if (Format == TypeFormat.Beta)
            {
                resultLength += BetaSizeOf;

                var results = new byte[resultLength];

                var fx = BitUtility.CastToInt32(FloatX);
                var fy = BitUtility.CastToInt32(FloatY);
                var fz = BitUtility.CastToInt32(FloatZ);

                BitUtility.Insert32Big(results, resultIndex, (int)fx);
                resultIndex += 4;
                BitUtility.Insert32Big(results, resultIndex, (int)fy);
                resultIndex += 4;
                BitUtility.Insert32Big(results, resultIndex, (int)fz);
                resultIndex += 4;
                BitUtility.Insert32Big(results, resultIndex, (int)Link);
                resultIndex += 4;

                return results;
            }
            else
            {
                throw new InvalidStateException("Format not set.");
            }
        }

        public void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

        public void Assemble(IAssembleContext context)
        {
            var aac = context.AssembleAppendBytes(ToByteArray(), Config.TargetWordSize);
            BaseDataOffset = aac.DataStartAddress;
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
    }
}
