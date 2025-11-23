using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Gebug64.Win.Config;
using Gebug64.Win.ViewModels.Config;
using Gebug64.Win.Windows;
using WpfMdiCore;

namespace Gebug64.Win.Extensions
{
    /// <summary>
    /// Extension methods for WPF controls and windows.
    /// </summary>
    public static class WpfControlExtensions
    {
        /// <summary>
        /// Gets the current UI state for the window.
        /// </summary>
        /// <param name="mdiChild">Window.</param>
        /// <returns>Current state.</returns>
        /// <exception cref="NullReferenceException">Thrown if parameter is null.</exception>
        /// <exception cref="NotSupportedException">Thrown if parameter doesn't implement <see cref="ILayoutWindow"/>.</exception>
        public static UiWindowState GetWindowLayoutState(this MdiChild mdiChild)
        {
            UserControl? control = mdiChild.Content as UserControl;

            if (object.ReferenceEquals(null, control))
            {
                throw new NullReferenceException();
            }

            var windowType = control.GetType();

            if (!typeof(ILayoutWindow).IsAssignableFrom(windowType))
            {
                throw new NotSupportedException();
            }

            var windowId = (ILayoutWindow)control;

            var result = new UiWindowState()
            {
                TypeName = windowType.FullName!,
                IsVisible = true,
                Width = mdiChild.Width,
                Height = mdiChild.Height,
                WindowState = mdiChild.WindowState,
                Left = mdiChild.Position.X,
                Top = mdiChild.Position.Y,
            };

            return result;
        }

        /// <summary>
        /// Sets the window size, position, and window state.
        /// </summary>
        /// <param name="mdiChild">Window.</param>
        /// <param name="state">UI state.</param>
        public static void SetWindowLayoutState(this MdiChild mdiChild, UiWindowState state)
        {
            mdiChild.Position = new Point(state.Left, state.Top);
            mdiChild.Width = state.Width;
            mdiChild.Height = state.Height;
            mdiChild.WindowState = state.WindowState;

            if (!state.IsVisible)
            {
                mdiChild.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// For the given window, checks to see if there is existing state information
        /// in the appconfig file, and if so loads the state with <see cref="SetWindowLayoutState"/>.
        /// </summary>
        /// <param name="mdiChild">Window.</param>
        /// <param name="appConfig">App config.</param>
        /// <exception cref="NullReferenceException">Thrown if parameter is null.</exception>
        /// <exception cref="NotSupportedException">Thrown if parameter doesn't implement <see cref="ILayoutWindow"/>.</exception>
        public static void TryLoadWindowLayoutState(this MdiChild mdiChild, AppConfigViewModel appConfig)
        {
            UserControl? control = mdiChild.Content as UserControl;

            if (object.ReferenceEquals(null, control))
            {
                throw new NullReferenceException();
            }

            if (!typeof(ILayoutWindow).IsAssignableFrom(control.GetType()))
            {
                throw new NotSupportedException();
            }

            var windowId = (ILayoutWindow)control;
            var refname = windowId.TypeName!;

            List<UiWindowState> windows = appConfig?.LayoutState?.Windows ?? new List<UiWindowState>();

            var layoutState = windows.FirstOrDefault(x => x.TypeName == refname);

            if (!object.ReferenceEquals(null, layoutState))
            {
                SetWindowLayoutState(mdiChild, layoutState);
            }
        }

        /// <summary>
        /// Gets the current UI state for the window.
        /// </summary>
        /// <param name="window">Window.</param>
        /// <returns>Current state.</returns>
        /// <exception cref="NullReferenceException">Thrown if parameter is null.</exception>
        /// <exception cref="NotSupportedException">Thrown if parameter doesn't implement <see cref="ILayoutWindow"/>.</exception>
        public static UiWindowState GetWindowLayoutState(this Window window)
        {
            if (object.ReferenceEquals(null, window))
            {
                throw new NullReferenceException();
            }

            var windowType = window.GetType();

            if (!typeof(ILayoutWindow).IsAssignableFrom(windowType))
            {
                throw new NotSupportedException();
            }

            var windowId = (ILayoutWindow)window;

            var result = new UiWindowState()
            {
                TypeName = windowType.FullName!,
                IsVisible = true,
                Width = window.ActualWidth,
                Height = window.ActualHeight,
                WindowState = window.WindowState,
                Left = window.Left,
                Top = window.Top,
            };

            return result;
        }

        /// <summary>
        /// Sets the window size, position, and window state.
        /// </summary>
        /// <param name="window">Window.</param>
        /// <param name="state">UI state.</param>
        public static void SetWindowLayoutState(this Window window, UiWindowState state)
        {
            window.Top = state.Top;
            window.Left = state.Left;
            window.Width = state.Width;
            window.Height = state.Height;
            window.WindowState = state.WindowState;

            if (!state.IsVisible)
            {
                window.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// For the given window, checks to see if there is existing state information
        /// in the appconfig file, and if so loads the state with <see cref="SetWindowLayoutState"/>.
        /// </summary>
        /// <param name="window">Window.</param>
        /// <param name="appConfig">App config.</param>
        /// <exception cref="NullReferenceException">Thrown if parameter is null.</exception>
        /// <exception cref="NotSupportedException">Thrown if parameter doesn't implement <see cref="ILayoutWindow"/>.</exception>
        public static void TryLoadWindowLayoutState(this Window window, AppConfigViewModel appConfig)
        {
            if (object.ReferenceEquals(null, window))
            {
                throw new NullReferenceException();
            }

            if (!typeof(ILayoutWindow).IsAssignableFrom(window.GetType()))
            {
                throw new NotSupportedException();
            }

            var windowId = (ILayoutWindow)window;
            var refname = windowId.TypeName!;

            List<UiWindowState> windows = appConfig?.LayoutState?.Windows ?? new List<UiWindowState>();

            var layoutState = windows.FirstOrDefault(x => x.TypeName == refname);

            if (!object.ReferenceEquals(null, layoutState))
            {
                SetWindowLayoutState(window, layoutState);
            }
        }
    }
}
