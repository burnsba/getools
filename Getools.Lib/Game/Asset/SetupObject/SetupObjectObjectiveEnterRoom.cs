using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for objective enter room.
    /// </summary>
    public class SetupObjectObjectiveEnterRoom : SetupObjectBase, ISetupObject
    {
        private const int _thisSize = 3 * Config.TargetWordSize;

        public const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectObjectiveEnterRoom"/> class.
        /// </summary>
        public SetupObjectObjectiveEnterRoom()
            : base(PropDef.ObjectiveEnterRoom)
        {
        }

        /// <summary>
        /// Room
        /// </summary>
        public int Room { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// </summary>
        public int Unknown04 { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// </summary>
        public int Unknown08 { get; set; }

        /// <inheritdoc />
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

            BitUtility.Insert32Big(bytes, pos, Room);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Unknown04);
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
            sb.Append(Room);
            sb.Append(", ");
            sb.Append(Unknown04);
            sb.Append(", ");
            sb.Append(Unknown08);
        }
    }
}
