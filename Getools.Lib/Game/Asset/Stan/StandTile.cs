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
    /// Single tile definition.
    /// Subset of <see cref="StandFile"/>.
    /// </summary>
    public class StandTile : IBinData
    {
        /// <summary>
        /// Size of the tile struct in bytes without any points.
        /// </summary>
        public const int SizeOfTileWithoutPoints = 8;

        /// <summary>
        /// Size of the beta tile struct in bytes without any points.
        /// </summary>
        public const int SizeOfBetaTileWithoutPoints = 12;

        /// <summary>
        /// String length of name on the beta stan.
        /// This is the exact string length including zeroes.
        /// </summary>
        public const int TileNameStringLength = 8;

        /// <summary>
        /// C file, default variable declaration prefix for tiles.
        /// </summary>
        public const string DefaultDeclarationName = "tile_";

        /// <summary>
        /// C file, tile type name, non-beta. Should match known struct type.
        /// </summary>
        public const string TileCTypeName = "StandTile";

        /// <summary>
        /// C file, beta tile type name. Should match known struct type.
        /// </summary>
        public const string TileBetaCTypeName = "BetaStandTile";

        private Guid _metaId = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="StandTile"/> class.
        /// </summary>
        public StandTile()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandTile"/> class.
        /// </summary>
        /// <param name="format">Stan format.</param>
        public StandTile(TypeFormat format)
        {
            Format = format;
        }

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId => _metaId;

        /// <summary>
        /// Tile internal name or identifier. Only used in release struct.
        /// 24 bits.
        /// Upper 16: Id?
        /// Last 8: Group Id?
        /// </summary>
        public int InternalName { get; set; }

        /// <summary>
        /// Tile room.  Only used in release struct.
        /// Assumed to correspond to <see cref="UnknownBeta"/> when converting
        /// to beta version, but not known for sure.
        /// </summary>
        public byte Room { get; set; }

        /// <summary>
        /// Only used by beta struct.
        /// Debug string giving the tile name. This may be the <see cref="InternalName"/>
        /// but this is not known for sure.
        /// </summary>
        public RodataString DebugName { get; set; }

        /// <summary>
        /// 4 bits.
        /// <see cref="StanFlags"/>.
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
        /// Only used by beta struct, otherwise not present. 16 bits.
        /// Assumed to be the <see cref="Room"/> when converting between normal version,
        /// but not known for sure.
        /// </summary>
        public short? UnknownBeta { get; set; } = null;

        /// <summary>
        /// 4 bits. (beta: 8 bits)
        /// Should only be used for reading from binary file; <see cref="Points.Count"/> is used internally when possible.
        /// </summary>
        public byte PointCount { get; set; }

        /// <summary>
        /// 4 bits. (beta: 8 bits)
        /// Index of (one of) most extreme points of the tile.
        /// </summary>
        public byte FirstPoint { get; set; }

        /// <summary>
        /// 4 bits. (beta: 8 bits)
        /// Index of (one of) most extreme points of the tile.
        /// </summary>
        public byte SecondPoint { get; set; }

        /// <summary>
        /// 4 bits. (beta: 8 bits)
        /// Index of (one of) most extreme points of the tile.
        /// </summary>
        public byte ThirdPoint { get; set; }

        /// <summary>
        /// When parsing a .c file, this will be the name of the tile after an underscore.
        /// For example, "tile_9" -> 9.
        /// When loading a binary file, this will be the index of the tile seen so far (0,1,2,...).
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// List of points associated with the tile.
        /// </summary>
        public List<StandTilePoint> Points { get; set; } = new List<StandTilePoint>();

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataSize => GetDataSizeOf();

        /// <summary>
        /// Gets or sets explanation for how object should be serialized to JSON.
        /// </summary>
        internal TypeFormat Format { get; set; }

        /// <summary>
        /// Gets the c type name based on the current format.
        /// </summary>
        /// <returns>Type name.</returns>
        public string GetCTypeName()
        {
            if (Format == TypeFormat.Normal)
            {
                return TileCTypeName;
            }
            else if (Format == TypeFormat.Beta)
            {
                return TileBetaCTypeName;
            }
            else
            {
                throw new InvalidStateException("Format not set.");
            }
        }

        /// <summary>
        /// Sets the format of the tile. Visits children and updates any format specific values.
        /// </summary>
        /// <param name="format">Format to use.</param>
        public void SetFormat(TypeFormat format)
        {
            Format = format;

            foreach (var point in Points)
            {
                point.SetFormat(format);
            }
        }

        /// <summary>
        /// Should be called after deserializing. Cleans up values/properties
        /// based on the known format.
        /// </summary>
        public void DeserializeFix()
        {
            int offset = BaseDataOffset;

            foreach (var point in Points)
            {
                point.BaseDataOffset = offset;
                point.DeserializeFix();

                offset += point.GetSizeOf();
            }

            if (string.IsNullOrEmpty(VariableName))
            {
                VariableName = $"tile_{OrderIndex}";
            }

            // This is just a guess at converting normal<->beta
            // since the setup data is missing ...
            if (Format == TypeFormat.Normal)
            {
                if (object.ReferenceEquals(null, DebugName) || string.IsNullOrEmpty(DebugName.Value))
                {
                    DebugName = "p" + InternalName.ToString("x:8").Substring(3);
                }

                if (UnknownBeta == 0 || !UnknownBeta.HasValue)
                {
                    UnknownBeta = Room;
                }
            }
            else if (Format == TypeFormat.Beta)
            {
                if (!object.ReferenceEquals(null, DebugName) && !string.IsNullOrEmpty(DebugName.Value) && DebugName.Value[0] == 'p')
                {
                    InternalName = Convert.ToInt32(DebugName.Value.Substring(1), 16);
                }

                if (Room == 0)
                {
                    // this is all a guess anyway, but it should be that the only time
                    // UnknownBeta is null is for the final regular tile that ends the
                    // sequence of beta tiles. In that case, it's alright to default to zero.
                    Room = (byte)(UnknownBeta ?? 0);
                }
            }
        }

        /// <summary>
        /// Converts the current object to a byte array.
        /// The size will vary based on <see cref="Format"/>.
        /// Uses Points.Count instead of PointsCount property.
        /// </summary>
        /// <returns>Byte array of object.</returns>
        public byte[] ToByteArray()
        {
            if (Format == TypeFormat.Normal)
            {
                var results = new byte[SizeOfTileWithoutPoints + (Points.Count * StandTilePoint.SizeOf)];

                BitUtility.InsertLower24Big(results, 0, InternalName);
                results[3] = Room;

                results[4] = (byte)(((Flags & 0xf) << 4) | (R & 0xf));
                results[5] = (byte)(((G & 0xf) << 4) | (B & 0xf));
                results[6] = (byte)((((byte)Points.Count & 0xf) << 4) | (FirstPoint & 0xf));
                results[7] = (byte)(((SecondPoint & 0xf) << 4) | (ThirdPoint & 0xf));

                int index = SizeOfTileWithoutPoints;
                for (int i = 0; i < Points.Count; i++)
                {
                    Array.Copy(Points[i].ToByteArray(), 0, results, index, StandTilePoint.SizeOf);
                    index += StandTilePoint.SizeOf;
                }

                return results;
            }
            else if (Format == TypeFormat.Beta)
            {
                var results = new byte[SizeOfBetaTileWithoutPoints + (Points.Count * StandTilePoint.BetaSizeOf)];

                // .rodata pointer address is unknown at this time.
                BitUtility.Insert32Big(results, 0, (short)0);

                results[4] = (byte)(((Flags & 0xf) << 4) | (R & 0xf));
                results[5] = (byte)(((G & 0xf) << 4) | (B & 0xf));

                BitUtility.InsertShortBig(results, 6, UnknownBeta ?? 0);

                results[8] = (byte)Points.Count;
                results[9] = (byte)FirstPoint;
                results[10] = (byte)SecondPoint;
                results[11] = (byte)ThirdPoint;

                int index = SizeOfBetaTileWithoutPoints;
                for (int i = 0; i < Points.Count; i++)
                {
                    Array.Copy(Points[i].ToByteArray(), 0, results, index, StandTilePoint.BetaSizeOf);
                    index += StandTilePoint.BetaSizeOf;
                }

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

            if (Format == TypeFormat.Beta)
            {
                context.AppendToRodataSection(DebugName);
            }

            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].Collect(context);
            }
        }

        public void Assemble(IAssembleContext context)
        {
            if (Format == TypeFormat.Normal)
            {
                var results = new byte[SizeOfTileWithoutPoints];

                BitUtility.InsertLower24Big(results, 0, InternalName);
                results[3] = Room;

                results[4] = (byte)(((Flags & 0xf) << 4) | (R & 0xf));
                results[5] = (byte)(((G & 0xf) << 4) | (B & 0xf));
                results[6] = (byte)((((byte)Points.Count & 0xf) << 4) | (FirstPoint & 0xf));
                results[7] = (byte)(((SecondPoint & 0xf) << 4) | (ThirdPoint & 0xf));

                var aac = context.AssembleAppendBytes(results, Config.TargetWordSize);
                BaseDataOffset = aac.DataStartAddress;
            }
            else if (Format == TypeFormat.Beta)
            {
                var results = new byte[SizeOfBetaTileWithoutPoints];

                // .rodata pointer address is unknown at this time.
                BitUtility.Insert32Big(results, 0, (short)0);

                results[4] = (byte)(((Flags & 0xf) << 4) | (R & 0xf));
                results[5] = (byte)(((G & 0xf) << 4) | (B & 0xf));

                BitUtility.InsertShortBig(results, 6, UnknownBeta ?? 0);

                results[8] = (byte)Points.Count;
                results[9] = (byte)FirstPoint;
                results[10] = (byte)SecondPoint;
                results[11] = (byte)ThirdPoint;

                var aac = context.AssembleAppendBytes(results, Config.TargetWordSize);
                BaseDataOffset = aac.DataStartAddress;

                var p = new PointerVariable(DebugName);
                p.BaseDataOffset = aac.DataStartAddress;
                context.RegisterPointer(p);
            }
            else
            {
                throw new InvalidStateException("Format not set.");
            }
        }

        /// <summary>
        /// Converts tile to c declaration.
        /// </summary>
        /// <param name="prefix">Optional prefix to begin lines with.</param>
        /// <param name="overrideFormat">If set, will use this format to convert tile to c. Otherwise uses the tile's current <see cref="Format"/>.</param>
        /// <returns>C declaration including variable name, brackets, and closing semi-colon.</returns>
        public string ToCDeclaration(string prefix = "", TypeFormat overrideFormat = TypeFormat.DefaultUnknown)
        {
            if (Format == TypeFormat.DefaultUnknown)
            {
                throw new InvalidStateException($"{nameof(ToCDeclaration)}: Format not set.");
            }

            if (overrideFormat == TypeFormat.Normal || Format == TypeFormat.Normal)
            {
                return ToNormalCDeclaration(prefix);
            }
            else if (overrideFormat == TypeFormat.Beta || Format == TypeFormat.Beta)
            {
                return ToBetaCDeclaration(prefix);
            }
            else
            {
                throw new NotSupportedException($"{nameof(ToCDeclaration)}: Type not supported: {overrideFormat}");
            }
        }

        /// <summary>
        /// Converts tile to inline c declaration.
        /// </summary>
        /// <param name="prefix">Optional prefix to begin lines with.</param>
        /// <param name="overrideFormat">If set, will use this format to convert tile to c. Otherwise uses the tile's current <see cref="Format"/>.</param>
        /// <returns>C inline declaration including brackets, but excluding variable name and closing semi-colon.</returns>
        public string ToCInlineDeclaration(string prefix = "", TypeFormat overrideFormat = TypeFormat.DefaultUnknown)
        {
            if (Format == TypeFormat.DefaultUnknown)
            {
                throw new InvalidStateException($"{nameof(ToCInlineDeclaration)}: Format not set.");
            }

            if (overrideFormat == TypeFormat.Normal || Format == TypeFormat.Normal)
            {
                return ToNormalCInlineDeclaration(prefix);
            }
            else if (overrideFormat == TypeFormat.Beta || Format == TypeFormat.Beta)
            {
                return ToBetaCInlineDeclaration(prefix);
            }
            else
            {
                throw new NotSupportedException($"{nameof(ToCInlineDeclaration)}: Type not supported: {overrideFormat}");
            }
        }

        /// <summary>
        /// Returns the size of the object in bytes,
        /// according to the current format.
        /// This does not include <see cref="Points"/>.
        /// </summary>
        /// <returns>Size in bytes.</returns>
        public int GetSizeOfEmpty()
        {
            if (Format == TypeFormat.Normal)
            {
                return 4 * SizeOfTileWithoutPoints;
            }
            else if (Format == TypeFormat.Beta)
            {
                return 4 * SizeOfBetaTileWithoutPoints;
            }
            else
            {
                throw new InvalidStateException("Format not set.");
            }
        }

        /// <summary>
        /// Calculates the .data section size of this object.
        /// This does include allocation for <see cref="Points"/>.
        /// </summary>
        /// <returns>Size in bytes.</returns>
        public int GetDataSizeOf()
        {
            if (Format == TypeFormat.Normal)
            {
                return SizeOfTileWithoutPoints + Points.Sum(x => x.GetSizeOf());
            }
            else if (Format == TypeFormat.Beta)
            {
                return SizeOfBetaTileWithoutPoints + Points.Sum(x => x.GetSizeOf());
            }
            else
            {
                throw new InvalidStateException("Format not set.");
            }
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        private string ToNormalCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{StandTile.TileCTypeName} {VariableName} = {{");

            ToCDeclarationCommon(sb, prefix);

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs.
        /// Does not include type, variable name, or trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        private string ToNormalCInlineDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{{ /* tile index {OrderIndex} */");

            ToCDeclarationCommon(sb, prefix);

            sb.Append($"{prefix}}}");

            return sb.ToString();
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using beta structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        private string ToBetaCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{StandTile.TileBetaCTypeName} {VariableName} = {{");

            ToBetaCDeclarationCommon(sb, prefix);

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using beta structs.
        /// Does not include type, variable name, or trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        private string ToBetaCInlineDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{{ /* tile index {OrderIndex} */");

            ToBetaCDeclarationCommon(sb, prefix);

            sb.Append($"{prefix}}}");

            return sb.ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1119:Statement should not use unnecessary parenthesis", Justification = "<Justification>")]
        private void ToCDeclarationCommon(StringBuilder sb, string prefix = "")
        {
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(InternalName & 0xffffff):x6}, 0x{(byte)Room:x2},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(Flags & 0xf):x1},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(R & 0xf):x1}, 0x{(G & 0xf):x1}, 0x{(B & 0xf):x1},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{PointCount & 0xf},");
            sb.Append($"{prefix}{Config.DefaultIndent}0x{(FirstPoint & 0xf):x1}, 0x{(SecondPoint & 0xf):x1}, 0x{(ThirdPoint & 0xf):x1}");

            if (Points.Any())
            {
                sb.AppendLine(",");

                // begin points list
                sb.AppendLine($"{prefix}{Config.DefaultIndent}{{");

                string indent = Config.DefaultIndent + Config.DefaultIndent;

                for (int i = 0; i < Points.Count - 1; i++)
                {
                    var p = Points[i];
                    sb.AppendLine(prefix + p.ToCInlineDeclaration(indent) + ",");
                }

                if (Points.Any())
                {
                    var p = Points.Last();
                    sb.AppendLine(prefix + p.ToCInlineDeclaration(indent));
                }

                // close points list
                sb.AppendLine($"{prefix}{Config.DefaultIndent}}}");
            }
            else
            {
                // end the last line
                sb.AppendLine(string.Empty);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1119:Statement should not use unnecessary parenthesis", Justification = "<Justification>")]
        private void ToBetaCDeclarationCommon(StringBuilder sb, string prefix = "")
        {
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{DebugName.ToCValueOrNullEmpty()},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(Flags & 0xf):x1},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(R & 0xf):x1}, 0x{(G & 0xf):x1}, 0x{(B & 0xf):x1},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{UnknownBeta:x4},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{PointCount},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{FirstPoint:x2}, 0x{SecondPoint:x2}, 0x{ThirdPoint:x2},");

            // begin points list
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{{");

            string indent = Config.DefaultIndent + Config.DefaultIndent;

            for (int i = 0; i < Points.Count - 1; i++)
            {
                var p = Points[i];
                sb.AppendLine(prefix + p.ToBetaCInlineDeclaration(indent) + ",");
            }

            if (Points.Any())
            {
                var p = Points.Last();
                sb.AppendLine(prefix + p.ToBetaCInlineDeclaration(indent));
            }

            // close points list
            sb.AppendLine($"{prefix}{Config.DefaultIndent}}}");
        }

        private void LookupPadLink()
        {
            // TODO: pad link.

            /*
             * from carnivorous:

unsigned char Byte1 = ((level.clippingPoints[clippingClicked.back().clippingRoom][clippingClicked.back().clippingNumber].threeBytes >> 16) & 0xFF);
unsigned char Byte2 = ((level.clippingPoints[clippingClicked.back().clippingRoom][clippingClicked.back().clippingNumber].threeBytes >> 8) & 0xFF);
unsigned char Byte3 = (level.clippingPoints[clippingClicked.back().clippingRoom][clippingClicked.back().clippingNumber].threeBytes & 0xFF);

CString tempDecimal;
tempDecimal.Format("p%u", ((Byte1 << 8) | Byte2));

char letters[26] = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

CString tempLetter;
tempLetter.Format("%c", letters[(int)Byte3 / (int) 8]);
tempDecimal += tempLetter;

if ((Byte3 % 8) > 0)
{
    CString tempChar;
    tempChar.Format("%u", (Byte3 % 8));
    tempDecimal += tempChar;
}
             * */
        }
    }
}
