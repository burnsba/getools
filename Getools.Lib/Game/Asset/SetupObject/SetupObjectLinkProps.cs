using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition to link props.
    /// </summary>
    public class SetupObjectLinkProps : SetupObjectBase, ISetupObject
    {
        private const int _thisSize = 3 * Config.TargetWordSize;

        public const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectLinkProps"/> class.
        /// </summary>
        public SetupObjectLinkProps()
            : base(PropDef.LinkProps)
        {
        }

        /// <summary>
        /// Item offset 1.
        /// </summary>
        public int Offset1 { get; set; }

        /// <summary>
        /// Item offset 2.
        /// </summary>
        public int Offset2 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public int Unknown08 { get; set; }

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

        public byte[] ToByteArray()
        {
            var bytes = new byte[_thisSize];

            int pos = 0;

            BitUtility.Insert32Big(bytes, pos, Offset1);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Offset2);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Unknown08);
            pos += Config.TargetWordSize;

            return bytes;
        }

        /// <inheritdoc />
        public override void Assemble(IAssembleContext context)
        {
            var bytes = new byte[SizeOf];

            var thisBytes = ToByteArray();

            var headerBytes = ((GameObjectHeaderBase)this).ToByteArray();
            Array.Copy(headerBytes, bytes, headerBytes.Length);
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
            sb.Append(Offset1);
            sb.Append(", ");
            sb.Append(Offset2);
            sb.Append(", ");
            sb.Append(Unknown08);
        }
    }
}
