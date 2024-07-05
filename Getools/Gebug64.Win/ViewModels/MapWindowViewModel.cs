using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Gebug64.Unfloader.Manage;
using Gebug64.Win.Controls;
using Gebug64.Win.Mvvm;
using Gebug64.Win.ViewModels.Config;
using Gebug64.Win.ViewModels.Map;
using Gebug64.Win.Windows.Mdi;
using Gebug64.Win.Wpf;
using Getools.Lib.Converters;
using Getools.Lib.Extensions;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Bg;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.SetupObject;
using Getools.Lib.Game.Asset.Stan;
using Getools.Lib.Game.Engine;
using Getools.Lib.Game.EnumModel;
using Getools.Lib.Game.Enums;
using Getools.Palantir;
using Getools.Palantir.Render;
using Getools.Utility.Logging;
using GzipSharpLib;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAPICodePack.Dialogs;
using SvgLib;
using static System.Windows.Forms.AxHost;

namespace Gebug64.Win.ViewModels
{
    public class MapWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IConnectionServiceProviderResolver _connectionServiceResolver;
        private readonly Dispatcher _dispatcher;

        private string? _setupBinFolder;
        private string? _stanBinFolder;
        private string? _bgBinFolder;

        private double _mapScaledWidth = 0;
        private double _mapScaledHeight = 0;

        private LevelIdX _selectedStage = LevelIdX.DefaultUnkown;

        private object _lock = new object();
        private bool _isLoading = false;

        /// <summary>
        /// Flag to disable saving app settings. Used during startup.
        /// </summary>
        private bool _ignoreAppSettingsChange = false;

        /// <summary>
        /// Current app settings.
        /// </summary>
        protected AppConfigViewModel _appConfig;

        /// <summary>
        /// Folder containing setup files.
        /// </summary>
        public string SetupBinFolder
        {
            get => _setupBinFolder;
            set
            {
                if (_setupBinFolder == value)
                {
                    return;
                }

                _setupBinFolder = value;
                OnPropertyChanged(nameof(SetupBinFolder));

                _appConfig.Map.SetupBinFolder = value;

                SaveAppSettings();
            }
        }

        /// <summary>
        /// Folder containing stan files.
        /// </summary>
        public string StanBinFolder
        {
            get => _stanBinFolder;
            set
            {
                if (_stanBinFolder == value)
                {
                    return;
                }

                _stanBinFolder = value;
                OnPropertyChanged(nameof(StanBinFolder));

                _appConfig.Map.StanBinFolder = value;

                SaveAppSettings();
            }
        }

        /// <summary>
        /// Folder containing stan files.
        /// </summary>
        public string BgBinFolder
        {
            get => _bgBinFolder;
            set
            {
                if (_bgBinFolder == value)
                {
                    return;
                }

                _bgBinFolder = value;
                OnPropertyChanged(nameof(BgBinFolder));

                _appConfig.Map.BgBinFolder = value;

                SaveAppSettings();
            }
        }

        /// <summary>
        /// Currently selected stage.
        /// </summary>
        public LevelIdX SelectedStage
        {
            get => _selectedStage;

            set
            {
                if (_selectedStage == value)
                {
                    return;
                }

                lock (_lock)
                {
                    if (_isLoading)
                    {
                        return;
                    }
                }

                _selectedStage = value;
                OnPropertyChanged(nameof(SelectedStage));

                LoadStageEntry(_selectedStage);
            }
        }

        /// <summary>
        /// List of available stages.
        /// </summary>
        public ObservableCollection<LevelIdX> AvailableStages { get; set; } = new ObservableCollection<LevelIdX>();

        public ObservableCollection<MapLayerViewModel> Layers { get; set; } = new ObservableCollection<MapLayerViewModel>();

        /// <summary>
        /// Command to set the <see cref="SetupBinFolder"/>.
        /// </summary>
        public ICommand SetSetupBinFolderCommand { get; set; }

        /// <summary>
        /// Command to set the <see cref="StanBinFolder"/>.
        /// </summary>
        public ICommand SetStanBinFolderCommand { get; set; }

