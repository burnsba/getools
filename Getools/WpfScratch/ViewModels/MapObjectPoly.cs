using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfScratch.ViewModels
{
    public class MapObjectPoly : MapObject
    {
        /// <summary>
        /// 2d x,z points in scaled coordinates.
        /// </summary>
        public PointCollection Points { get; set; }

        public MapObjectPoly()
        {
        }

        public MapObjectPoly(double x, double y)
            : base(x, y)
        {
        }
    }
}
