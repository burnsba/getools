using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gebug64.Win.Config
{
    /// <summary>
    /// Describes window state and position.
    /// </summary>
    public record UiWindowState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UiWindowState"/> class.
        /// </summary>
        public UiWindowState()
        {
        }

        /// <summary>
        /// Full type of window or control.
        /// </summary>
        public string? TypeName { get; init; }

        /// <summary>
        /// Whether or not the window is currently visible.
        /// </summary>
        public bool IsVisible { get; init; }

        /// <summary>
        /// Current window state.
        /// </summary>
        public WindowState WindowState { get; init; }

        /// <summary>
        /// Width of the window.
        /// </summary>
        public double Width { get; init; }

        /// <summary>
        /// Height of the window.
        /// </summary>
        public double Height { get; init; }

        /// <summary>
        /// Distance from window left edge to its parent left.
        /// </summary>
        public double Left { get; init; }

        /// <summary>
        /// Distance from window top edge to its parent top.
        /// </summary>
        public double Top { get; init; }
    }
}
