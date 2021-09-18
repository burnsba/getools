using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for setting/changing a guard attribute.
    /// </summary>
    public class SetupObjectSetGuardAttribute : SetupObjectBase, ISetupObject
    {
        private const int _thisSize = 2 * Config.TargetWordSize;

        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public new const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectSetGuardAttribute"/> class.
        /// </summary>
        public SetupObjectSetGuardAttribute()
            : base(PropDef.SetGuardAttribute)
        {
        }

        /// <summary>
        /// Gets or sets guard id.
        /// Struct offset 0x0.
        /// </summary>
        public uint GuardId { get; set; }

        /// <summary>
        /// Gets or sets guard attribute value.
        /// Struct offset 0x4.
        /// </summary>
        public int Attribute { get; set; }

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

            BitUtility.Insert32Big(bytes, pos, GuardId);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Attribute);
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
            sb.Append(GuardId);
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Attribute));
        }
    }
}
