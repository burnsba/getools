using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game
{
    public abstract class GameObjectHeaderBase : GameObjectBase
    {
        public UInt16 Scale { get; set; }

        public byte Hidden2Raw { get; set; }

        public byte TypeRaw { get; set; }

        public virtual string ToCInlineS32Array(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            AppendToCInlineS32Array(sb);

            return sb.ToString();
        }

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
