using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.QueryTask
{
    /// <summary>
    /// Why the task is no longer running.
    /// </summary>
    public enum TaskStopReason
    {
        /// <summary>
        /// Unitialized.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Task had a "natural" ending.
        /// </summary>
        GracefulStop,

        /// <summary>
        /// Exception encountered.
        /// </summary>
        Error,

        /// <summary>
        /// User cancelled task.
        /// </summary>
        UserCancel,
    }
}
