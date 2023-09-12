using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game
{
    /// <summary>
    /// Generic base class for all setup-related objects.
    /// Should coorespond to type `struct ObjHeaderData`.
    /// </summary>
    public abstract class GameObjectHeaderBase : GameObjectBase, IGameObjectHeader
    {
        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public const int SizeOf = Config.TargetWordSize;

        /// <inheritdoc />
        public UInt16 Scale { get; set; }

        /// <inheritdoc />
        public byte Hidden2Raw { get; set; }

        /// <inheritdoc />
        public byte TypeRaw { get; set; }

        /// <inheritdoc />
        public virtual string ToCInlineS32Array(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            AppendToCInlineS32Array(sb);

            return sb.ToString();
        }

        /// <summary>
        /// Converts the current object to a byte array.
        /// Alignment is not considered.
        /// </summary>
        /// <returns>Byte array of object.</returns>
        public byte[] ToByteArray()
        {
            var bytes = new byte[4];

            BitUtility.InsertShortBig(bytes, 0, Scale);
            bytes[2] = Hidden2Raw;
            bytes[3] = TypeRaw;

            return bytes;
        }

        /// <summary>
        /// Base class implementation to list the object
        /// header data in a c declaration.
        /// </summary>
        /// <param name="sb">String builder to append to.</param>
        protected virtual void AppendToCInlineS32Array(StringBuilder sb)
        {
            sb.AppendFormat(
                Config.CMacro_WordFromShortByteByte(
                    Scale,
                    Hidden2Raw,
                    TypeRaw));
        }
    }
}
