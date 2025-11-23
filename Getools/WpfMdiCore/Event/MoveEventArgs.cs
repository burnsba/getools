using System.Windows;

namespace WpfMdiCore
{
    public class MoveEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets or sets position of child before move event.
        /// </summary>
        public System.Windows.Point OldPosition { get; set; }

        /// <summary>
        /// Gets or sets position of child after move event.
        /// </summary>
        public System.Windows.Point NewPosition { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event.</param>
        public MoveEventArgs(RoutedEvent routedEvent) : base(routedEvent)
        {
        }
    }
}