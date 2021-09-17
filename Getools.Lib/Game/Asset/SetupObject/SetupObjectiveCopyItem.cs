using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list objective to copy item.
    /// </summary>
    public class SetupObjectiveCopyItem : SetupObjectBase, ISetupObject
    {
        private const int _thisSize = 2 * Config.TargetWordSize;

        public const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectiveCopyItem"/> class.
        /// </summary>
        public SetupObjectiveCopyItem()
            : base(PropDef.ObjectiveCopy_Item)
        {
        }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public uint TagId { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x4.
        /// </summary>
        public ushort Unknown_04 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x6.
        /// </summary>
        public ushort Unknown_06 { get; set; }

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

            BitUtility.Insert32Big(bytes, pos, TagId);
            pos += Config.TargetWordSize;

            BitUtility.InsertShortBig(bytes, pos, Unknown_04);
            pos += Config.TargetShortSize;

            BitUtility.InsertShortBig(bytes, pos, Unknown_06);
            pos += Config.TargetShortSize;

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
            sb.Append(TagId);
            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                Formatters.IntegralTypes.ToHex4(Unknown_04),
                Formatters.IntegralTypes.ToHex4(Unknown_06));
        }
    }
}
