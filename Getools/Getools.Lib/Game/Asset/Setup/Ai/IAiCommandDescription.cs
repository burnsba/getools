using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// Base interface to desribe an AI Command.
    /// </summary>
    public interface IAiCommandDescription
    {
        /// <summary>
        /// Friendly name.
        /// </summary>
        string? DecompName { get; set; }

        /// <summary>
        /// AI Command byte.
        /// </summary>
        byte CommandId { get; set; }

        /// <summary>
        /// Gets or sets length of all byte data, including leading commandid byte.
        /// </summary>
        int CommandLengthBytes { get; set; }
    }
}
