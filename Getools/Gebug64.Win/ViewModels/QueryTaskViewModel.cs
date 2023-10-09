using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Gebug64.Win.Mvvm;
using Gebug64.Win.QueryTask;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// Container for long running data requests, such as continuous position reporting,
    /// demo playback, demo recording, etc.
    /// </summary>
    public class QueryTaskViewModel : ViewModelBase
    {
        private readonly QueryTaskContext _context;
        private readonly Type _taskType;
        private bool _isCurrentlyCancelling = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTaskViewModel"/> class.
        /// </summary>
        /// <param name="context">Associated work.</param>
        /// <param name="deleteAction">Action to perform for <see cref="DeleteCommand"/>.</param>
        public QueryTaskViewModel(QueryTaskContext context, Action<QueryTaskViewModel> deleteAction)
        {
            _context = context;

            _taskType = context.GetType();

            Action deleteCallback = () => deleteAction(this);

            CancelCommand = new CommandHandler(Cancel, () => CanRequestCancel);
            DeleteCommand = new CommandHandler(deleteCallback, () => CanDelete);
        }

        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        public ICommand CancelCommand { get; init; }

        /// <summary>
        /// Gets the delete command.
        /// </summary>
        public ICommand DeleteCommand { get; init; }

        /// <summary>
        /// Current task state.
        /// </summary>
        public TaskState State => _context.State;

        /// <summary>
        /// <see cref="State"/> as string.
        /// </summary>
        public string StateDisplayName => State.ToString();

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
        /// Gets value indicating whether item can be removed from parent collection.
        /// </summary>
        public bool CanDelete => _context.State == TaskState.Stopped;

        /// <summary>
        /// Gets the underlying worker task type.
        /// </summary>
        public Type TaskType => _taskType;

        /// <summary>
        /// Start worker task.
        /// </summary>
        public void Begin()
        {
            _context.Begin();

            RefreshDisplayStatus();
        }

        /// <summary>
        /// Attempts to cancel the current task.
        /// </summary>
        public void Cancel()
        {
            _context.Cancel();

            RefreshDisplayStatus();
        }

        /// <summary>
        /// Sends view notification to refresh properties.
        /// </summary>
        public void RefreshDisplayStatus()
        {
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(StateDisplayName));
            OnPropertyChanged(nameof(StopReason));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(CanRequestCancel));
            OnPropertyChanged(nameof(CanDelete));
        }
    }
}
