using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gebug64.Win.ViewModels.Map
{
    /// <summary>
    /// Container to draw wpf rectangle.
    /// </summary>
    public class MapObjectRect : MapObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapObjectRect"/> class.
        /// </summary>
        public MapObjectRect()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapObjectRect"/> class.
        /// </summary>
        /// <param name="x"><see cref="UiX"/>.</param>
        /// <param name="y"><see cref="UiY"/>.</param>
        public MapObjectRect(double x, double y)
            : base(x, y)
        {
        }

        /// <summary>
        /// How much to rotate the rectangle.
        /// </summary>
        public double RotationDegree { get; set; } = 0.0;
    }
}
