using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gebug64.Win.ViewModels.Map
{
    /// <summary>
    /// Container to draw wpf rectangle.
    /// </summary>
    public class MapObjectRect : MapObject
    {
        private double _rotationDegree = 0.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapObjectRect"/> class.
        /// </summary>
        public MapObjectRect()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapObjectRect"/> class.
        /// </summary>
        /// <param name="x"><see cref="UiX"/>.</param>
        /// <param name="y"><see cref="UiY"/>.</param>
        public MapObjectRect(double x, double y)
            : base(x, y)
        {
        }

        /// <summary>
        /// How much to rotate the rectangle.
        /// </summary>
        public double RotationDegree
        {
            get => _rotationDegree;
            set
            {
                if (_rotationDegree != value)
                {
                    _rotationDegree = value;
                    OnPropertyChanged(nameof(RotationDegree));
                }
            }
        }

        /// <summary>
        /// Updates object to new position, offset by half the width and height.
        /// </summary>
        /// <param name="uix">New x.</param>
        /// <param name="uiy">New y.</param>
        /// <param name="rotation">New rotation.</param>
        public void SetPositionLessHalf(double uix, double uiy, double rotation)
        {
            _uix = uix - (_uiWidth / 2.0);
            _uiy = uiy - (_uiHeight / 2.0);
            _rotationDegree = rotation;
            OnPropertyChanged(nameof(UiX));
            OnPropertyChanged(nameof(UiY));
            OnPropertyChanged(nameof(RotationDegree));
        }
    }
}
