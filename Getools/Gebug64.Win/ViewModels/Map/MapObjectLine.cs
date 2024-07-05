using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gebug64.Win.ViewModels.Map
{
    public class MapObjectLine : MapObject
    {
        public Point P1 { get; set; }
        public Point P2 { get; set; }

        public MapObjectLine()
        {
        }

        public MapObjectLine(double x, double y)
            : base(x, y)
        {
        }
    }
}
