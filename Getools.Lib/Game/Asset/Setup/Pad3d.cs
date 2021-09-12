using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Getools.Lib.BinPack;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pad3d. Extends <see cref="Pad"/> class by adding 3d bounding box.
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Pad3d"/> class.
        /// </summary>
        public Pad3d()
        {
        }

        /// <summary>
        /// Gets or sets 3d bounding box.
        /// Struct offset 0x2c (relative to start of <see cref="Pad"/>).
        /// </summary>
        public BoundingBoxf BoundingBox { get; set; }

        /// <inheritdoc />
        public override int BaseDataSize => SizeOf;

        /// <summary>
        /// Reads from current position in stream. Loads object from
        /// stream as it would be read from a binary file using normal structs.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        /// <returns>New object.</returns>
        public static new Pad3d ReadFromBinFile(BinaryReader br)
        {
            var result = new Pad3d();

            result.Position = Coord3df.ReadFromBinFile(br);
            result.Up = Coord3df.ReadFromBinFile(br);
            result.Look = Coord3df.ReadFromBinFile(br);

            result.Name = BitUtility.Read16Big(br);
            result.Unknown = BitUtility.Read32Big(br);

            result.BoundingBox = BoundingBoxf.ReadFromBinFile(br);

            return result;
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(Position);
            context.AppendToDataSection(Up);
            context.AppendToDataSection(Look);
            context.AppendToDataSection(this);
            context.AppendToDataSection(BoundingBox);
        }

        /// <inheritdoc />
        protected override void ToCDeclarationCommon(StringBuilder sb, string prefix = "")
        {
            base.ToCDeclarationCommon(sb, prefix);
            sb.Append(", ");
            sb.Append(BoundingBox.ToCInlineDeclaration(string.Empty));
        }
    }
}
