using System.Collections.Generic;
using System.Windows.Documents;
using Gebug64.Win.Game;
using Gebug64.Win.Mvvm;
using Getools.Lib.Game;

namespace Gebug64.Win.ViewModels.Map
{
    /// <summary>
    /// Base class for an object to be drawn on screen.
    /// </summary>
    public abstract class MapObject : ViewModelBase
    {
        private bool _isVisible;

        /// <summary>
        /// Backer field: X position for drawing on screen.
        /// </summary>
        protected double _uix;

        /// <summary>
        /// Backer field: Y position for drawing on screen.
        /// </summary>
        protected double _uiy;

        /// <summary>
        /// Backer field: Width for drawing on screen.
        /// </summary>
        protected double _uiWidth;

        /// <summary>
        /// Backer field: Height for drawing on screen.
        /// </summary>
        protected double _uiHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapObject"/> class.
        /// </summary>
        /// <param name="src">Object to copy.</param>
        protected MapObject(MapObject src)
        {
            _isVisible = src._isVisible;
            _uix = src._uix;
            _uiy = src._uiy;
            _uiWidth = src._uiWidth;
            _uiHeight = src._uiHeight;
            ScaledOrigin = src.ScaledOrigin.Clone();
            ScaledMin = src.ScaledMin.Clone();
            ScaledMax = src.ScaledMax.Clone();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapObject"/> class.
        /// </summary>
        public MapObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapObject"/> class.
        /// </summary>
        /// <param name="x"><see cref="UiX"/>.</param>
        /// <param name="y"><see cref="UiY"/>.</param>
        public MapObject(double x, double y)
        {
            _uix = x;
            _uiy = y;
        }

        /// <summary>
        /// X position for drawing on screen.
        /// </summary>
        public double UiX
        {
            get => _uix;
            set
            {
                if (_uix != value)
                {
                    _uix = value;
                    OnPropertyChanged(nameof(UiX));
                }
            }
        }

        /// <summary>
        /// Y position for drawing on screen.
        /// </summary>
        public double UiY
        {
            get => _uiy;
            set
            {
                if (_uiy != value)
                {
                    _uiy = value;
                    OnPropertyChanged(nameof(UiY));
                }
            }
        }

        /// <summary>
        /// Width for drawing on screen.
        /// </summary>
        public double UiWidth
        {
            get => _uiWidth;
            set
            {
                if (_uiWidth != value)
                {
                    _uiWidth = value;
                    OnPropertyChanged(nameof(UiWidth));
                }
            }
        }

        /// <summary>
        /// Height for drawing on screen.
        /// </summary>
        public double UiHeight
        {
            get => _uiHeight;
            set
            {
                if (_uiHeight != value)
                {
                    _uiHeight = value;
                    OnPropertyChanged(nameof(UiHeight));
                }
            }
        }

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
        /// Stroke.
        /// </summary>
        public System.Windows.Media.Brush? Stroke { get; set; }

        /// <summary>
        /// Stroke thickness.
        /// </summary>
        public double? StrokeThickness { get; set; }

        /// <summary>
        /// Object fill.
        /// </summary>
        public System.Windows.Media.Brush? Fill { get; set; }

        /// <summary>
        /// Gets or sets the origin (associated pad origin).
        /// </summary>
        public Coord3dd ScaledOrigin { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// Stage scaled coordinate value.
        /// </summary>
        public Coord3dd ScaledMin { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// Stage scaled coordinate value.
        /// </summary>
        public Coord3dd ScaledMax { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// Backer or reference object that is being drawn on the map.
        /// </summary>
        public GameObject? DataSource { get; set; }

        /// <summary>
        /// Related child objects to show on the map.
        /// </summary>
        public List<MapObject> Children { get; set; } = new List<MapObject>();

        /// <summary>
        /// Optional parent of this instance.
        /// </summary>
        public MapObject? Parent { get; set; }

        /// <summary>
        /// Updates object to new position, offset by half the width and height.
        /// </summary>
        /// <param name="uix">New x.</param>
        /// <param name="uiy">New y.</param>
        public void SetPositionLessHalf(double uix, double uiy)
        {
            _uix = uix - (_uiWidth / 2.0);
            _uiy = uiy - (_uiHeight / 2.0);
            OnPropertyChanged(nameof(UiX));
            OnPropertyChanged(nameof(UiY));
        }
    }
}
