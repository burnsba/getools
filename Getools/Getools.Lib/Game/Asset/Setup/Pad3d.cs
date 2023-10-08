using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

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
        public BoundingBoxf? BoundingBox { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public override int BaseDataSize => SizeOf;

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            if (object.ReferenceEquals(null, Position))
            {
                throw new NullReferenceException($"{nameof(Position)}");
            }

            if (object.ReferenceEquals(null, Up))
            {
                throw new NullReferenceException($"{nameof(Up)}");
            }

            if (object.ReferenceEquals(null, Look))
            {
                throw new NullReferenceException($"{nameof(Look)}");
            }

            if (object.ReferenceEquals(null, BoundingBox))
            {
                throw new NullReferenceException($"{nameof(BoundingBox)}");
            }

            context.AppendToDataSection(Position);
            context.AppendToDataSection(Up);
            context.AppendToDataSection(Look);
            context.AppendToDataSection(this);
            context.AppendToDataSection(BoundingBox);
        }

        /// <inheritdoc />
        protected override void ToCDeclarationCommon(StringBuilder sb, string prefix = "")
        {
            if (object.ReferenceEquals(null, BoundingBox))
            {
                throw new NullReferenceException($"{nameof(BoundingBox)}");
            }

            base.ToCDeclarationCommon(sb, prefix);
            sb.Append(", ");
            sb.Append(BoundingBox.ToCInlineDeclaration(string.Empty));
        }
    }
}
