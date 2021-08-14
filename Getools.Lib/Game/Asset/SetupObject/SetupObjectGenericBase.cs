using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Base class for object list / propdef.
    /// </summary>
    public abstract class SetupObjectGenericBase : SetupObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectGenericBase"/> class.
        /// </summary>
        /// <param name="type">Type of object.</param>
        public SetupObjectGenericBase(PropDef type)
            : base(type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectGenericBase"/> class.
        /// For use when serializing.
        /// </summary>
        internal SetupObjectGenericBase()
        {
        }

        /// <summary>
        /// Gets or sets object id.
        /// </summary>
        public UInt16 ObjectId { get; set; }

        /// <summary>
        /// Gets or sets preset id.
        /// </summary>
        public UInt16 Preset { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Flags1 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Flags2 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 PointerPositionData { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 PointerObjInstanceController { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown18 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown1c { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown20 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown24 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown28 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown2c { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown30 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown34 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown38 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown3c { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown40 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown44 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown48 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown4c { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown50 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown54 { get; set; }

        /// <summary>
        /// Runtime x position.
        /// </summary>
        public UInt32 Xpos { get; set; }

        /// <summary>
        /// Runtime y position.
        /// </summary>
        public UInt32 Ypos { get; set; }

        /// <summary>
        /// Runtime z position.
        /// </summary>
        public UInt32 Zpos { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Bitflags { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 PointerCollisionBlock { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown6c { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown70 { get; set; }

        /// <summary>
        /// Health of object.
        /// </summary>
        public UInt16 Health { get; set; }

        /// <summary>
        /// Max health of object.
        /// </summary>
        public UInt16 MaxHealth { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown78 { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// </summary>
        public UInt32 Unknown7c { get; set; }

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
            sb.AppendFormat(Config.CMacro_WordFromShorts_Format, ObjectId, Preset);
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Flags1));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Flags2));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(PointerPositionData));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(PointerObjInstanceController));
            sb.Append(", ");
            sb.Append(Unknown18);
            sb.Append(", ");
            sb.Append(Unknown1c);
            sb.Append(", ");
            sb.Append(Unknown20);
            sb.Append(", ");
            sb.Append(Unknown24);
            sb.Append(", ");
            sb.Append(Unknown28);
            sb.Append(", ");
            sb.Append(Unknown2c);
            sb.Append(", ");
            sb.Append(Unknown30);
            sb.Append(", ");
            sb.Append(Unknown34);
            sb.Append(", ");
            sb.Append(Unknown38);
            sb.Append(", ");
            sb.Append(Unknown3c);
            sb.Append(", ");
            sb.Append(Unknown40);
            sb.Append(", ");
            sb.Append(Unknown44);
            sb.Append(", ");
            sb.Append(Unknown48);
            sb.Append(", ");
            sb.Append(Unknown4c);
            sb.Append(", ");
            sb.Append(Unknown50);
            sb.Append(", ");
            sb.Append(Unknown54);
            sb.Append(", ");
            sb.Append(Xpos);
            sb.Append(", ");
            sb.Append(Ypos);
            sb.Append(", ");
            sb.Append(Zpos);
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(Bitflags));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex8(PointerCollisionBlock));
            sb.Append(", ");
            sb.Append(Unknown6c);
            sb.Append(", ");
            sb.Append(Unknown70);
            sb.Append(", ");
            sb.AppendFormat(Config.CMacro_WordFromShorts_Format, Health, MaxHealth);
            sb.Append(", ");
            sb.Append(Unknown78);
            sb.Append(", ");
            sb.Append(Unknown7c);
        }
    }
}
