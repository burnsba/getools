﻿using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup definition for surveilance camera.
    /// </summary>
    public class SetupObjectCctv : SetupObjectGenericBase
    {
        private const int _dataSize = 108;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectCctv"/> class.
        /// </summary>
        public SetupObjectCctv()
            : base(PropDef.Cctv)
        {
        }

        /// <summary>
        /// Object data.
        /// TODO: determine real properties.
        /// </summary>
        public byte[] Data { get; set; } = new byte[_dataSize];

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

            var s32count = _dataSize / 4;
            int dataOffset = 0;
            for (int i = 0; i < s32count; i++, dataOffset += 4)
            {
                sb.Append(", ");
                sb.Append(Formatters.IntegralTypes.ToHex8(BitUtility.Read32Big(Data, dataOffset)));
            }
        }
    }
}