using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for tinted glass.
    /// </summary>
    public class SetupObjectGlassTinted : SetupObjectGenericBase
    {
        private const int _thisSize = 5 * Config.TargetWordSize;

        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public new const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectGlassTinted"/> class.
        /// </summary>
        public SetupObjectGlassTinted()
            : base(PropDef.TintedGlass)
        {
        }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown04 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown08 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown0c { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown10 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown14 { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public override int BaseDataSize
        {
            get
            {
                return SizeOf;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Converts this object to byte array as it would appear in MIPS .data section.
        /// Alignment is not considered.
        /// </summary>
        /// <returns>Byte array of this object.</returns>
        public new byte[] ToByteArray()
        {
            var bytes = new byte[_thisSize];

            int pos = 0;

            BitUtility.Insert32Big(bytes, pos, Unknown04);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Unknown08);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Unknown0c);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Unknown10);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Unknown14);
            pos += Config.TargetWordSize;

            return bytes;
        }

        /// <inheritdoc />
        public override void Assemble(IAssembleContext context)
        {
            var bytes = new byte[SizeOf];

            var thisBytes = ToByteArray();

            var baseBytes = ((SetupObjectGenericBase)this).ToByteArray();
            Array.Copy(baseBytes, bytes, baseBytes.Length);
            Array.Copy(thisBytes, bytes, thisBytes.Length);

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }

        /// <inheritdoc />
        public override string ToCInlineS32Array(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            AppendToCInlineS32Array(sb);

            return sb.ToString();
        }

        /// <inheritdoc />
        protected override void AppendToCInlineS32Array(StringBuilder sb)
        {
            base.AppendToCInlineS32Array(sb);

            sb.Append(", ");
            sb.Append(Unknown04);
            sb.Append(", ");
            sb.Append(Unknown08);
            sb.Append(", ");
            sb.Append(Unknown0c);
            sb.Append(", ");
            sb.Append(Unknown10);
            sb.Append(", ");
            sb.Append(Unknown14);
        }
    }
}
