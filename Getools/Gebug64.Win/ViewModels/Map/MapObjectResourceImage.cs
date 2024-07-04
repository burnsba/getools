using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gebug64.Win.ViewModels.Map
{
    public class MapObjectResourceImage : MapObject
    {
        public string ResourcePath { get; set; }

        public double RotationDegree { get; set; } = 0.0;

        public MapObjectResourceImage()
        {
        }

        public MapObjectResourceImage(double x, double y)
            : base(x, y)
        {
        }
    }
}
