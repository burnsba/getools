using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gebug64.Win.ViewModels.Map
{
    public class MapObjectPolyLine : MapObject
    {
        /// <summary>
        /// 2d x,z points in scaled coordinates.
        /// </summary>
        public PointCollection Points { get; set; }

        public MapObjectPolyLine()
        {
        }

        public MapObjectPolyLine(double x, double y)
            : base(x, y)
        {
        }
    }
}
