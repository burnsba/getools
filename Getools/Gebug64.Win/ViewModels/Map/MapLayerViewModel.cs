using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private UiMapLayerZSliceCompare _zSliceCompare;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapLayerViewModel"/> class.
        /// </summary>
        /// <param name="layerId">Type of layer.</param>
        public MapLayerViewModel(UiMapLayer layerId)
        {
            LayerId = layerId;
            _isVisible = true;

            // Set the comparison method based on the layer type.
            switch (layerId)
            {
                case UiMapLayer.Bg:
                case UiMapLayer.Stan:
                case UiMapLayer.SetupPathWaypoint:
                case UiMapLayer.SetupPatrolPath:
                    _zSliceCompare = UiMapLayerZSliceCompare.Bbox;
                    break;

                case UiMapLayer.Bond: // Bond: special case, just use origin point.
                    _zSliceCompare = UiMapLayerZSliceCompare.OriginPoint;
                    break;

                case UiMapLayer.SetupPad: // fallthrough
                case UiMapLayer.SetupAlarm:
                case UiMapLayer.SetupAmmo:
                case UiMapLayer.SetupAircraft:
                case UiMapLayer.SetupBodyArmor:
                case UiMapLayer.SetupGuard:
                case UiMapLayer.SetupCctv:
                case UiMapLayer.SetupCollectable:
                case UiMapLayer.SetupDoor:
                case UiMapLayer.SetupDrone:
                case UiMapLayer.SetupKey:
                case UiMapLayer.SetupSafe:
                case UiMapLayer.SetupSingleMontior:
                case UiMapLayer.SetupStandardProp:
                case UiMapLayer.SetupTank:
                case UiMapLayer.SetupIntro:
                    _zSliceCompare = UiMapLayerZSliceCompare.OriginPoint;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Type of layer
        /// </summary>
        public UiMapLayer LayerId { get; private set; }

        /// <summary>
        /// Explains how map objects in the layer should be compared to the Z min/max range.
        /// </summary>
        public UiMapLayerZSliceCompare ZSliceCompare => _zSliceCompare;

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
        public ObservableCollection<MapObject> Entities { get; set; } = new ObservableCollection<MapObject>();
    }
}
