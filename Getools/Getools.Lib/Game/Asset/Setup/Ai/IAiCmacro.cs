using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// Interface to define something that can be written as a C macro.
    /// </summary>
    public interface IAiCmacro
    {
        /// <summary>
        /// Writes object into string builder as C macro definition.
        /// </summary>
        /// <param name="prefix">Whitespace indentation.</param>
        /// <param name="sb">String builder to write to.</param>
        void CMacroAppend(string prefix, StringBuilder sb);
    }
}
