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
        public double RotationDegree { get; set; } = 0.0;
    }
}
