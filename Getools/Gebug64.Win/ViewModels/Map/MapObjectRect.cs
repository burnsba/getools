using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gebug64.Win.ViewModels.Map
{
    public class MapObjectRect : MapObject
    {
        public double RotationDegree { get; set; } = 0.0;

        public MapObjectRect()
        {
        }

        public MapObjectRect(double x, double y)
            : base(x, y)
        {
        }
    }
}
