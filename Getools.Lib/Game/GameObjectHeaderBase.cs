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
