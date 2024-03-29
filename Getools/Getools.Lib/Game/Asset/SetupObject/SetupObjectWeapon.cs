﻿using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Setup object list / prop definition for weapon.
    /// </summary>
    public class SetupObjectWeapon : SetupObjectGenericBase
    {
        private const int _thisSize = 2 * Config.TargetWordSize;

        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public new const int SizeOf = GameObjectHeaderBase.SizeOf + _thisSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectWeapon"/> class.
        /// </summary>
        public SetupObjectWeapon()
            : base(PropDef.Collectable)
        {
        }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x0.
        /// </summary>
        public byte GunPickup { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x1.
        /// </summary>
        public byte LinkedItem { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x2.
        /// </summary>
        public UInt16 Timer { get; set; }

        /// <summary>
        /// TODO: unknown.
        /// Struct offset 0x4.
        /// </summary>
        public UInt32 PointerLinkedItem { get; set; }

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

            bytes[pos] = GunPickup;
            pos++;

            bytes[pos] = LinkedItem;
            pos++;

            BitUtility.InsertShortBig(bytes, pos, Timer);
            pos += Config.TargetShortSize;

            BitUtility.Insert32Big(bytes, pos, PointerLinkedItem);
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
            sb.AppendFormat(
                Config.CMacro_WordFromByteByteShort(
                    Formatters.IntegralTypes.ToHex2(GunPickup),
                    Formatters.IntegralTypes.ToHex2(LinkedItem),
                    Formatters.IntegralTypes.ToHex4(Timer)));
            sb.Append(", ");
            sb.Append(PointerLinkedItem);
        }
    }
}
