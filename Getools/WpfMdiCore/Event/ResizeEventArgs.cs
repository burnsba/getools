using System.Windows;

namespace WpfMdiCore
{
    public class ResizeEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets or sets width of child before move event.
        /// </summary>
        public double OldWidth { get; set; }

        /// <summary>
        /// Gets or sets height of child before move event.
        /// </summary>
        public double OldHeight { get; set; }

        /// <summary>
        /// Gets or sets width of child after move event.
        /// </summary>
        public double NewWidth { get; set; }

        /// <summary>
        /// Gets or sets height of child after move event.
        /// </summary>
        public double NewHeight { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizeEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event.</param>
        public ResizeEventArgs(RoutedEvent routedEvent) : base(routedEvent)
        {
        }
    }
}