        /// <summary>
        /// Command to set the <see cref="BgBinFolder"/>.
        /// </summary>
        public ICommand SetBgBinFolderCommand { get; set; }

        public double MapScaledWidth
        {
            get => _mapScaledWidth;
            set
            {
                if (_mapScaledWidth == value)
                {
                    return;
                }

                _mapScaledWidth = value;
                OnPropertyChanged(nameof(MapScaledWidth));
            }
        }

        public double MapScaledHeight
        {
            get => _mapScaledHeight;
            set
            {
                if (_mapScaledHeight == value)
                {
                    return;
                }

                _mapScaledHeight = value;
                OnPropertyChanged(nameof(MapScaledHeight));
            }
        }

        public MapWindowViewModel(ILogger logger, IConnectionServiceProviderResolver deviceManagerResolver, AppConfigViewModel appConfig)
        {
            _ignoreAppSettingsChange = true;

            _logger = logger;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _connectionServiceResolver = deviceManagerResolver;
            _appConfig = appConfig;

            SetSetupBinFolderCommand = new CommandHandler(
                () => Workspace.Instance.SetDirectoryCommandHandler(
                    this,
                    nameof(SetupBinFolder),
                    () => System.IO.Path.GetDirectoryName(System.AppContext.BaseDirectory)!),
                () => true);

            SetStanBinFolderCommand = new CommandHandler(
                () => Workspace.Instance.SetDirectoryCommandHandler(
                    this,
                    nameof(StanBinFolder),
                    () => System.IO.Path.GetDirectoryName(System.AppContext.BaseDirectory)!),
                () => true);

            SetBgBinFolderCommand = new CommandHandler(
                () => Workspace.Instance.SetDirectoryCommandHandler(
                    this,
                    nameof(BgBinFolder),
                    () => System.IO.Path.GetDirectoryName(System.AppContext.BaseDirectory)!),
                () => true);

            _setupBinFolder = _appConfig.Map.SetupBinFolder;
            _stanBinFolder = _appConfig.Map.StanBinFolder;
            _bgBinFolder = _appConfig.Map.BgBinFolder;

            LevelIdX.SinglePlayerStages.ForEach(x => AvailableStages.Add(x));

            _ignoreAppSettingsChange = false;
        }

        /// <summary>
        /// Pass through to <see cref="Workspace.Instance.SaveAppSettings"/>.
        /// </summary>
        protected void SaveAppSettings()
        {
            if (_ignoreAppSettingsChange)
            {
                return;
            }

            Workspace.Instance.SaveAppSettings();

            _appConfig.ClearIsDirty();
        }

        private void LoadStageEntry(LevelIdX stageid)
        {
            lock (_lock)
            {
                if (_isLoading)
                {
                    return;
                }

                _isLoading = true;
            }

            // Fighting with the dispatcher and dependency source object creation on the
            // right thread is not really making this any faster, so just going to block
            // the main thread.
            LoadStage(stageid);

            lock (_lock)
            {
                _isLoading = false;
            }
        }

