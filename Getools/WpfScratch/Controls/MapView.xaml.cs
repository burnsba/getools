using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
using WpfScratch.ViewModels;

namespace WpfScratch.Controls
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        private readonly ScrollViewer _scrollViewer;
        private readonly UIElement _content;
        private Point _scrollMousePoint;
        private double _hOff = 1;
        private double _vOff = 1;
        private bool _mouseCaptured = false;

        private double _uiScale = 1.0;

        private MapViewModel _vm;
        private bool _init = false;

        public MapView()
        {
            InitializeComponent();

            // DataCont

            _content = this;
            _scrollViewer = MainScrollViewer;
        }

        private void _vm_NotifyChildChangedEvent(object? sender, EventArgs e)
        {
            MainContent.Items.Refresh();
        }

        // https://stackoverflow.com/a/41985834/1462295
        public static T? FindParentOfType<T>(DependencyObject? child) where T : DependencyObject
        {
            if (object.ReferenceEquals(null, child))
            {
                return null;
            }

            DependencyObject parentDepObj = child;
            do
            {
                parentDepObj = VisualTreeHelper.GetParent(parentDepObj);
                T? parent = parentDepObj as T;
                if (parent != null)
                {
                    return parent;
                }
            }
            while (parentDepObj != null);

            return null;
        }

        public static T? FindParentByRef<T>(DependencyObject? child, T obj) where T : DependencyObject
        {
            if (object.ReferenceEquals(null, child))
            {
                return null;
            }

            DependencyObject parentDepObj = child;
            do
            {
                parentDepObj = VisualTreeHelper.GetParent(parentDepObj);
                T? parent = parentDepObj as T;
                if (parent != null && object.ReferenceEquals(parent, obj))
                {
                    return parent;
                }
            }
            while (parentDepObj != null);

            return null;
        }

        private void scrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!MainScrollViewer.IsMouseOver)
            {
                return;
            }

            // The mouse events are bound on the overall usercontrol. However, the pan/zoom area
            // consumes the scrollviewer edges including the scrollbars. When a user clicks on the
            // scrollbars, instead of scrolling this begins the pan logic.
            // Check if the mouse event is contained within a scrollbar, and if so, ignore it.
            var a = FindParentOfType<ScrollBar>((DependencyObject)e.OriginalSource);
            var b = FindParentByRef((DependencyObject)a, MainScrollViewer);
            if (!object.ReferenceEquals(null, b))
            {
                return;
            }

            _content.CaptureMouse();
            _mouseCaptured = true;

            _scrollMousePoint = e.GetPosition(_scrollViewer);
            _hOff = _scrollViewer.HorizontalOffset;
            _vOff = _scrollViewer.VerticalOffset;
        }

        private void scrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // If somehow the mouseup event was missed, fix the state.
            if (e.LeftButton == MouseButtonState.Released)
            {
                _mouseCaptured = false;
            }

            // There seems to be some delay on the mouse capture method, so key off the
            // instance boolean instead.
            if (!_mouseCaptured)
            {
                return;
            }

            var newOffsetY = _vOff + (_scrollMousePoint.Y - e.GetPosition(_scrollViewer).Y);
            _scrollViewer.ScrollToVerticalOffset(newOffsetY);

            var newOffsetX = _hOff + (_scrollMousePoint.X - e.GetPosition(_scrollViewer).X);
            _scrollViewer.ScrollToHorizontalOffset(newOffsetX);
        }

        private void scrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _content.ReleaseMouseCapture();
            _mouseCaptured = false;

            var end = e.GetPosition(_scrollViewer);
        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!_content.IsMouseOver)
            {
                return;
            }

            // Don't zoom if currently panning.
            if (_mouseCaptured)
            {
                return;
            }

            double startScale = _uiScale;
            double newScale = _uiScale;
            var startTransform = new ScaleTransform(startScale, startScale);

            var startMousePosition = e.GetPosition(MainContent);

            ScaleTransform? st = MainContent.LayoutTransform as ScaleTransform;

            if (st == null)
            {
                st = new ScaleTransform();
                MainContent.LayoutTransform = st;
            }

            if (e.Delta > 0)
            {
                newScale *= 1.25;

                if (newScale > 64)
                {
                    newScale = 64;
                }
            }
            else
            {
                newScale /= 1.25;

                if (newScale < 0.01)
                {
                    newScale = 0.01;
                }
            }

            _uiScale = newScale;

            var transform = new ScaleTransform
            {
                ScaleX = newScale,
                ScaleY = newScale
            };

            MainContent.LayoutTransform = transform;

            var startPoint = startTransform.Transform(new Point(startMousePosition.X, startMousePosition.Y));
            //System.Diagnostics.Debug.WriteLine($"{startPoint.X}, {startPoint.Y}");

            var endPoint = transform.Transform(new Point(startMousePosition.X, startMousePosition.Y));
            //System.Diagnostics.Debug.WriteLine($"{endPoint.X}, {endPoint.Y}");

            // Find the amount the mouse pixel has shifted due to change in scale.
            var shift = endPoint - startPoint;

            // Adjust current scrollviewer offset to maintain mouse pixel.
            MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset + shift.Y);
            MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset + shift.X);

            this.UpdateLayout();

            e.Handled = true;
        }
    }
}
