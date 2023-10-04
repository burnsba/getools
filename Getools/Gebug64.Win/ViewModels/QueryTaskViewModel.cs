using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Win.QueryTask;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// Container for long running data requests, such as continuous position reporting,
    /// demo playback, demo recording, etc.
    /// </summary>
    public class QueryTaskViewModel
    {
        private readonly QueryTaskContext _context;
        private readonly Type _taskType;
        private bool _isCurrentlyCancelling = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTaskViewModel"/> class.
        /// </summary>
        /// <param name="context">Associated work.</param>
        public QueryTaskViewModel(QueryTaskContext context)
        {
            _context = context;

            _taskType = context.GetType();
        }

        /// <summary>
        /// Current task state.
        /// </summary>
        public TaskState State => _context.State;

        /// <summary>
        /// Reason task is no longer running.
        /// </summary>
        public TaskStopReason StopReason => _context.StopReason;

        /// <summary>
        /// Gets a value indicating whether more than one instance of this type of task
        /// can be running at the same time.
        /// </summary>
        public bool TaskIsUnique => _context.TaskIsUnique;

        /// <summary>
        /// Gets the description text to show in UI.
        /// </summary>
        public string DisplayName => _context.DisplayName;

        /// <summary>
        /// Gets a value indicating whether the current task can be cancelled.
        /// </summary>
        public bool CanRequestCancel => _isCurrentlyCancelling == false && State == TaskState.Running;

        /// <summary>
        /// Gets the underlying worker task type.
        /// </summary>
        public Type TaskType => _taskType;

        /// <summary>
        /// Start worker task.
        /// </summary>
        public void Begin() => _context.Begin();
    }
}
