﻿using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for ammo magazine.
    /// </summary>
    public class SetupObjectAmmoMag : SetupObjectGenericBase
    {
        private const int _thisSize = 1 * Config.TargetWordSize;

        public const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectAmmoMag"/> class.
        /// </summary>
        public SetupObjectAmmoMag()
            : base(PropDef.AmmoMag)
        {
        }

        /// <summary>
        /// Ammo type.
        /// See <see cref="Getools.Lib.Game.Enums.AmmoType"/>.
        /// </summary>
        public int AmmoType { get; set; }

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

            BitUtility.Insert32Big(bytes, pos, AmmoType);
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
            sb.Append(AmmoType);
        }
    }
}
