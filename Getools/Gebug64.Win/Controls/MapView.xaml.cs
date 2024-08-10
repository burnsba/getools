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
using AutoMapper.Features;
using Gebug64.Win.Event;
using Gebug64.Win.ViewModels.Map;
using Getools.Lib.Game;

namespace Gebug64.Win.Controls
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

        private List<DependencyObject> _mouseHitResultList = new List<DependencyObject>();

        private double _uiScale = 1.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapView"/> class.
        /// </summary>
        public MapView()
        {
            InitializeComponent();

            _content = this;
            _scrollViewer = MainScrollViewer;
        }

        /// <summary>
        /// Delegate to describe the method to handle notifications when the mouse is over game objects.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Args.</param>
        public delegate void NotifyMouseOverGameObjectHandler(object sender, NotifyMouseOverGameObjectEventArgs e);

        /// <summary>
        /// Delegate to describe handler for <see cref="NotifyMouseMoveGamePosition"/>.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Args.</param>
        public delegate void NotifyMouseMoveGamePositionHandler(object sender, NotifyMouseMoveTranslatedPositionEventArgs e);

        /// <summary>
        /// Event for notifications when mouse is over game objects.
        /// </summary>
        public event NotifyMouseOverGameObjectHandler? NotifyMouseOverGameObject;

        /// <summary>
        /// Event for notifying the current mouse position in scaled but translated game position.
        /// This is translated by the min value of the level such that
        /// the smallest possible position is (0,0).
        /// </summary>
        public event NotifyMouseMoveGamePositionHandler? NotifyMouseMoveGamePosition;

        /// <summary>
        /// When right click context menu is opened, notify what game objects are below the mouse.
        /// </summary>
        public event NotifyMouseOverGameObjectHandler? NotifyContextMenuGameObject;

        /// <summary>
        /// When right click context menu is opened, notify the current mouse position in
        /// scaled but translated game position.
        /// This is translated by the min value of the level such that
        /// the smallest possible position is (0,0).
        /// </summary>
        public event NotifyMouseMoveGamePositionHandler? NotifyContextMenuGamePosition;

        private void ScrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!MainScrollViewer.IsMouseOver)
            {
                return;
            }

            // The mouse events are bound on the overall usercontrol. However, the pan/zoom area
            // consumes the scrollviewer edges including the scrollbars. When a user clicks on the
            // scrollbars, instead of scrolling this begins the pan logic.
            // Check if the mouse event is contained within a scrollbar, and if so, ignore it.
            var a = Wpf.Utility.FindParentOfType<ScrollBar>((DependencyObject)e.OriginalSource);
            var b = Wpf.Utility.FindParentByRef(a, MainScrollViewer);
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

        private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
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

        private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

            // disable mouse events if the context menu is open
            if (MainScrollViewer.ContextMenu.IsOpen)
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

                if (newScale < 0.001)
                {
                    newScale = 0.001;
                }
            }

            _uiScale = newScale;

            var transform = new ScaleTransform
            {
                ScaleX = newScale,
                ScaleY = newScale,
            };

            MainContent.LayoutTransform = transform;

            var startPoint = startTransform.Transform(new Point(startMousePosition.X, startMousePosition.Y));

            var endPoint = transform.Transform(new Point(startMousePosition.X, startMousePosition.Y));

            // Find the amount the mouse pixel has shifted due to change in scale.
            var shift = endPoint - startPoint;

            // Adjust current scrollviewer offset to maintain mouse pixel.
            MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset + shift.Y);
            MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset + shift.X);

            this.UpdateLayout();

            e.Handled = true;
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            // disable mouse events if the context menu is open
            if (MainScrollViewer.ContextMenu.IsOpen)
            {
                return;
            }

            Point gamePosition = new(
                e.GetPosition(MainContent).X,
                e.GetPosition(MainContent).Y);

            NotifyMouseMoveGamePosition?.Invoke(this, new NotifyMouseMoveTranslatedPositionEventArgs()
            {
                Position = gamePosition,
            });

            // Retrieve the coordinate of the mouse position.
            Point pt = e.GetPosition((UIElement)sender);

            // Clear the contents of the list used for hit test results.
            _mouseHitResultList.Clear();

            // Set up a callback to receive the hit test result enumeration.
            VisualTreeHelper.HitTest(
                MainScrollViewer,
                new HitTestFilterCallback(MyHitTestFilter),
                new HitTestResultCallback(MyHitTestResult),
                new PointHitTestParameters(pt));

            // Perform actions on the hit test results list.
            if (_mouseHitResultList.Count > 0)
            {
                // MyHitTestFilter filters the objects, so these casts should always succeed.
                var gameObjects = _mouseHitResultList.Select(x => ((MapObject)((FrameworkElement)x).DataContext).DataSource!).ToList();

                NotifyMouseOverGameObject?.Invoke(this, new NotifyMouseOverGameObjectEventArgs()
                {
                    MouseOverObjects = gameObjects,
                });
            }
        }

        private HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            _mouseHitResultList.Add(result.VisualHit);

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }

        private HitTestFilterBehavior MyHitTestFilter(DependencyObject o)
        {
            // Test for the object value you want to filter.
            FrameworkElement? fe = o as FrameworkElement;
            if (fe != null)
            {
                // If a layer is disabled, no need to continue.
                if (fe is System.Windows.Controls.ItemsControl ic)
                {
                    if (ic.Visibility != Visibility.Visible)
                    {
                        return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                    }
                }

                // Only concerned about mapobject items.
                if (fe is System.Windows.Shapes.Shape || fe is System.Windows.Controls.Image)
                {
                    // maybe
                }
                else
                {
                    return HitTestFilterBehavior.ContinueSkipSelf;
                }

                if (fe.DataContext != null)
                {
                    MapObject? mapObject = fe.DataContext as MapObject;
                    {
                        if (mapObject != null && mapObject.DataSource != null && mapObject.IsVisible)
                        {
                            // Visual object is part of hit test results enumeration.
                            return HitTestFilterBehavior.Continue;
                        }
                    }
                }
            }

            return HitTestFilterBehavior.ContinueSkipSelf;
        }

        private void ContextMenu_ToolTipOpening(object sender, RoutedEventArgs e)
        {
            var gamePosition = Mouse.GetPosition(MainContent);

            NotifyContextMenuGamePosition?.Invoke(this, new NotifyMouseMoveTranslatedPositionEventArgs()
            {
                Position = gamePosition,
            });

            // Retrieve the coordinate of the mouse position.
            Point pt = Mouse.GetPosition(this);

            // Clear the contents of the list used for hit test results.
            _mouseHitResultList.Clear();

            // Set up a callback to receive the hit test result enumeration.
            VisualTreeHelper.HitTest(
                MainScrollViewer,
                new HitTestFilterCallback(MyHitTestFilter),
                new HitTestResultCallback(MyHitTestResult),
                new PointHitTestParameters(pt));

            // Perform actions on the hit test results list.
            if (_mouseHitResultList.Count > 0)
            {
                // MyHitTestFilter filters the objects, so these casts should always succeed.
                var gameObjects = _mouseHitResultList.Select(x => ((MapObject)((FrameworkElement)x).DataContext).DataSource!).ToList();

                NotifyContextMenuGameObject?.Invoke(this, new NotifyMouseOverGameObjectEventArgs()
                {
                    MouseOverObjects = gameObjects,
                });
            }
        }
    }
}