        private void LoadStage(LevelIdX stageid)
        {
            if (!LevelIdX.SinglePlayerStages.Contains(stageid))
            {
                _logger.LogError($"{nameof(LoadStage)}: id {stageid} not supported");
                return;
            }

            if (string.IsNullOrEmpty(_setupBinFolder))
            {
                _logger.LogError($"{nameof(LoadStage)}: setup bin folder not set");
                return;
            }

            if (string.IsNullOrEmpty(_stanBinFolder))
            {
                _logger.LogError($"{nameof(LoadStage)}: stan bin folder not set");
                return;
            }

            if (string.IsNullOrEmpty(_bgBinFolder))
            {
                _logger.LogError($"{nameof(LoadStage)}: bg bin folder not set");
                return;
            }

            StageSetupFile? setup = null;
            StandFile? stan = null;
            BgFile? bg = null;

            var setupFileMap = Getools.Lib.Game.Asset.Setup.SourceMap.GetFileMap(Getools.Lib.Game.Enums.Version.Ntsc, stageid.Id);
            var stanFileMap = Getools.Lib.Game.Asset.Stan.SourceMap.GetFileMap(stageid.Id);
            var bgFileMap = Getools.Lib.Game.Asset.Bg.SourceMap.GetFileMap(Getools.Lib.Game.Enums.Version.Ntsc, stageid.Id);

            var setupFilePath = Path.Combine(_setupBinFolder, setupFileMap.Dir, setupFileMap.Filename + ".bin");
            var stanFilePath = Path.Combine(_stanBinFolder, stanFileMap.Dir, stanFileMap.Filename + ".bin");
            var bgFilePath = Path.Combine(_bgBinFolder, bgFileMap.Dir, bgFileMap.Filename + ".bin");

            if (!File.Exists(setupFilePath))
            {
                _logger.LogError($"{nameof(LoadStage)}: setup not found: {setupFilePath}");
                return;
            }

            if (!File.Exists(stanFilePath))
            {
                _logger.LogError($"{nameof(LoadStage)}: stan not found: {stanFilePath}");
                return;
            }

            if (!File.Exists(bgFilePath))
            {
                _logger.LogError($"{nameof(LoadStage)}: bg not found: {bgFilePath}");
                return;
            }

            setup = SetupConverters.ReadFromBinFile(setupFilePath);
            stan = StanConverters.ReadFromBinFile(stanFilePath, "ignore");
            bg = BgConverters.ReadFromBinFile(bgFilePath);

            double levelScale = Getools.Lib.Game.Asset.StageScale.GetStageScale(stageid.Id);

            var stage = new Getools.Palantir.Stage()
            {
                Bg = bg,
                Setup = setup,
                Stan = stan,
                LevelScale = levelScale,
            };

            var mim = new Getools.Palantir.MapImageMaker(stage);

            var rawStage = mim.GetFullRawStage();

            // Find offset to zero axis.
            var adjustx = 0 - rawStage.ScaledMin.X;
            var adjusty = 0 - rawStage.ScaledMin.Z;

            // The resulting output should cover the range of the level.
            // This will be the "view box". It might have to scale to accomodate
            // the desired output width.
            var outputVeiwboxWidth = Math.Abs(rawStage.ScaledMax.X - rawStage.ScaledMin.X);
            var outputViewboxHeight = Math.Abs(rawStage.ScaledMax.Z - rawStage.ScaledMin.Z);

            // the map area will be a 3x3 grid, move the main content into the center cell.
            adjustx += outputVeiwboxWidth;
            adjusty += outputViewboxHeight;

            ClearMap();

            AddStanLayer(rawStage, adjustx, adjusty);

            // room boundaries need to be above the tiles.
            AddBgLayer(rawStage, adjustx, adjusty);

            AddPadLayer(rawStage, adjustx, adjusty, levelScale);

            // draw path waypoints on top of pads
            AddPathWaypointLayer(rawStage, adjustx, adjusty, levelScale);

            // draw patrol paths on top of pads and waypoints
            AddPatrolPathLayer(rawStage, adjustx, adjusty, levelScale);

            AddSetupLayer(rawStage, PropDef.AmmoBox, adjustx, adjusty, levelScale);

            // safe should be under door z layer.
            AddSetupLayer(rawStage, PropDef.Safe, adjustx, adjusty, levelScale);
            AddSetupLayer(rawStage, PropDef.Door, adjustx, adjusty, levelScale);
            AddSetupLayer(rawStage, PropDef.Alarm, adjustx, adjusty, levelScale);
            AddSetupLayer(rawStage, PropDef.Cctv, adjustx, adjusty, levelScale);
            AddSetupLayer(rawStage, PropDef.Drone, adjustx, adjusty, levelScale);
            AddSetupLayer(rawStage, PropDef.Aircraft, adjustx, adjusty, levelScale);
            AddSetupLayer(rawStage, PropDef.Tank, adjustx, adjusty, levelScale);
            AddSetupLayer(rawStage, PropDef.StandardProp, adjustx, adjusty, levelScale);
            AddSetupLayer(rawStage, PropDef.SingleMonitor, adjustx, adjusty, levelScale);
            AddSetupLayer(rawStage, PropDef.Collectable, adjustx, adjusty, levelScale);
            AddSetupLayer(rawStage, PropDef.Armour, adjustx, adjusty, levelScale);

            // keys should be on top of tables (StandardProp)
            AddSetupLayer(rawStage, PropDef.Key, adjustx, adjusty, levelScale);

            // guards should be one of the highest z layers
            AddSetupLayer(rawStage, PropDef.Guard, adjustx, adjusty, levelScale);

            // Intro star should be above other layers
            AddIntroLayer(rawStage, adjustx, adjusty, levelScale);

            // Create a 3x3 grid to render the map, to be able to pan around/past the edge.
            MapScaledWidth = outputVeiwboxWidth * 3;
            MapScaledHeight = outputViewboxHeight * 3;

            _logger.LogInformation($"Map viewer: done loading {stageid.Name}");
        }

