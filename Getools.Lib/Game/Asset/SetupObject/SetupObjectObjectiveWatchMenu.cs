﻿using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for watch menu text definition.
    /// </summary>
    public class SetupObjectObjectiveWatchMenu : SetupObjectBase, ISetupObject
    {
        private const int _thisSize = 3 * Config.TargetWordSize;

        public const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectObjectiveWatchMenu"/> class.
        /// </summary>
        public SetupObjectObjectiveWatchMenu()
            : base(PropDef.WatchMenuObjectiveText)
        {
        }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x0.
        /// </summary>
        public int MenuOption { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x4.
        /// </summary>
        public int TextId { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x8.
        /// </summary>
        public int End { get; set; }

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

            BitUtility.Insert32Big(bytes, pos, MenuOption);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, TextId);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, End);
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
            sb.Append(MenuOption);
            sb.Append(", ");
            sb.Append(TextId);
            sb.Append(", ");
            sb.Append(End);
        }
    }
}
