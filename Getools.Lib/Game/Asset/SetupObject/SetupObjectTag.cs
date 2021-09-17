using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for tag.
    /// </summary>
    public class SetupObjectTag : SetupObjectBase, ISetupObject
    {
        private const int _thisSize = 3 * Config.TargetWordSize;

        public const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectTag"/> class.
        /// </summary>
        public SetupObjectTag()
            : base(PropDef.Tag)
        {
        }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x0.
        /// </summary>
        public ushort TagId { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x2.
        /// </summary>
        public ushort Value { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x4.
        /// </summary>
        public uint Unknown_04 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x8.
        /// </summary>
        public uint Unknown_08 { get; set; }

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

            BitUtility.InsertShortBig(bytes, pos, TagId);
            pos += Config.TargetShortSize;

            BitUtility.InsertShortBig(bytes, pos, Value);
            pos += Config.TargetShortSize;

            BitUtility.Insert32Big(bytes, pos, Unknown_04);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Unknown_08);
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
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                TagId.ToString(),
                Formatters.IntegralTypes.ToHex4(Value));
            sb.Append(", ");
            sb.Append(Unknown_04);
            sb.Append(", ");
            sb.Append(Unknown_08);
        }
    }
}
