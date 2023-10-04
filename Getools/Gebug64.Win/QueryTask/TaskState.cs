using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.QueryTask
{
    /// <summary>
    /// Current task worker state.
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// Task hasn't been started yet.
        /// </summary>
        NotStarted,

        /// <summary>
        /// Task is currently running.
        /// </summary>
        Running,

        /// <summary>
        /// Task is no longer running.
        /// </summary>
        Stopped,
    }
}
