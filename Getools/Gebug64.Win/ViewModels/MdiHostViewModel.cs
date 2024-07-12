using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Gebug64.Win.Controls;
using Gebug64.Win.Mvvm;
using Gebug64.Win.Windows.Mdi;
using WPF.MDI;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// Main host conatiner viewmodel.
    /// </summary>
    public class MdiHostViewModel : WindowViewModelBase
    {
        private Action<Type, string>? _focusCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="MdiHostViewModel"/> class.
        /// </summary>
        public MdiHostViewModel()
        {
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
            // var args = (ResizeEventArgs)e;

            // System.Diagnostics.Debug.WriteLine("Resize child");
        }

        /// <summary>
        /// Move event handler for moving a child control.
        /// </summary>
        /// <param name="sender">Sender object (child).</param>
        /// <param name="e">Event args.</param>
        public void MoveChildHandler(object sender, RoutedEventArgs e)
        {
            // var args = (MoveEventArgs)e;

            // System.Diagnostics.Debug.WriteLine("Move child");
        }

        /// <summary>
        /// Close event handler for closing a child control.
        /// </summary>
        /// <param name="sender">Sender object (child).</param>
        /// <param name="e">Event args.</param>
        public void CloseChildHandler(object sender, RoutedEventArgs e)
        {
            // System.Diagnostics.Debug.WriteLine("Close child");
        }

        private void BuildViewWindowList()
        {
            Action<object?> dddd = x => InvokeFocusCallback((MenuItemViewModel)x!);

            var mivm = new MenuItemViewModel() { Header = Gebug64.Win.Ui.Lang.Window_MessageCenterTitle, Value = typeof(MainControl) };
            mivm.Command = new CommandHandler(dddd, () => true);

            MenuShowWindow.Add(mivm);

            mivm = new MenuItemViewModel() { Header = Gebug64.Win.Ui.Lang.Window_MapTitle, Value = typeof(MapControl) };
            mivm.Command = new CommandHandler(dddd, () => true);

            MenuShowWindow.Add(mivm);

            mivm = new MenuItemViewModel() { Header = Gebug64.Win.Ui.Lang.Window_LogTitle, Value = typeof(LogControl) };
            mivm.Command = new CommandHandler(dddd, () => true);

            MenuShowWindow.Add(mivm);

            mivm = new MenuItemViewModel() { Header = Gebug64.Win.Ui.Lang.Window_QueryTaskTitle, Value = typeof(QueryTasksControl) };
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
    }
}
