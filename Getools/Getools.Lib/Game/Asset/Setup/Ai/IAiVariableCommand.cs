using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// Interface to define variable length command, concrete implementation.
    /// </summary>
    public interface IAiVariableCommand : IAiConcreteCommand
    {
        /// <summary>
        /// Variable length command data.
        /// </summary>
        byte[]? CommandData { get; set; }
    }
}
