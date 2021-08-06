using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.Error;

namespace Getools.Lib.Game.Asset.Stan
{
    /// <summary>
    /// Single tile definition.
    /// Subset of <see cref="StandFile"/>.
    /// </summary>
    public class StandTile
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
        /// Initializes a new instance of the <see cref="StandTile"/> class.
        /// </summary>
        public StandTile()
        {
        }

        /// <summary>
        /// Tile internal name or identifier.
        /// 24 bits.
        /// Upper 16: Id?
        /// Last 8: Group Id?
        /// </summary>
        public int InternalName { get; set; }

        /// <summary>
        /// Tile room.
        /// </summary>
        public byte Room { get; set; }

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
        /// Only used by citadel stan, otherwise not present. 16 bits.
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

        /// <summary>
        /// Gets or sets explanation for how object should be serialized to JSON.
        /// </summary>
        internal TypeFormat SerializeFormat { get; set; }

        /// <summary>
        /// Sets the format of the tile. Visits children and updates any format specific values.
        /// </summary>
        /// <param name="format">Format to use.</param>
        public void SetFormat(TypeFormat format)
        {
            SerializeFormat = format;

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
            foreach (var point in Points)
            {
                point.DeserializeFix();
            }

            if (string.IsNullOrEmpty(VariableName))
            {
                VariableName = $"tile_{OrderIndex}";
            }
        }

        /// <summary>
        /// Converts the current object to a byte array, as it would
        /// exist in a regular binary format.
        /// Uses Points.Count instead of PointsCount property.
        /// </summary>
        /// <returns>Byte array of object.</returns>
        public byte[] ToByteArray()
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

