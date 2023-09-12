using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for locked door.
    /// </summary>
    public class SetupObjectLock : SetupObjectBase, ISetupObject
    {
        private const int _thisSize = 3 * Config.TargetWordSize;

        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public new const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectLock"/> class.
        /// </summary>
        public SetupObjectLock()
            : base(PropDef.LockDoor)
        {
        }

        /// <summary>
        /// Door.
        /// </summary>
        public int Door { get; set; }

        /// <summary>
        /// Lock.
        /// </summary>
        public int Lock { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Maybe end of record marker.
        /// </summary>
        public int Empty { get; set; }

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

            BitUtility.Insert32Big(bytes, pos, Door);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Lock);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Empty);
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
            sb.Append(Door);
            sb.Append(", ");
            sb.Append(Lock);
            sb.Append(", ");
            sb.Append(Empty);
        }
    }
}
