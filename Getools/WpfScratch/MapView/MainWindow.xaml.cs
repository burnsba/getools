using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var vm = new Vm();
            var roomLayer = new VmLayer();
            var doorLayer = new VmLayer();

            var p1 = new MapObjectPoly()
            {
                UiX = 75,
                UiY = 66,
            };
            p1.Points = new PointCollection(new Point[] { new Point(75, 66), new Point(127, 118), new Point(169, 67), new Point(172, 175), new Point(75, 153), new Point(75, 66) });
            p1.Points = Utility.AbsoluteToRelative(p1.Points);

            roomLayer.Entities.Add(p1);

            var p2 = new MapObjectPoly()
            {
                UiX = 222,
                UiY = 113,
            };
            p2.Points = new PointCollection(new Point[] { new Point(222, 113), new Point(336, 93), new Point(290, 200), new Point(222, 113) });
            p2.Points = Utility.AbsoluteToRelative(p2.Points);

            roomLayer.Entities.Add(p2);

            var d1 = new MapObjectRect()
            {
                UiX = 170 - (23 / 2),
                UiY = 132 - (45 / 2),
                Width = 23,
                Height = 45,
            };


            doorLayer.Entities.Add(d1);

            var d2 = new MapObjectRect()
            {
                UiX = 311 - (23 / 2),
                UiY = 152 - (45 / 2),
                Width = 23,
                Height = 45,
                RotationDegrees = 30,
            };

            doorLayer.Entities.Add(d2);

            vm.Layers.Add(roomLayer);
            vm.Layers.Add(doorLayer);

            DataContext = vm;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePos = Mouse.GetPosition(MainWindowZ);
            MousePos.Text = $"{mousePos.X}, {mousePos.Y}";
        }
    }

    public static class Utility
    {
        public static PointCollection AbsoluteToRelative(PointCollection points)
        {
            Point first = points[0];
            var result = new PointCollection();

            foreach (var p in points)
            {
                result.Add(new Point(p.X - first.X, p.Y - first.Y));
            }

            return result;
        }
    }

    public abstract class MapObject
    {
        public double UiX { get; set; }
        public double UiY { get; set; }
    }

    public class MapObjectPoly : MapObject
    {
        public PointCollection Points { get; set; }
    }

    public class MapObjectRect : MapObject
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double RotationDegrees { get; set; }
    }

    public class VmLayer
    {
        public ObservableCollection<MapObject> Entities { get; set; } = new ObservableCollection<MapObject>();
    }

    public class Vm
    {
        public ObservableCollection<VmLayer> Layers { get; set; } = new ObservableCollection<VmLayer>();
    }
}