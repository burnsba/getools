using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Win.Enum;
using Gebug64.Win.Mvvm;

namespace Gebug64.Win.ViewModels.Map
{
    /// <summary>
    /// Logical collection of entities.
    /// </summary>
    public class MapLayerViewModel : ViewModelBase
    {
        private bool _isVisible;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapLayerViewModel"/> class.
        /// </summary>
        /// <param name="layerId">Type of layer.</param>
        public MapLayerViewModel(UiMapLayer layerId)
        {
            LayerId = layerId;
            _isVisible = true;
        }

        /// <summary>
        /// Type of layer
        /// </summary>
        public UiMapLayer LayerId { get; private set; }

        /// <summary>
        /// Whether or not to draw on screen.
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        /// <summary>
        /// Collection of entities in the layer.
        /// </summary>
        public List<MapObject> Entities { get; set; } = new List<MapObject>();
    }
}
