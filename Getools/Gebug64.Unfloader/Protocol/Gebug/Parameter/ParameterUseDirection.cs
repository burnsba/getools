using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Parameter
{
    /// <summary>
    /// Describes when parameter should be included in message.
    /// </summary>
    public enum ParameterUseDirection
    {
        /// <summary>
        /// Default / never.
        /// </summary>
        Never,

        /// <summary>
        /// Source message (pc).
        /// </summary>
        PcToConsole,

        /// <summary>
        /// Reply message (console).
        /// </summary>
        ConsoleToPc,

        /// <summary>
        /// Always include.
        /// </summary>
        Both,
    }
}