        /// <summary>
        /// Converts the current object to a byte array, as it would
        /// exist in a beta binary format.
        /// Uses Points.Count instead of PointsCount property.
        /// </summary>
        /// <returns>Byte array of object.</returns>
        public byte[] ToBetaByteArray()
        {
            var results = new byte[SizeOfBetaTileWithoutPoints + (Points.Count * StandTilePoint.BetaSizeOf)];

            BitUtility.InsertLower24Big(results, 0, InternalName);
            results[3] = Room;

            results[4] = (byte)(((Flags & 0xf) << 4) | (R & 0xf));
            results[5] = (byte)(((G & 0xf) << 4) | (B & 0xf));

            BitUtility.InsertShortBig(results, 6, UnknownBeta ?? 0);

            results[8] = (byte)Points.Count;
            results[9] = FirstPoint;
            results[10] = SecondPoint;
            results[11] = ThirdPoint;

            int index = SizeOfBetaTileWithoutPoints;
            for (int i = 0; i < Points.Count; i++)
            {
                Array.Copy(Points[i].ToBetaByteArray(), 0, results, index, StandTilePoint.BetaSizeOf);
                index += StandTilePoint.BetaSizeOf;
            }

            return results;
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{Config.Stan.TileCTypeName} tile_{OrderIndex} = {{");

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
        public string ToCInlineDeclaration(string prefix = "")
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
        public string ToBetaCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{Config.Stan.TileBetaCTypeName} tile_{OrderIndex} = {{");

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
        public string ToBetaCInlineDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{{ /* tile index {OrderIndex} */");

            ToBetaCDeclarationCommon(sb, prefix);

            sb.Append($"{prefix}}}");

            return sb.ToString();
        }

        /// <summary>
        /// Reads from current position in stream. Loads object from
        /// stream as it would be read from a binary file using normal structs.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        /// <param name="tileIndex">Sets the <see cref="OrderIndex"/> and used to build the standard <see cref="VariableName"/>.</param>
        /// <returns>New object.</returns>
        internal static StandTile ReadFromBinFile(BinaryReader br, int tileIndex)
        {
            var result = new StandTile();

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
            result.R = (byte)(b & 0xf);

            b = br.ReadByte();
            result.G = (byte)((b >> 4) & 0xf);
            result.B = (byte)(b & 0xf);

            b = br.ReadByte();
            result.PointCount = (byte)((b >> 4) & 0xf);
            result.FirstPoint = (byte)(b & 0xf);

            if (result.PointCount < 1)
            {
                throw new BadFileFormatException("Tile is defined with zero points");
            }

            b = br.ReadByte();
            result.SecondPoint = (byte)((b >> 4) & 0xf);
            result.ThirdPoint = (byte)(b & 0xf);

            result.OrderIndex = tileIndex;

            // Done with tile header, now read points.
            for (int i = 0; i < result.PointCount; i++)
            {
                var point = StandTilePoint.ReadFromBinFile(br);
                result.Points.Add(point);
            }

            return result;
        }

        /// <summary>
        /// Reads from current position in stream. Loads object from
        /// stream as it would be read from a binary file using beta structs.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        /// <param name="tileIndex">Sets the <see cref="OrderIndex"/> and used to build the standard <see cref="VariableName"/>.</param>
        /// <returns>New object.</returns>
        internal static StandTile ReadFromBetaBinFile(BinaryReader br, int tileIndex)
        {
            var result = new StandTile();

            Byte b;

            b = br.ReadByte();
            result.InternalName = b << 16;
            b = br.ReadByte();
            result.InternalName |= b << 8;
            b = br.ReadByte();
            result.InternalName |= b;

            result.Room = br.ReadByte();

            // beta has some null room references, so also check against InternalName
            if (result.InternalName == 0 && result.Room == 0)
            {
                br.BaseStream.Seek(-4, SeekOrigin.Current);
                throw new Error.ExpectedStreamEndException();
            }

            b = br.ReadByte();
            result.Flags = (byte)((b >> 4) & 0xf);
            result.R = (byte)(b & 0xf);

            b = br.ReadByte();
            result.G = (byte)((b >> 4) & 0xf);
            result.B = (byte)(b & 0xf);

            result.UnknownBeta = BitUtility.Read16Big(br);

            result.PointCount = br.ReadByte();

            if (result.PointCount < 1)
            {
                throw new BadFileFormatException("Tile is defined with zero points");
            }

            result.FirstPoint = br.ReadByte();
            result.SecondPoint = br.ReadByte();
            result.ThirdPoint = br.ReadByte();

            result.OrderIndex = tileIndex;

            // Done with tile header, now read points.
            for (int i = 0; i < result.PointCount; i++)
            {
                var point = StandTilePoint.ReadFromBetaBinFile(br);
                result.Points.Add(point);
            }

            return result;
        }

        /// <summary>
        /// Converts this object to a byte array using normal structs and writes
        /// it to the current stream position.
        /// </summary>
        /// <param name="stream">Stream to write to.</param>
        internal void AppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToByteArray();
            stream.Write(bytes);
        }

        /// <summary>
        /// Converts this object to a byte array using beta structs and writes
        /// it to the current stream position.
        /// </summary>
        /// <param name="stream">Stream to write to.</param>
        internal void BetaAppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToBetaByteArray();
            stream.Write(bytes);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1119:Statement should not use unnecessary parenthesis", Justification = "<Justification>")]
        private void ToCDeclarationCommon(StringBuilder sb, string prefix = "")
        {
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(InternalName & 0xffffff):x6}, 0x{(byte)Room:x2},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(Flags & 0xf):x1},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(R & 0xf):x1}, 0x{(G & 0xf):x1}, 0x{(B & 0xf):x1},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{PointCount & 0xf},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(FirstPoint & 0xf):x1}, 0x{(SecondPoint & 0xf):x1}, 0x{(ThirdPoint & 0xf):x1},");

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1119:Statement should not use unnecessary parenthesis", Justification = "<Justification>")]
        private void ToBetaCDeclarationCommon(StringBuilder sb, string prefix = "")
        {
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(InternalName & 0xffffff):x6}, 0x{(byte)Room:x2},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(Flags & 0xf):x1},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{(R & 0xf):x1}, 0x{(G & 0xf):x1}, 0x{(B & 0xf):x1},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}0x{UnknownBeta:x2},");
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
    }
}
