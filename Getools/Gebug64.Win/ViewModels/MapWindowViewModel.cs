﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
using Getools.Lib.Game.Asset.Stan;
using Getools.Lib.Game.Engine;
using Getools.Lib.Game.EnumModel;
using Getools.Lib.Game.Enums;
using Getools.Palantir;
using Getools.Palantir.Render;
using Getools.Utility.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAPICodePack.Dialogs;
using SvgLib;

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

            AddBgLayer(rawStage, adjustx, adjusty);
            AddStanLayer(rawStage, adjustx, adjusty);

            AddDoorLayer(rawStage, adjustx, adjusty, levelScale);

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
            layer.Stroke = Brushes.Blue;
            layer.StrokeThickness = 12;
            layer.Fill = Brushes.Transparent;

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
            layer.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#9e87a3")!;
            layer.StrokeThickness = 4;
            layer.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#fdf5ff")!;

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
                };

                layer.Entities.Add(mo);
            }

            Layers.Add(layer);
        }

        private void AddDoorLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            var collection = rawStage.SetupPolygonsCollection[PropDef.Door];

            layer = new MapLayerViewModel();
            layer.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#8d968e")!;
            layer.StrokeThickness = 2;
            layer.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#e1ffdb")!;

            foreach (var setupObj in collection)
            {
                mo = GetPropDefaultModelBbox_door(setupObj, adjustx, adjusty, levelScale);

                layer.Entities.Add(mo);
            }

            Layers.Add(layer);
        }

        private MapObject GetPropDefaultModelBbox_door(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_door(pp, levelScale);

            var mor = new MapObjectRect(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X;
            mor.UiHeight = stagePosition.ModelSize.Z;

            return mor;
        }
    }
}
