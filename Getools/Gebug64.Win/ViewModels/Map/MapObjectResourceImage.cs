using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gebug64.Win.ViewModels.Map
{
    /// <summary>
    /// Container to draw wpf image.
    /// </summary>
    public class MapObjectResourceImage : MapObject
    {
        private double _rotationDegree = 0.0;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private MapObjectResourceImage(MapObjectResourceImage src)
            : base(src)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _rotationDegree = src._rotationDegree;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapObjectResourceImage"/> class.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MapObjectResourceImage()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapObjectResourceImage"/> class.
        /// </summary>
        /// <param name="x"><see cref="UiX"/>.</param>
        /// <param name="y"><see cref="UiY"/>.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MapObjectResourceImage(double x, double y)
            : base(x, y)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        /// <summary>
        /// Path of image to be used.
        /// </summary>
        /// <remarks>
        /// This just needs to resolve for the wpf Image.Source binding.
        /// </remarks>
        public string ResourcePath { get; set; }

        /// <summary>
        /// How much to rotate the image.
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
        /// Create a copy of the current object.
        /// Does not copy <see cref="MapObject.DataSource"/>.
        /// </summary>
        /// <returns>Copy.</returns>
        public MapObjectResourceImage Clone()
        {
            return new MapObjectResourceImage(this);
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
