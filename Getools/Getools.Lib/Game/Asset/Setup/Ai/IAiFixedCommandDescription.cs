using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// A "fixed" AI Command always has the same number of parameters / bytes.
    /// </summary>
    public interface IAiFixedCommandDescription : IAiCommandDescription
    {
        /// <summary>
        /// Number of parameters in command.
        /// </summary>
        int NumberParameters { get; set; }

        /// <summary>
        /// Container of command parameters.
        /// </summary>
        List<IAiParameter> CommandParameters { get; set; }
    }
}
