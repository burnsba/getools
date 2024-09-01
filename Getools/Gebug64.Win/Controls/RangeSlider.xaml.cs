using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gebug64.Win.Controls
{
    /// <summary>
    /// Interaction logic for RangeSlider.xaml
    /// </summary>
    /// <remarks>
    /// Based on https://stackoverflow.com/a/27287315
    /// </remarks>
    public partial class RangeSlider : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="Minimum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        /// <summary>
        /// Identifies the <see cref="LowerValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LowerValueProperty =
            DependencyProperty.Register("LowerValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d, null, LowerValueCoerceValueCallback));

        /// <summary>
        /// Identifies the <see cref="UpperValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UpperValueProperty =
            DependencyProperty.Register("UpperValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(1d, null, UpperValueCoerceValueCallback));

        /// <summary>
        /// Identifies the <see cref="Maximum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(1d));

        /// <summary>
        /// Identifies the <see cref="IsSnapToTickEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSnapToTickEnabledProperty =
            DependencyProperty.Register("IsSnapToTickEnabled", typeof(bool), typeof(RangeSlider), new UIPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="TickFrequency"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0.1d));

        /// <summary>
        /// Identifies the <see cref="TickPlacement"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TickPlacementProperty =
            DependencyProperty.Register("TickPlacement", typeof(TickPlacement), typeof(RangeSlider), new UIPropertyMetadata(TickPlacement.None));

        /// <summary>
        /// Identifies the <see cref="Ticks"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TicksProperty =
            DependencyProperty.Register("Ticks", typeof(DoubleCollection), typeof(RangeSlider), new UIPropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeSlider"/> class.
        /// </summary>
        public RangeSlider()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the System.Windows.Controls.Primitives.RangeBase.Minimum possible
        /// System.Windows.Controls.Primitives.RangeBase.Value of the range element.
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Gets or sets the current lower magnitude of the range control.
        /// </summary>
        public double LowerValue
        {
            get { return (double)GetValue(LowerValueProperty); }
            set { SetValue(LowerValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the current upper magnitude of the range control.
        /// </summary>
        public double UpperValue
        {
            get { return (double)GetValue(UpperValueProperty); }
            set { SetValue(UpperValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the highest possible System.Windows.Controls.Primitives.RangeBase.Value of the range element.
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the System.Windows.Controls.Slider
        /// automatically moves the System.Windows.Controls.Primitives.Track.Thumb to the
        /// closest tick mark.
        /// </summary>
        public bool IsSnapToTickEnabled
        {
            get { return (bool)GetValue(IsSnapToTickEnabledProperty); }
            set { SetValue(IsSnapToTickEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the interval between tick marks.
        /// </summary>
        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        /// <summary>
        /// Gets or sets the position of tick marks with respect to the System.Windows.Controls.Primitives.Track
        /// of the System.Windows.Controls.Slider.
        /// </summary>
        public TickPlacement TickPlacement
        {
            get { return (TickPlacement)GetValue(TickPlacementProperty); }
            set { SetValue(TickPlacementProperty, value); }
        }

        /// <summary>
        /// Gets or sets the positions of the tick marks to display for a System.Windows.Controls.Slider.
        /// </summary>
        public DoubleCollection Ticks
        {
            get { return (DoubleCollection)GetValue(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }

        private static object LowerValueCoerceValueCallback(DependencyObject target, object valueObject)
        {
            RangeSlider targetSlider = (RangeSlider)target;
            double value = (double)valueObject;

            return Math.Min(value, targetSlider.UpperValue);
        }

        private static object UpperValueCoerceValueCallback(DependencyObject target, object valueObject)
        {
            RangeSlider targetSlider = (RangeSlider)target;
            double value = (double)valueObject;

            return Math.Max(value, targetSlider.LowerValue);
        }
    }
}
