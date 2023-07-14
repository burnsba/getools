using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public interface IAiCmacro
    {
        void CMacroAppend(string prefix, StringBuilder sb);
    }
}
