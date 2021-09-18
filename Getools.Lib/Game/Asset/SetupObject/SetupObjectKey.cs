using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for key.
    /// </summary>
    public class SetupObjectKey : SetupObjectGenericBase
    {
        private const int _thisSize = 1 * Config.TargetWordSize;

        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public new const int SizeOf = SetupObjectBase.BaseSizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectKey"/> class.
        /// </summary>
        public SetupObjectKey()
            : base(PropDef.Key)
        {
        }

        /// <summary>
        /// Gets or sets key id.
        /// Struct offset 0x0.
        /// </summary>
        public uint Key { get; set; }

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

        /// <inheritdoc />
        public override string ToCInlineS32Array(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            AppendToCInlineS32Array(sb);

            return sb.ToString();
        }

        /// <summary>
        /// Converts the current object to a byte array.
        /// Alignment is not considered.
        /// </summary>
        /// <returns>Byte array of object.</returns>
        public new byte[] ToByteArray()
        {
            var bytes = new byte[_thisSize];

            int pos = 0;

            BitUtility.Insert32Big(bytes, pos, Key);
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
        protected override void AppendToCInlineS32Array(StringBuilder sb)
        {
            base.AppendToCInlineS32Array(sb);

            sb.Append(", ");
            sb.Append(Key);
        }
    }
}
