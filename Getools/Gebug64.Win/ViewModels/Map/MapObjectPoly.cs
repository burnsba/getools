using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gebug64.Win.ViewModels.Map
{
    /// <summary>
    /// Container to draw wpf closed polygon.
    /// </summary>
    public class MapObjectPoly : MapObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapObjectPoly"/> class.
        /// </summary>
        public MapObjectPoly()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapObjectPoly"/> class.
        /// </summary>
        /// <param name="x"><see cref="UiX"/>.</param>
        /// <param name="y"><see cref="UiY"/>.</param>
        public MapObjectPoly(double x, double y)
            : base(x, y)
        {
        }

        /// <summary>
        /// 2d x,z points in scaled coordinates.
        /// </summary>
        public PointCollection Points { get; set; } = new PointCollection();
    }
}
