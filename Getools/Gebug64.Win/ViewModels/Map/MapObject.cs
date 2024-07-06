using Gebug64.Win.Mvvm;

namespace Gebug64.Win.ViewModels.Map
{
    /// <summary>
    /// Base class for an object to be drawn on screen.
    /// </summary>
    public abstract class MapObject : ViewModelBase
    {
        private bool _isVisible;
        private double _uix;
        private double _uiy;
        private double _uiWidth;
        private double _uiHeight;

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
    }
}
