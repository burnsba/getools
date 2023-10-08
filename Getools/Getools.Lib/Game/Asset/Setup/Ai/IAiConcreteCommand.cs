using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// Interface to define a "concrete" command.
    /// This is in contrast to the command desription.
    /// </summary>
    public interface IAiConcreteCommand : IAiCommandDescription, IAiCmacro, IAiByteConvertable
    {
    }
}
