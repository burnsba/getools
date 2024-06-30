using Getools.Lib.Converters;
using Getools.Lib.Game.Asset.Bg;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.Stan;
using GzipSharpLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfScratch.ViewModels;

namespace WpfScratch
{
    public partial class MainWindow : Window
    {
        private MapViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            StageSetupFile? setup = null;
            StandFile? stan = null;
            BgFile? bg = null;

            setup = SetupConverters.ReadFromBinFile(@"C:\Users\benja\code\getools\Getools\asset\setup\UsetupdamZ.bin");
            stan = StanConverters.ReadFromBinFile(@"C:\Users\benja\code\getools\Getools\asset\stan\Tbg_dam_all_p_stanZ.bin", "ignore");
            bg = BgConverters.ReadFromBinFile(@"C:\Users\benja\code\getools\Getools\asset\bg\bg_dam_all_p.bin");

            var stage = new Getools.Palantir.Stage()
            {
                Bg = bg,
                Setup = setup,
                Stan = stan,
                LevelScale = 0.23363999,
            };

            var mim = new Getools.Palantir.MapImageMaker(stage);

            var rawStage = mim.GetFullRawStage();

            // Find offset to zero axis.
            var adjustx = 0 - rawStage.ScaledMin.X;
            var adjusty = 0 - rawStage.ScaledMin.Z;

            var drawWidth = rawStage.ScaledMax.X - rawStage.ScaledMin.X;
            var drawHeight = rawStage.ScaledMax.Z - rawStage.ScaledMin.Z;

            // The resulting output should cover the range of the level.
            // This will be the "view box". It might have to scale to accomodate
            // the desired output width.
            var outputVeiwboxWidth = Math.Abs(rawStage.ScaledMax.X - rawStage.ScaledMin.X);
            var outputViewboxHeight = Math.Abs(rawStage.ScaledMax.Z - rawStage.ScaledMin.Z);

            ////

            _vm = new MapViewModel();

            MapLayerViewModel layer;
            MapObject mo;

            // bg
            {
                layer = new MapLayerViewModel();
                layer.Stroke = Brushes.Blue;
                layer.StrokeThickness = 12;
                layer.Fill = Brushes.Transparent;

                foreach (var poly in rawStage.RoomPolygons)
                {
                    var pc = new PointCollection();
                    foreach (var p in poly.Points!)
                    {
                        pc.Add(new Point(p.X + adjustx, p.Y + adjusty));
                    }

                    mo = new MapObjectPoly(poly.ScaledMin.X + adjustx, poly.ScaledMin.Z + adjusty)
                    {
                        Points = pc,
                        UiWidth = Math.Abs(poly.ScaledMax.X - poly.ScaledMin.X),
                        UiHeight = Math.Abs(poly.ScaledMax.Z - poly.ScaledMin.Z),
                    };

                    layer.Entities.Add(mo);
                }

                _vm.Layers.Add(layer);
            }

            // stan
            {
                layer = new MapLayerViewModel();
                layer.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#9e87a3")!;
                layer.StrokeThickness = 4;
                layer.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#fdf5ff")!;

                foreach (var poly in rawStage.TilePolygons)
                {
                    var pc = new PointCollection();
                    foreach (var p in poly.Points!)
                    {
                        pc.Add(new Point(p.X + adjustx, p.Y + adjusty));
                    }

                    mo = new MapObjectPoly(poly.ScaledMin.X + adjustx, poly.ScaledMin.Z + adjusty)
                    {
                        Points = pc,
                        UiWidth = Math.Abs(poly.ScaledMax.X - poly.ScaledMin.X),
                        UiHeight = Math.Abs(poly.ScaledMax.Z - poly.ScaledMin.Z),
                    };

                    layer.Entities.Add(mo);
                }

                _vm.Layers.Add(layer);
            }

            DataContext = _vm;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var mop = (MapObjectPoly)_vm.TheLayer.Entities[0];

            //mop.SetUiX(mop.UiX + 10);
        }
    }
}
