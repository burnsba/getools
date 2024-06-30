using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfScratch.ViewModels
{
    public class MapLayerViewModel
    {
        public System.Windows.Media.Brush Stroke { get; set; }

        public double StrokeThickness { get; set; }

        public System.Windows.Media.Brush Fill { get; set; }

        public List<MapObject> Entities { get; set; } = new List<MapObject>();

        public MapLayerViewModel()
        {
        }
    }
}
