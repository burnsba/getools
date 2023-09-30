using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol
{
    /// <summary>
    /// General interface to identify protocol meassage wraps a higher level message.
    /// </summary>
    /// <remarks>
    /// A bit experimental ...
    /// </remarks>
    public interface IEncapsulate
    {
        /// <summary>
        /// Inner message type.
        /// </summary>
        Type? InnerType { get; }

        /// <summary>
        /// Inner message, if set.
        /// </summary>
        object? InnerData { get; }
    }
}