        private void ClearMap()
        {
            while (Layers.Any())
            {
                Layers.RemoveAt(0);
            }
        }

        private void AddBgLayer(ProcessedStageData rawStage, double adjustx, double adjusty)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel();

            foreach (var poly in rawStage.RoomPolygons)
            {
                var pc = new PointCollection();

                var first = poly.Points!.First();

                foreach (var p in poly.Points!)
                {
                    pc.Add(new Point(p.X + adjustx, p.Y + adjusty));
                }

                pc = pc.AbsoluteToRelative();

                mo = new MapObjectPoly(first.X + adjustx, first.Y + adjusty)
                {
                    Points = pc,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 12,
                    Fill = Brushes.Transparent,
                };

                layer.Entities.Add(mo);
            }

            Layers.Add(layer);
        }

        private void AddStanLayer(ProcessedStageData rawStage, double adjustx, double adjusty)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel();

            foreach (var poly in rawStage.TilePolygons)
            {
                var pc = new PointCollection();

                var first = poly.Points!.First();

                foreach (var p in poly.Points!)
                {
                    pc.Add(new Point(p.X + adjustx, p.Y + adjusty));
                }

                pc = pc.AbsoluteToRelative();

                mo = new MapObjectPoly(first.X + adjustx, first.Y + adjusty)
                {
                    Points = pc,
                    Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#9e87a3")!,
                    StrokeThickness = 4,
                    Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#fdf5ff")!,
                };

                layer.Entities.Add(mo);
            }

            Layers.Add(layer);
        }

        private void AddIntroLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel();

            foreach (var pp in rawStage.IntroPolygons)
            {
                mo = GetPropDefaultBbox_intro(pp, adjustx, adjusty, levelScale);

                layer.Entities.Add(mo);
            }

            Layers.Add(layer);
        }

        private void AddSetupLayer(ProcessedStageData rawStage, PropDef setupLayerKey, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel();

            if (!rawStage.SetupPolygonsCollection.ContainsKey(setupLayerKey))
            {
                Layers.Add(layer);
                return;
            }

            var collection = rawStage.SetupPolygonsCollection[setupLayerKey];

            foreach (var pp in collection)
            {
                if (object.ReferenceEquals(null, pp.SetupObject))
                {
                    throw new NullReferenceException($"{nameof(pp.SetupObject)}");
                }

                // guard does not implement SetupObjectGenericBase
                SetupObjectGenericBase? baseObject = pp.SetupObject as SetupObjectGenericBase;

                switch (pp.SetupObject.Type)
                {
                    case PropDef.Door:
                    mo = GetPropDefaultModelBbox_door(pp, adjustx, adjusty, levelScale);
                    break;

                    case PropDef.Guard:
                    mo = GetPropDefaultModelBbox_chr(pp, adjustx, adjusty, levelScale);
                    break;

                    case PropDef.AmmoBox:
                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#274d23", 4, "#66ed58");
                    break;

                    case PropDef.Alarm:
                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale / 3, "#cccccc", 4, "#ff0000");
                    break;

                    case PropDef.Armour:
                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#0c1c63", 4, "#0000ff");
                    break;

                    case PropDef.Key:
                    mo = GetPropDefaultBbox_key(pp, adjustx, adjusty, levelScale);
                    break;

                    case PropDef.Cctv:
                    mo = GetPropDefaultBbox_cctv(pp, adjustx, adjusty, levelScale);
                    break;

                    case PropDef.Aircraft:
                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#808018", 4, "#dbdb60");
                    break;

                    case PropDef.Collectable: // fallthrough
                    case PropDef.Drone:
                    case PropDef.Safe:
                    case PropDef.SingleMonitor:
                    case PropDef.StandardProp:
                    {
                        if (object.ReferenceEquals(null, baseObject))
                        {
                            throw new NullReferenceException();
                        }

                        switch ((PropId)baseObject.ObjectId)
                        {
                            // hat
                            case PropId.PROP_HATFURRY:
                            case PropId.PROP_HATFURRYBROWN:
                            case PropId.PROP_HATFURRYBLACK:
                            case PropId.PROP_HATTBIRD:
                            case PropId.PROP_HATTBIRDBROWN:
                            case PropId.PROP_HATHELMET:
                            case PropId.PROP_HATHELMETGREY:
                            case PropId.PROP_HATMOON:
                            case PropId.PROP_HATBERET:
                            case PropId.PROP_HATBERETBLUE:
                            case PropId.PROP_HATBERETRED:
                            case PropId.PROP_HATPEAKED:
                                // skip
                                continue;

                            case PropId.PROP_PADLOCK:
                                mo = GetPropDefaultBbox_lock(pp, adjustx, adjusty, levelScale);
                                break;

                            case PropId.PROP_GUN_RUNWAY1:
                            case PropId.PROP_ROOFGUN:
                            case PropId.PROP_GROUNDGUN:
                                mo = GetPropDefaultBbox_heavygun(pp, adjustx, adjusty, levelScale);
                                break;

                            case PropId.PROP_CHRGOLDENEYEKEY:
                                mo = GetPropDefaultBbox_key(pp, adjustx, adjusty, levelScale);
                                break;

                            case PropId.PROP_MAINFRAME1:
                            case PropId.PROP_MAINFRAME2:
                            case PropId.PROP_DOOR_MF:
                                mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#666666", 4, "#94dff2");
                                break;

                            case PropId.PROP_AMMO_CRATE1:
                            case PropId.PROP_AMMO_CRATE2:
                            case PropId.PROP_AMMO_CRATE3:
                            case PropId.PROP_AMMO_CRATE4:
                            case PropId.PROP_AMMO_CRATE5:
                                mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#274d23", 4, "#66ed58");
                                break;

                            case PropId.PROP_CHRCIRCUITBOARD:
                                mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#009900", 4, "#00ff00");
                                break;

                            case PropId.PROP_CHRVIDEOTAPE:
                            case PropId.PROP_CHRDOSSIERRED:
                            case PropId.PROP_CHRSTAFFLIST:
                            case PropId.PROP_CHRCLIPBOARD:
                            case PropId.PROP_DISK_DRIVE1:
                            case PropId.PROP_CHRDATTAPE:
                                mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#ff0000", 4, "#ff0000");
                                break;

                            case PropId.PROP_TIGER:
                            case PropId.PROP_MILCOPTER:
                            case PropId.PROP_HELICOPTER:
                                mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#808018", 4, "#dbdb60");
                                break;

                            default:
                                mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#916b2a", 4, "#ffdfa8");
                                break;
                        }

                        break;
                    }

                    case PropDef.Tank:
                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#255c25", 4, "#00ff00");
                    break;

                    default:
                    throw new NotImplementedException();
                }

                layer.Entities.Add(mo);
            }

            Layers.Add(layer);
        }

        private void AddPadLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel();

            var collection = rawStage.PresetPolygons;

            foreach (var pp in collection)
            {
                mo = GetPadBbox(pp, adjustx, adjusty, levelScale);
                layer.Entities.Add(mo);
            }

            Layers.Add(layer);
        }

        private void AddPathWaypointLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel();

            var collection = rawStage.PathWaypointLines;

            foreach (var renderLine in collection)
            {
                var p1 = renderLine.P1.Scale(1.0 / levelScale);
                var p2 = renderLine.P2.Scale(1.0 / levelScale);

                double uix = p1.X + adjustx;
                double uiy = p1.Z + adjusty;

                double uix2 = p2.X - p1.X;
                double uiy2 = p2.Z - p1.Z;

                mo = new MapObjectLine(uix, uiy)
                {
                    P1 = new Point(0, 0),
                    P2 = new Point(uix2, uiy2),
                    StrokeThickness = 12,
                    Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#ff80ff")!,
                };

                layer.Entities.Add(mo);
            }

            Layers.Add(layer);
        }

        private void AddPatrolPathLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel();

            var collection = rawStage.PatrolPathLines;

            foreach (var pathset in collection)
            {
                var pc = new PointCollection();

                var scaledPoints = pathset.Points.Select(x => x.Scale(1.0 / levelScale).To2DXZ());
                var first = scaledPoints.First();

                foreach (var p in scaledPoints)
                {
                    pc.Add(new Point(p.X + adjustx, p.Y + adjusty));
                }

                pc = pc.AbsoluteToRelative();

                mo = new MapObjectPolyLine(first.X + adjustx, first.Y + adjusty)
                {
                    Points = pc,
                    Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#3fb049")!,
                    StrokeThickness = 18,
                };

                layer.Entities.Add(mo);
            }

            Layers.Add(layer);
        }

        private MapObject GetPropDefaultModelBbox_prop(PropPointPosition pp, double adjustx, double adjusty, double levelScale, string stroke, double strokeWidth, string fill)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_prop(pp, levelScale);

            var mor = new MapObjectRect(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom(stroke)!;
            mor.StrokeThickness = strokeWidth;
            mor.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(fill)!;

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X;
            mor.UiHeight = stagePosition.ModelSize.Z;

            return mor;
        }

        private MapObject GetPropDefaultModelBbox_door(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_door(pp, levelScale);

            var mor = new MapObjectRect(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#8d968e")!;
            mor.StrokeThickness = 2;
            mor.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#e1ffdb")!;

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X;
            mor.UiHeight = stagePosition.ModelSize.Z;

            return mor;
        }

        private MapObject GetPropDefaultModelBbox_chr(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_chr(pp, levelScale);

            var mor = new MapObjectResourceImage(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/chr.png";

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X;
            mor.UiHeight = stagePosition.ModelSize.Z;

            return mor;
        }

        private MapObject GetPropDefaultBbox_key(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            double w = 34;
            double h = 112;
            double hw = w / 2;
            double hh = h / 2;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var mor = new MapObjectResourceImage(
                pos.X - hw + adjustx,
                pos.Z - hh + adjusty);

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/key.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            return mor;
        }

        private MapObject GetPropDefaultBbox_cctv(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            double w = 90;
            double h = 90;
            double hw = w / 2;
            double hh = h / 2;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var mor = new MapObjectResourceImage(
                pos.X - hw + adjustx,
                pos.Z - hh + adjusty);

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/cctv.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            return mor;
        }

        private MapObject GetPropDefaultBbox_lock(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            double w = 34;
            double h = 112;
            double hw = w / 2;
            double hh = h / 2;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var mor = new MapObjectResourceImage(
                pos.X - hw + adjustx,
                pos.Z - hh + adjusty);

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/lock.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            return mor;
        }

        private MapObject GetPropDefaultBbox_heavygun(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            double w = 140;
            double h = 140;
            double hw = w / 2;
            double hh = h / 2;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var mor = new MapObjectResourceImage(
                pos.X - hw + adjustx,
                pos.Z - hh + adjusty);

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/heavy_gun.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            return mor;
        }

        private MapObject GetPropDefaultBbox_intro(PointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            double w = 90;
            double h = 90;
            double hw = w / 2;
            double hh = h / 2;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            var mor = new MapObjectResourceImage(
                pos.X - hw + adjustx,
                pos.Z - hh + adjusty);

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/star.png";

            mor.UiWidth = w;
            mor.UiHeight = h;

            return mor;
        }

        private MapObject GetPadBbox(PointPosition rp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPadBbox(rp, levelScale);

            var mor = new MapObjectRect(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#9c009e")!;
            mor.StrokeThickness = 4;
            mor.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#ff00ff")!;

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X;
            mor.UiHeight = stagePosition.ModelSize.Z;

            return mor;
        }
    }
}
