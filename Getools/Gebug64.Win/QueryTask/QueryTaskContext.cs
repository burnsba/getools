using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.QueryTask
{
    /// <summary>
    /// Base class to define a long running or periodic task that sends data to/from
    /// the connected flashcart.
    /// </summary>
    public abstract class QueryTaskContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTaskContext"/> class.
        /// </summary>
        public QueryTaskContext()
        {
        }

        /// <summary>
        /// Gets a value indicating whether more than one instance of this type of task
        /// can be running at the same time.
        /// </summary>
        public abstract bool TaskIsUnique { get; }

        /// <summary>
        /// Gets the description text to show in UI.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Current task state.
        /// </summary>
        public TaskState State { get; protected set; }

        /// <summary>
        /// Reason task is no longer running.
        /// </summary>
        public TaskStopReason StopReason { get; protected set; }

        /// <summary>
        /// Start worker task.
        /// </summary>
        public abstract void Begin();

        /// <summary>
        /// Attempts to cancel the current task.
        /// </summary>
        public abstract void Cancel();
    }
}
