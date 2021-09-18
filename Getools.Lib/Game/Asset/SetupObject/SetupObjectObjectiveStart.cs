using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition to mark start of an objective definition.
    /// </summary>
    public class SetupObjectObjectiveStart : SetupObjectBase, ISetupObject
    {
        private const int _thisSize = 3 * Config.TargetWordSize;

        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public new const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectObjectiveStart"/> class.
        /// </summary>
        public SetupObjectObjectiveStart()
            : base(PropDef.ObjectiveStart)
        {
        }

        /// <summary>
        /// Gets or sets objective number.
        /// Struct offset 0x0.
        /// </summary>
        public int ObjectiveNumber { get; set; }

        /// <summary>
        /// Gets or sets text block id.
        /// Struct offset 0x4.
        /// </summary>
        public int TextId { get; set; }

        /// <summary>
        /// Gets or sets minimum difficulty this appears for.
        /// Struct offset 0x8.
        /// </summary>
        public int MinDifficulty { get; set; }

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

            BitUtility.Insert32Big(bytes, pos, ObjectiveNumber);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, TextId);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, MinDifficulty);
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
            sb.Append(ObjectiveNumber);
            sb.Append(", ");
            sb.Append(TextId);
            sb.Append(", ");
            sb.Append(MinDifficulty);
        }
    }
}
