using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Parse
{
    /// <summary>
    /// Binary parse success.
    /// </summary>
    public enum PacketParseStatus
    {
        /// <summary>
        /// Unset default value.
        /// </summary>
        DefaultUnknown,

        /// <summary>
        /// Successful parse.
        /// </summary>
        Success,

        /// <summary>
        /// Fail to parse.
        /// </summary>
        Error,
    }
}
