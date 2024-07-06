using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gebug64.Win.ViewModels.Map
{
    /// <summary>
    /// Container to draw wpf line.
    /// </summary>
    public class MapObjectLine : MapObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapObjectLine"/> class.
        /// </summary>
        public MapObjectLine()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapObjectLine"/> class.
        /// </summary>
        /// <param name="x"><see cref="UiX"/>.</param>
        /// <param name="y"><see cref="UiY"/>.</param>
        public MapObjectLine(double x, double y)
            : base(x, y)
        {
        }

        /// <summary>
        /// Origin point.
        /// </summary>
        public Point P1 { get; set; }

        /// <summary>
        /// End point relative to <see cref="P1"/>.
        /// </summary>
        public Point P2 { get; set; }
    }
}
