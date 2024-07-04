using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Map
{
    public class MapLayerViewModel
    {
        public List<MapObject> Entities { get; set; } = new List<MapObject>();

        public MapLayerViewModel()
        {
        }
    }
}
