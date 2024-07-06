using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Map
{
    /// <summary>
    /// Logical collection of entities.
    /// </summary>
    public class MapLayerViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapLayerViewModel"/> class.
        /// </summary>
        public MapLayerViewModel()
        {
        }

        /// <summary>
        /// Collection of entities in the layer.
        /// </summary>
        public List<MapObject> Entities { get; set; } = new List<MapObject>();
    }
}
