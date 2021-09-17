using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for cutscene (end of level).
    /// </summary>
    public class SetupObjectCutscene : SetupObjectBase, ISetupObject
    {
        private const int _thisSize = 6 * Config.TargetWordSize;

        public const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectCutscene"/> class.
        /// </summary>
        public SetupObjectCutscene()
            : base(PropDef.Cutscene)
        {
        }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x0.
        /// </summary>
        public int XCoord { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x4.
        /// </summary>
        public int YCoord { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x8.
        /// </summary>
        public int ZCoord { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0xc.
        /// </summary>
        public int LatRot { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x10.
        /// </summary>
        public int VertRot { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x14.
        /// </summary>
        public uint IllumPreset { get; set; }

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

            BitUtility.Insert32Big(bytes, pos, XCoord);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, YCoord);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, ZCoord);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, LatRot);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, VertRot);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, IllumPreset);
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
            sb.Append(XCoord);
            sb.Append(", ");
            sb.Append(YCoord);
            sb.Append(", ");
            sb.Append(ZCoord);
            sb.Append(", ");
            sb.Append(LatRot);
            sb.Append(", ");
            sb.Append(VertRot);
            sb.Append(", ");
            sb.Append(IllumPreset);
        }
    }
}
