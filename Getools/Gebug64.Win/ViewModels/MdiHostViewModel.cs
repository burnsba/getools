using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Gebug64.Win.Controls;
using Gebug64.Win.Mvvm;
using Gebug64.Win.ViewModels.Config;
using Gebug64.Win.Windows.Mdi;
using WPF.MDI;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// Main host conatiner viewmodel.
    /// </summary>
    public class MdiHostViewModel : WindowViewModelBase
    {
        private const int _saveUiStateDelay = 1000; // ms

        private Action<Type, string>? _focusCallback;

        private object _timerLock = new object();
        private bool _saveUiStateTimerActive = false;
        private System.Timers.Timer _saveUiStateTimer;

        private bool _ready = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MdiHostViewModel"/> class.
        /// </summary>
        public MdiHostViewModel()
        {
            _saveUiStateTimer = new System.Timers.Timer();
            _saveUiStateTimer.Interval = _saveUiStateDelay; // ms
            _saveUiStateTimer.Enabled = false;
            _saveUiStateTimer.AutoReset = false;
            _saveUiStateTimer.Elapsed += NotifySaveUiState;

            BuildViewWindowList();
        }

        /// <summary>
        /// List of "standard windows" that should always be available to show.
        /// </summary>
        public ObservableCollection<MenuItemViewModel> MenuShowWindow { get; set; } = new ObservableCollection<MenuItemViewModel>();

        /// <summary>
        /// Passthrough helper to set the focus action for when an item from the "standard window list" is clicked.
        /// </summary>
        /// <param name="callback">Callback to execute.</param>
        public void SetFocusChildCallback(Action<Type, string> callback)
        {
            _focusCallback = callback;
        }

        /// <summary>
        /// Resize event handler for resizing a child control.
        /// </summary>
        /// <param name="sender">Sender object (child).</param>
        /// <param name="e">Event args.</param>
        public void ResizeChildHandler(object sender, RoutedEventArgs e)
        {
            StartExtendNotifySaveUiTimer();
        }

        /// <summary>
        /// Move event handler for moving a child control.
        /// </summary>
        /// <param name="sender">Sender object (child).</param>
        /// <param name="e">Event args.</param>
        public void MoveChildHandler(object sender, RoutedEventArgs e)
        {
            StartExtendNotifySaveUiTimer();
        }

        /// <summary>
        /// Close event handler for closing a child control.
        /// </summary>
        /// <param name="sender">Sender object (child).</param>
        /// <param name="e">Event args.</param>
        public void CloseChildHandler(object sender, RoutedEventArgs e)
        {
            StartExtendNotifySaveUiTimer();
        }

        /// <summary>
        /// Sets a flag to allow UI state changes to write to the appconfig file.
        /// (That is, startup/setup is ignored and won't write to appconfig).
        /// </summary>
        public void DoneInit()
        {
            _ready = true;
        }

        private void BuildViewWindowList()
        {
            Type t;
            Action<object?> dddd = x => InvokeFocusCallback((MenuItemViewModel)x!);

            t = typeof(MainControl);
            var mivm = new MenuItemViewModel() { Header = Gebug64.Win.Ui.Lang.GetDefaultWindowTitle(t), Value = t };
            mivm.Command = new CommandHandler(dddd, () => true);

            MenuShowWindow.Add(mivm);

            t = typeof(MapControl);
            mivm = new MenuItemViewModel() { Header = Gebug64.Win.Ui.Lang.GetDefaultWindowTitle(t), Value = t };
            mivm.Command = new CommandHandler(dddd, () => true);

            MenuShowWindow.Add(mivm);

            t = typeof(LogControl);
            mivm = new MenuItemViewModel() { Header = Gebug64.Win.Ui.Lang.GetDefaultWindowTitle(t), Value = t };
            mivm.Command = new CommandHandler(dddd, () => true);

            MenuShowWindow.Add(mivm);

            t = typeof(QueryTasksControl);
            mivm = new MenuItemViewModel() { Header = Gebug64.Win.Ui.Lang.GetDefaultWindowTitle(t), Value = t };
            mivm.Command = new CommandHandler(dddd, () => true);

            MenuShowWindow.Add(mivm);

            t = typeof(MemoryControl);
            mivm = new MenuItemViewModel() { Header = Gebug64.Win.Ui.Lang.GetDefaultWindowTitle(t), Value = t };
            mivm.Command = new CommandHandler(dddd, () => true);

            MenuShowWindow.Add(mivm);
        }

        private void InvokeFocusCallback(MenuItemViewModel mivm)
        {
            if (object.ReferenceEquals(null, mivm.Value))
            {
                throw new NullReferenceException();
            }

            if (object.ReferenceEquals(null, mivm.Header))
            {
                throw new NullReferenceException();
            }

            _focusCallback?.Invoke((Type)mivm.Value, mivm.Header);
        }

        /// <summary>
        /// Assuming <see cref="DoneInit"/> has already been called, timer event handler
        /// to save UI state to appconfig.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void NotifySaveUiState(object? sender, ElapsedEventArgs e)
        {
            _saveUiStateTimerActive = false;

            if (!_ready)
            {
                return;
            }

            Workspace.Instance.SaveUiState();
        }

        /// <summary>
        /// Event handler called when window changes state, moves, or resizes.
        /// Starts a timer to call <see cref="NotifySaveUiState"/>, or if the timer
        /// is already running stops it and restarts it to extend the timeout.
        /// </summary>
        private void StartExtendNotifySaveUiTimer()
        {
            if (!_ready)
            {
                return;
            }

            lock (_timerLock)
            {
                if (!_saveUiStateTimerActive)
                {
                    _saveUiStateTimerActive = true;
                    _saveUiStateTimer.Start();
                    return;
                }

                _saveUiStateTimer.Stop();
                _saveUiStateTimer.Start();
            }
        }
    }
}
