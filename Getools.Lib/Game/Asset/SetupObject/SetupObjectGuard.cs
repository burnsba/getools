using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list guard definition.
    /// </summary>
    public class SetupObjectGuard : SetupObjectBase, ISetupObject
    {
        private const int _thisSize = 6 * Config.TargetWordSize;

        public const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectGuard"/> class.
        /// </summary>
        public SetupObjectGuard()
            : base(PropDef.Guard)
        {
        }

        /// <summary>
        /// Gets or sets object id / guard id.
        /// </summary>
        public ushort ObjectId { get; set; }

        /// <summary>
        /// Gets or sets preset id.
        /// </summary>
        public ushort Preset { get; set; }

        /// <summary>
        /// Gets or sets guard body.
        /// </summary>
        public ushort BodyId { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public ushort ActionPathAssignment { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public uint PresetToTrigger { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public ushort Unknown10 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public ushort Health { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public ushort ReactionTime { get; set; }

        /// <summary>
        /// Id of head to use with guard.
        /// </summary>
        public ushort Head { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public uint PointerRuntimeData { get; set; }

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

            BitUtility.InsertShortBig(bytes, pos, ObjectId);
            pos += Config.TargetShortSize;

            BitUtility.InsertShortBig(bytes, pos, Preset);
            pos += Config.TargetShortSize;

            BitUtility.InsertShortBig(bytes, pos, BodyId);
            pos += Config.TargetShortSize;

            BitUtility.InsertShortBig(bytes, pos, ActionPathAssignment);
            pos += Config.TargetShortSize;

            BitUtility.Insert32Big(bytes, pos, PresetToTrigger);
            pos += Config.TargetWordSize;

            BitUtility.InsertShortBig(bytes, pos, Unknown10);
            pos += Config.TargetShortSize;

            BitUtility.InsertShortBig(bytes, pos, Health);
            pos += Config.TargetShortSize;

            BitUtility.InsertShortBig(bytes, pos, ReactionTime);
            pos += Config.TargetShortSize;

            BitUtility.InsertShortBig(bytes, pos, Head);
            pos += Config.TargetShortSize;

            BitUtility.Insert32Big(bytes, pos, PointerRuntimeData);
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
                ObjectId,
                Preset);
            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                BodyId,
                ActionPathAssignment);
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(PresetToTrigger));
            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                Unknown10,
                Health);
            sb.Append(", ");
            sb.AppendFormat(
                Config.CMacro_WordFromShorts_Format,
                ReactionTime,
                Formatters.IntegralTypes.ToHex4(Head));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(PointerRuntimeData));
        }
    }
}
