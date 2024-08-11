using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Dto;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Win.Controls;
using Gebug64.Win.Enum;
using Gebug64.Win.Event;
using Gebug64.Win.Mvvm;
using Gebug64.Win.ViewModels.Config;
using Gebug64.Win.ViewModels.Game;
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
using Getools.Lib.Kaitai.Gen;
using Getools.Palantir;
using Getools.Palantir.Render;
using Getools.Utility.Logging;
using GzipSharpLib;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Microsoft.WindowsAPICodePack.Dialogs;
using SvgLib;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// Viewmodel for main map control and UI state.
    /// This is for drawing a map in 2d (top down projection).
    /// </summary>
    public class MapWindowViewModel : ViewModelBase
    {
        private const int _mapResliceDelay = 200; // ms

        /// <summary>
        /// Lot of magic fine tuning constants to get the guards to look approximately correct.
        /// Right now the SVG map looks ok, so changing the underlying model size code will
        /// break that. Therefore, apply an adjustment to the image here.
        /// </summary>
        private const double _wpfGuardModelScaleFactor = 1.5;

        private readonly ILogger _logger;
        private readonly IConnectionServiceProviderResolver _connectionServiceResolver;
        private readonly Dispatcher _dispatcher;
        private readonly MessageBus<IGebugMessage>? _appGebugMessageBus;
        private readonly Guid _mapGebugMessageSubscription;

        private string? _setupBinFolder;
        private string? _stanBinFolder;
        private string? _bgBinFolder;
        private string? _statusBarTextMouseOver;
        private string? _statusBarTextPosition = "0, 0";

        private double _mapScaledWidth = 0;
        private double _mapScaledHeight = 0;
        private double _mapMinVertical = 0;
        private double _mapMaxVertical = 0;
        private double _mapSelectedMinVertical = 0;
        private double _mapSelectedMaxVertical = 0;
        private double _adjustx = 0;
        private double _adjusty = 0;
        private double _levelScale = 1.0;
        private double _currentMouseGamePositionX = 0.0;
        private double _currentMouseGamePositionY = 0.0;
        private double _currentContextMenuMouseGamePositionX = 0.0;
        private double _currentContextMenuMouseGamePositionY = 0.0;

        private LevelIdX _selectedStage = LevelIdX.DefaultUnkown;

        private object _lock = new object();
        private bool _isLoading = false;
        private bool _isMapLoaded = false;

        private object _guardLayerLock = new object();
        private object _timerLock = new object();
        private bool _mapZSliceTimerActive = false;
        private System.Timers.Timer _mapZSliceTimer;

        /// <summary>
        /// Keep a reference to the guard layer.
        /// </summary>
        private MapLayerViewModel? _guardLayer = null;

        private GameObject? _selectedMapObject = null;

        private List<GameObject> _mouseOverItems = new List<GameObject>();
        private List<GameObject> _contextMenuItems = new List<GameObject>();

        /// <summary>
        /// This variable holds the last intro coordinates read. Used to set Bond's initial position on stage load.
        /// </summary>
        private Coord3dd _lastIntroPosition = Coord3dd.Zero.Clone();

        /// <summary>
        /// Flag to disable saving app settings. Used during startup.
        /// </summary>
        private bool _ignoreAppSettingsChange = false;

        /// <summary>
        /// Current app settings.
        /// </summary>
        protected AppConfigViewModel _appConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapWindowViewModel"/> class.
        /// </summary>
        /// <param name="logger">App logger.</param>
        /// <param name="deviceManagerResolver">Device resolver.</param>
        /// <param name="appConfig">App config.</param>
        /// <param name="appGebugMessageBus">Message bus to listen for <see cref="IGebugMessage"/> messages from console.</param>
        public MapWindowViewModel(
            ILogger logger,
            IConnectionServiceProviderResolver deviceManagerResolver,
            AppConfigViewModel appConfig,
            MessageBus<IGebugMessage> appGebugMessageBus)
        {
            _ignoreAppSettingsChange = true;

            _mapZSliceTimer = new System.Timers.Timer();
            _mapZSliceTimer.Interval = _mapResliceDelay; // ms
            _mapZSliceTimer.Enabled = false;
            _mapZSliceTimer.AutoReset = false;
            _mapZSliceTimer.Elapsed += MapZSliceTimerElapsed;

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

            ToggleLayerVisibilityCommand = new CommandHandler(
                x => ToggleLayerVisibility((UiMapLayer)x!),
                () => true);

            TeleportToCommand = new CommandHandler(
                TeleportToCommandHandler,
                () => CanTeleportToMouse);

            _setupBinFolder = _appConfig.Map.SetupBinFolder;
            _stanBinFolder = _appConfig.Map.StanBinFolder;
            _bgBinFolder = _appConfig.Map.BgBinFolder;

            LevelIdX.SinglePlayerStages.ForEach(x => AvailableStages.Add(x));

            _ignoreAppSettingsChange = false;

            _appGebugMessageBus = appGebugMessageBus;
            _mapGebugMessageSubscription = _appGebugMessageBus!.Subscribe(MessageBusMapGebugCallback);
        }

        /// <summary>
        /// Folder containing setup files.
        /// </summary>
        public string? SetupBinFolder
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
        public string? StanBinFolder
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
        public string? BgBinFolder
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

        /// <summary>
        /// Collection of layers in the map.
        /// </summary>
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

        /// <summary>
        /// Click handler for checkbox layer visibility.
        /// </summary>
        public ICommand ToggleLayerVisibilityCommand { get; set; }

        /// <summary>
        /// Width of the map for the level in scaled in-game units.
        /// </summary>
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

        /// <summary>
        /// (screen) Height of the map for the level in scaled in-game units.
        /// </summary>
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

        /// <summary>
        /// Flag to indicate whether or not there is a stage loaded.
        /// </summary>
        public bool IsMapLoaded
        {
            get => _isMapLoaded;
            set
            {
                if (_isMapLoaded != value)
                {
                    _isMapLoaded = value;
                    OnPropertyChanged(nameof(IsMapLoaded));
                }
            }
        }

        /// <summary>
        /// Map minimum vertical point in scaled units.
        /// </summary>
        public double MapMinVertical
        {
            get => _mapMinVertical;
            set
            {
                if (_mapMinVertical != value)
                {
                    _mapMinVertical = value;
                    OnPropertyChanged(nameof(MapMinVertical));
                    OnPropertyChanged(nameof(MapMinVerticalText));

                    if (_mapSelectedMinVertical < _mapMinVertical)
                    {
                        MapSelectedMinVertical = _mapMinVertical;
                    }

                    if (_mapSelectedMaxVertical < _mapMinVertical)
                    {
                        MapSelectedMaxVertical = _mapMinVertical;
                    }

                    StartExtendMapZSliceTimer();
                }
            }
        }

        /// <summary>
        /// <see cref="MapMinVertical"/> as string.
        /// </summary>
        public string MapMinVerticalText => _mapMinVertical.ToString(Gebug64.Win.Config.Constants.DefaultDecimalFormat);

        /// <summary>
        /// Map maximum vertical point in scaled units.
        /// </summary>
        public double MapMaxVertical
        {
            get => _mapMaxVertical;
            set
            {
                if (_mapMaxVertical != value)
                {
                    _mapMaxVertical = value;
                    OnPropertyChanged(nameof(MapMaxVertical));
                    OnPropertyChanged(nameof(MapMaxVerticalText));

                    if (_mapSelectedMaxVertical > _mapMaxVertical)
                    {
                        MapSelectedMaxVertical = _mapMaxVertical;
                    }

                    if (_mapSelectedMinVertical > _mapMaxVertical)
                    {
                        MapSelectedMinVertical = _mapMaxVertical;
                    }

                    StartExtendMapZSliceTimer();
                }
            }
        }

        /// <summary>
        /// <see cref="MapMaxVertical"/> as string.
        /// </summary>
        public string MapMaxVerticalText => _mapMaxVertical.ToString(Gebug64.Win.Config.Constants.DefaultDecimalFormat);

        /// <summary>
        /// User selected lower boundary vertical point in scaled units.
        /// </summary>
        public double MapSelectedMinVertical
        {
            get => _mapSelectedMinVertical;
            set
            {
                if (_mapSelectedMinVertical != value && value >= _mapMinVertical && value <= _mapMaxVertical && value <= _mapSelectedMaxVertical)
                {
                    _mapSelectedMinVertical = value;
                    OnPropertyChanged(nameof(MapSelectedMinVertical));
                    OnPropertyChanged(nameof(MapSelectedMinVerticalText));

                    StartExtendMapZSliceTimer();
                }
            }
        }

        /// <summary>
        /// <see cref="MapSelectedMinVertical"/> as string.
        /// </summary>
        public string MapSelectedMinVerticalText
        {
            get => _mapSelectedMinVertical.ToString(Gebug64.Win.Config.Constants.DefaultDecimalFormat);
            set
            {
                double d;
                if (double.TryParse(value, out d))
                {
                    if (_mapSelectedMinVertical != d && d >= _mapMinVertical && d <= _mapMaxVertical && d <= _mapSelectedMaxVertical)
                    {
                        _mapSelectedMinVertical = d;
                        OnPropertyChanged(nameof(MapSelectedMinVerticalText));
                        OnPropertyChanged(nameof(MapSelectedMinVertical));

                        StartExtendMapZSliceTimer();
                    }
                }
            }
        }

        /// <summary>
        /// User selected upper boundary vertical point in scaled units.
        /// </summary>
        public double MapSelectedMaxVertical
        {
            get => _mapSelectedMaxVertical;
            set
            {
                if (_mapSelectedMaxVertical != value && value >= _mapMinVertical && value <= _mapMaxVertical && value >= _mapSelectedMinVertical)
                {
                    _mapSelectedMaxVertical = value;
                    OnPropertyChanged(nameof(MapSelectedMaxVertical));
                    OnPropertyChanged(nameof(MapSelectedMaxVerticalText));

                    StartExtendMapZSliceTimer();
                }
            }
        }

        /// <summary>
        /// <see cref="MapSelectedMaxVertical"/> as string.
        /// </summary>
        public string MapSelectedMaxVerticalText
        {
            get => _mapSelectedMaxVertical.ToString(Gebug64.Win.Config.Constants.DefaultDecimalFormat);
            set
            {
                double d;
                if (double.TryParse(value, out d))
                {
                    if (_mapSelectedMaxVertical != d && d >= _mapMinVertical && d <= _mapMaxVertical && d >= _mapSelectedMinVertical)
                    {
                        _mapSelectedMaxVertical = d;
                        OnPropertyChanged(nameof(MapSelectedMaxVerticalText));
                        OnPropertyChanged(nameof(MapSelectedMaxVertical));

                        StartExtendMapZSliceTimer();
                    }
                }
            }
        }

        /// <summary>
        /// Pointer for Bond's current position data, in the current map.
        /// </summary>
        public MapObject? Bond { get; private set; }

        /// <summary>
        /// Status bar display text for mouse over items.
        /// </summary>
        public string? StatusBarTextMouseOver
        {
            get => _statusBarTextMouseOver;

            set
            {
                if (_statusBarTextMouseOver != value)
                {
                    _statusBarTextMouseOver = value;
                    OnPropertyChanged(nameof(StatusBarTextMouseOver));
                }
            }
        }

        /// <summary>
        /// Status bar display text for mouse position.
        /// </summary>
        public string? StatusBarTextPosition
        {
            get => _statusBarTextPosition;

            set
            {
                if (_statusBarTextPosition != value)
                {
                    _statusBarTextPosition = value;
                    OnPropertyChanged(nameof(StatusBarTextPosition));
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently selected <see cref="GameObject"/>/<see cref="MapObject"/>
        /// in the main map control.
        /// </summary>
        public GameObject? SelectedMapObject
        {
            get => _selectedMapObject;

            set
            {
                if (_selectedMapObject != value)
                {
                    _selectedMapObject = value;
                    OnPropertyChanged(nameof(SelectedMapObject));
                }
            }
        }

        /// <summary>
        /// Right click context menu, "Select..." child item list.
        /// </summary>
        public ObservableCollection<MenuItemViewModel> ContextMenuSelectList { get; set; } = new ObservableCollection<MenuItemViewModel>();

        /// <summary>
        /// Flag to indicate whether context menu is currently opened or not.
        /// One way binding from user interface to the view model.
        /// </summary>
        public bool ContextMenuIsOpen { get; set; }

        /// <summary>
        /// Command to invoke from the right click context menu.
        /// Teleport Bond to location.
        /// </summary>
        public ICommand TeleportToCommand { get; set; }

        /// <summary>
        /// Whether <see cref="TeleportToCommand"/> can execute.
        /// </summary>
        public bool CanTeleportToMouse
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown;
            }
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

        /// <summary>
        /// External method to be called when mouse over object list is updated.
        /// </summary>
        /// <param name="e">Current mouseover items.</param>
        public void NotifyMouseOverObjectsChanged(NotifyMouseOverGameObjectEventArgs e)
        {
            List<GameObject> items = e.MouseOverObjects;

            _mouseOverItems = items;

            UpdateMouseOverStatusText();
            UpdateContextMenuSelect();
        }

        /// <summary>
        /// External method to be called when context menu is opened.
        /// </summary>
        /// <param name="e">Current mouseover items.</param>
        public void NotifyContextMenuObjectsChanged(NotifyMouseOverGameObjectEventArgs e)
        {
            List<GameObject> items = e.MouseOverObjects;

            _contextMenuItems = items;
        }

        /// <summary>
        /// Callback to handle when the mouse is moved in the map control.
        /// </summary>
        /// <param name="e">Mouse position in scaled game units. The map control has been shifted
        /// by the min map values such that the smallest possible point is (0,0).</param>
        public void NotifyMouseMove(NotifyMouseMoveTranslatedPositionEventArgs e)
        {
            var p = e.Position;

            _currentMouseGamePositionX = p.X - _adjustx;
            _currentMouseGamePositionY = p.Y - _adjusty;

            StatusBarTextPosition = $"{_currentMouseGamePositionX:0.0000}, {_currentMouseGamePositionY:0.0000}";
        }

        /// <summary>
        /// External method to be called when context menu is opened.
        /// </summary>
        /// <param name="e">Mouse position in scaled game units. The map control has been shifted
        /// by the min map values such that the smallest possible point is (0,0).</param>
        public void NotifyContextMenuPosition(NotifyMouseMoveTranslatedPositionEventArgs e)
        {
            var p = e.Position;

            _currentContextMenuMouseGamePositionX = p.X - _adjustx;
            _currentContextMenuMouseGamePositionY = p.Y - _adjusty;
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
            IsMapLoaded = false;

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
            if (string.IsNullOrEmpty(setupFileMap.Filename))
            {
                _logger.LogError($"{nameof(LoadStage)}: Could not resolve setup relative path");
                return;
            }

            var stanFileMap = Getools.Lib.Game.Asset.Stan.SourceMap.GetFileMap(stageid.Id);
            if (string.IsNullOrEmpty(stanFileMap.Filename))
            {
                _logger.LogError($"{nameof(LoadStage)}: Could not resolve stan relative path");
                return;
            }

            var bgFileMap = Getools.Lib.Game.Asset.Bg.SourceMap.GetFileMap(Getools.Lib.Game.Enums.Version.Ntsc, stageid.Id);
            if (string.IsNullOrEmpty(bgFileMap.Filename))
            {
                _logger.LogError($"{nameof(LoadStage)}: Could not resolve bg relative path");
                return;
            }

            string setupFilePath;
            string stanFilePath;
            string bgFilePath;

            if (string.IsNullOrEmpty(setupFileMap.Dir))
            {
                setupFilePath = Path.Combine(_setupBinFolder, setupFileMap.Filename + ".bin");
            }
            else
            {
                setupFilePath = Path.Combine(_setupBinFolder, setupFileMap.Dir, setupFileMap.Filename + ".bin");
            }

            if (string.IsNullOrEmpty(stanFileMap.Dir))
            {
                stanFilePath = Path.Combine(_stanBinFolder, stanFileMap.Filename + ".bin");
            }
            else
            {
                stanFilePath = Path.Combine(_stanBinFolder, stanFileMap.Dir, stanFileMap.Filename + ".bin");
            }

            if (string.IsNullOrEmpty(bgFileMap.Dir))
            {
                bgFilePath = Path.Combine(_bgBinFolder, bgFileMap.Filename + ".bin");
            }
            else
            {
                bgFilePath = Path.Combine(_bgBinFolder, bgFileMap.Dir, bgFileMap.Filename + ".bin");
            }

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

            _levelScale = Getools.Lib.Game.Asset.StageScale.GetStageScale(stageid.Id);

            var stage = new Getools.Palantir.Stage()
            {
                Bg = bg,
                Setup = setup,
                Stan = stan,
                LevelScale = _levelScale,
            };

            var mim = new Getools.Palantir.MapImageMaker(stage);

            var rawStage = mim.GetFullRawStage();

            // Find offset to zero axis.
            _adjustx = 0 - rawStage.ScaledMin.X;
            _adjusty = 0 - rawStage.ScaledMin.Z;

            // The resulting output should cover the range of the level.
            // This will be the "view box". It might have to scale to accomodate
            // the desired output width.
            var outputVeiwboxWidth = Math.Abs(rawStage.ScaledMax.X - rawStage.ScaledMin.X);
            var outputViewboxHeight = Math.Abs(rawStage.ScaledMax.Z - rawStage.ScaledMin.Z);

            ClearMap();

            AddStanLayer(rawStage, _adjustx, _adjusty);

            // room boundaries need to be above the tiles.
            AddBgLayer(rawStage, _adjustx, _adjusty);

            AddPadLayer(rawStage, _adjustx, _adjusty, _levelScale);

            // draw path waypoints on top of pads
            AddPathWaypointLayer(rawStage, _adjustx, _adjusty, _levelScale);

            // draw patrol paths on top of pads and waypoints
            AddPatrolPathLayer(rawStage, _adjustx, _adjusty, _levelScale);

            AddSetupLayer(rawStage, PropDef.AmmoBox, _adjustx, _adjusty, _levelScale);

            // safe should be under door z layer.
            AddSetupLayer(rawStage, PropDef.Safe, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Door, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Alarm, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Cctv, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Drone, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Aircraft, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Tank, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.StandardProp, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.SingleMonitor, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Collectable, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Armour, _adjustx, _adjusty, _levelScale);

            // keys should be on top of tables (StandardProp)
            AddSetupLayer(rawStage, PropDef.Key, _adjustx, _adjusty, _levelScale);

            // guards should be one of the highest z layers
            AddSetupLayer(rawStage, PropDef.Guard, _adjustx, _adjusty, _levelScale);
            _guardLayer = Layers.First(x => x.LayerId == UiMapLayer.SetupGuard);

            // Intro star should be above other layers
            AddIntroLayer(rawStage, _adjustx, _adjusty, _levelScale);

            // Create a reference for Bond and place in a new layer, on top of everything else.
            AddBondLayer(rawStage, _adjustx, _adjusty, _levelScale);

            MapScaledWidth = outputVeiwboxWidth;
            MapScaledHeight = outputViewboxHeight;

            _logger.LogInformation($"Map viewer: done loading {stageid.Name}");

            IsMapLoaded = true;
            MapMinVertical = rawStage.ScaledMin.Y;
            MapMaxVertical = rawStage.ScaledMax.Y;
            MapSelectedMinVertical = rawStage.ScaledMin.Y;
            MapSelectedMaxVertical = rawStage.ScaledMax.Y;
        }

        private void ClearMap()
        {
            while (Layers.Any())
            {
                Layers.RemoveAt(0);
            }
        }

        private void ToggleLayerVisibility(UiMapLayer layerId)
        {
            var layer = Layers.FirstOrDefault(x => x.LayerId == layerId);
            if (object.ReferenceEquals(null, layer))
            {
                return;
            }

            layer.IsVisible = !layer.IsVisible;

            _appConfig.Map.SetMapLayerVisibility(layerId, layer.IsVisible);
        }

        private void AddBgLayer(ProcessedStageData rawStage, double adjustx, double adjusty)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.Bg)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.Bg],
            };

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

                mo.ScaledMin = poly.ScaledMin.Clone();
                mo.ScaledMax = poly.ScaledMax.Clone();

                mo.DataSource = new Game.Bg(_logger, _connectionServiceResolver)
                {
                    LayerIndexId = poly.Room,
                };

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            Layers.Add(layer);
        }

        private void AddStanLayer(ProcessedStageData rawStage, double adjustx, double adjusty)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.Stan)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.Stan],
            };

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

                mo.ScaledMin = poly.ScaledMin.Clone();
                mo.ScaledMax = poly.ScaledMax.Clone();

                mo.DataSource = new Game.Stan(_logger, _connectionServiceResolver)
                {
                    LayerIndexId = poly.OrderIndex,
                    LayerInstanceId = poly.StanNameId,
                };

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            Layers.Add(layer);
        }

        private void AddIntroLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.SetupIntro)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupIntro],
            };

            foreach (var pp in rawStage.IntroPolygons)
            {
                mo = GetPropDefaultBbox_intro(pp, adjustx, adjusty, levelScale);

                mo.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

                mo.DataSource = new Game.GameObject(_logger, _connectionServiceResolver);

                _lastIntroPosition = mo.ScaledOrigin.Clone();

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            Layers.Add(layer);
        }

        private void AddBondLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;

            layer = new MapLayerViewModel(Enum.UiMapLayer.Bond)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.Bond],
            };

            Bond = GetPropDefaultBond(new PropPointPosition(), adjustx, adjusty, levelScale);

            // set Bond's position to (last) intro
            Bond.ScaledOrigin.X = _lastIntroPosition.X;
            Bond.ScaledOrigin.Y = _lastIntroPosition.Y;
            Bond.ScaledOrigin.Z = _lastIntroPosition.Z;

            layer.DispatchAddEntity(_dispatcher, Bond);

            Layers.Add(layer);
        }

        private void AddSetupLayer(ProcessedStageData rawStage, PropDef setupLayerKey, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            var uilayerid = PropDefToUiLayer(setupLayerKey);

            layer = new MapLayerViewModel(uilayerid)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[uilayerid],
            };

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

                // msgGuard does not implement SetupObjectGenericBase
                SetupObjectGenericBase? baseObject = pp.SetupObject as SetupObjectGenericBase;

                switch (pp.SetupObject.Type)
                {
                    case PropDef.Door:
                    mo = GetPropDefaultModelBbox_door(pp, adjustx, adjusty, levelScale);
                    break;

                    case PropDef.Guard:
                    mo = GetPropDefaultModelBbox_chr(pp, adjustx, adjusty, levelScale);

                    // Set chr id from setup file
                    var guard = (SetupObjectGuard)pp.SetupObject;
                    mo.DataSource!.LayerInstanceId = (int)guard.ObjectId;

                    break;

                    case PropDef.AmmoBox:
                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#274d23", 4, "#66ed58");
                    break;

                    case PropDef.Alarm:
                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#cccccc", 1, "#ff0000");
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

                            case PropId.PROP_ALARM1:
                            case PropId.PROP_ALARM2:
                                mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#cccccc", 1, "#ff0000");
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

                mo.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

                mo.DataSource!.PropDefType = pp.SetupObject.Type;
                mo.DataSource!.LayerIndexId = pp.SetupObject.SetupSectionTypeIndex;

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            Layers.Add(layer);
        }

        private void AddPadLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.SetupPad)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupPad],
            };

            var collection = rawStage.PresetPolygons;

            foreach (var pp in collection)
            {
                mo = GetPadBbox(pp, adjustx, adjusty, levelScale);

                mo.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

                mo.DataSource = new Game.Pad(_logger, _connectionServiceResolver)
                {
                    LayerIndexId = pp.PadId,
                };

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            Layers.Add(layer);
        }

        private void AddPathWaypointLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.SetupPathWaypoint)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupPathWaypoint],
            };

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

                var scaledPos = renderLine.Bbox.Scale(1 / levelScale);

                mo.ScaledMin.X = scaledPos.MinX;
                mo.ScaledMin.Y = scaledPos.MinY;
                mo.ScaledMin.Z = scaledPos.MinZ;
                mo.ScaledMax.X = scaledPos.MaxX;
                mo.ScaledMax.Y = scaledPos.MaxY;
                mo.ScaledMax.Z = scaledPos.MaxZ;

                mo.DataSource = new Game.GameObject(_logger, _connectionServiceResolver);

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            Layers.Add(layer);
        }

        private void AddPatrolPathLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.SetupPatrolPath)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupPatrolPath],
            };

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

                var scaledPos = pathset.Bbox.Scale(1 / levelScale);

                mo.ScaledMin.X = scaledPos.MinX;
                mo.ScaledMin.Y = scaledPos.MinY;
                mo.ScaledMin.Z = scaledPos.MinZ;
                mo.ScaledMax.X = scaledPos.MaxX;
                mo.ScaledMax.Y = scaledPos.MaxY;
                mo.ScaledMax.Z = scaledPos.MaxZ;

                mo.DataSource = new Game.GameObject(_logger, _connectionServiceResolver);

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            Layers.Add(layer);
        }

        private MapObject GetPropDefaultModelBbox_prop(PropPointPosition pp, double adjustx, double adjusty, double levelScale, string stroke, double strokeWidth, string fill)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_prop(pp, levelScale);

            var mor = new MapObjectRect(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

            mor.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom(stroke)!;
            mor.StrokeThickness = strokeWidth;
            mor.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(fill)!;

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X;
            mor.UiHeight = stagePosition.ModelSize.Z;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPropDefaultModelBbox_door(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_door(pp, levelScale);

            var mor = new MapObjectRect(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

            mor.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#8d968e")!;
            mor.StrokeThickness = 2;
            mor.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#e1ffdb")!;

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X;
            mor.UiHeight = stagePosition.ModelSize.Z;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPropDefaultModelBbox_chr(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_chr(pp, levelScale);

            var mor = new MapObjectResourceImage(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/chr.png";

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X * _wpfGuardModelScaleFactor;
            mor.UiHeight = stagePosition.ModelSize.Z * _wpfGuardModelScaleFactor;

            mor.DataSource = new Game.Chr(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPropDefaultBond(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_chr(pp, levelScale);

            var mor = new MapObjectResourceImage(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/bond.png";

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X * _wpfGuardModelScaleFactor;
            mor.UiHeight = stagePosition.ModelSize.Z * _wpfGuardModelScaleFactor;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

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

            mor.ScaledOrigin = pos;

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/key.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

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

            mor.ScaledOrigin = pos;

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/cctv.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

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

            mor.ScaledOrigin = pos;

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/lock.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

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

            mor.ScaledOrigin = pos;

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/heavy_gun.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

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

            mor.ScaledOrigin = pos;

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/star.png";

            mor.UiWidth = w;
            mor.UiHeight = h;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPadBbox(PointPosition rp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPadBbox(rp, levelScale);

            var mor = new MapObjectRect(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.ScaledOrigin = rp.Origin.Clone().Scale(1 / levelScale);

            mor.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#9c009e")!;
            mor.StrokeThickness = 4;
            mor.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#ff00ff")!;

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X;
            mor.UiHeight = stagePosition.ModelSize.Z;

            return mor;
        }

        private Enum.UiMapLayer PropDefToUiLayer(PropDef pd)
        {
            switch (pd)
            {
                case PropDef.Alarm: return Enum.UiMapLayer.SetupAlarm;
                case PropDef.AmmoBox: return Enum.UiMapLayer.SetupAmmo;
                case PropDef.Aircraft: return Enum.UiMapLayer.SetupAircraft;
                case PropDef.Armour: return Enum.UiMapLayer.SetupBodyArmor;
                case PropDef.Guard: return Enum.UiMapLayer.SetupGuard;
                case PropDef.Cctv: return Enum.UiMapLayer.SetupCctv;
                case PropDef.Collectable: return Enum.UiMapLayer.SetupCollectable;
                case PropDef.Door: return Enum.UiMapLayer.SetupDoor;
                case PropDef.Drone: return Enum.UiMapLayer.SetupDrone;
                case PropDef.Key: return Enum.UiMapLayer.SetupKey;
                case PropDef.Safe: return Enum.UiMapLayer.SetupSafe;
                case PropDef.SingleMonitor: return Enum.UiMapLayer.SetupSingleMontior;
                case PropDef.StandardProp: return Enum.UiMapLayer.SetupStandardProp;
                case PropDef.Tank: return Enum.UiMapLayer.SetupTank;
            }

            throw new NotSupportedException();
        }

        private void MapZSliceTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            lock (_timerLock)
            {
                _mapZSliceTimerActive = false;
            }

            foreach (var layer in Layers)
            {
                foreach (var entity in layer.GetEntities())
                {
                    if (layer.ZSliceCompare == UiMapLayerZSliceCompare.Bbox)
                    {
                        if ((entity.ScaledMax.Y >= _mapSelectedMinVertical && entity.ScaledMax.Y <= _mapSelectedMaxVertical)
                            || (entity.ScaledMin.Y >= _mapSelectedMinVertical && entity.ScaledMin.Y <= _mapSelectedMaxVertical))
                        {
                            entity.IsVisible = true;
                        }
                        else
                        {
                            entity.IsVisible = false;
                        }
                    }
                    else if (layer.ZSliceCompare == UiMapLayerZSliceCompare.OriginPoint)
                    {
                        if (entity.ScaledOrigin.Y >= _mapSelectedMinVertical && entity.ScaledOrigin.Y <= _mapSelectedMaxVertical)
                        {
                            entity.IsVisible = true;
                        }
                        else
                        {
                            entity.IsVisible = false;
                        }
                    }
                }
            }
        }

        private void StartExtendMapZSliceTimer()
        {
            lock (_timerLock)
            {
                if (!_mapZSliceTimerActive)
                {
                    _mapZSliceTimerActive = true;
                    _mapZSliceTimer.Start();
                    return;
                }

                _mapZSliceTimer.Stop();
                _mapZSliceTimer.Start();
            }
        }

        /// <summary>
        /// Callback to monitor incoming messages from the console.
        /// </summary>
        /// <param name="msg">Message.</param>
        private void MessageBusMapGebugCallback(IGebugMessage msg)
        {
            // If a map is loaded and Bond's position is received, update the local Bond mapobject.
            if (_isMapLoaded
                && !object.ReferenceEquals(null, Bond)
                && msg.Category == GebugMessageCategory.Bond
                && msg.Command == (int)GebugCmdBond.SendPosition)
            {
                var posMessage = (GebugBondSendPositionMessage)msg;
                if (!object.ReferenceEquals(null, posMessage))
                {
                    var bondimg = (MapObjectResourceImage)Bond;
                    bondimg.ScaledOrigin.Y = posMessage.PosY;
                    bondimg.SetPositionLessHalf(posMessage.PosX + _adjustx, posMessage.PosZ + _adjusty, posMessage.VVTheta);
                }
            }

            if (_isMapLoaded
                && msg.Category == GebugMessageCategory.Chr
                && msg.Command == (int)GebugCmdChr.SendAllGuardInfo)
            {
                var guarddata = ((GebugChrSendAllGuardInfoMessage)msg).ParseGuardPositions();

                foreach (var msgGuard in guarddata)
                {
                    switch (msgGuard.ActionType)
                    {
                        case GuardActType.ActDead:
                            MessageCallbackGuardDead(msgGuard);
                            break;

                        default:
                            MessageCallbackGuardUpdatePosition(msgGuard);
                            break;
                    }
                }

                var mesgGuardIndexIds = guarddata.Select(x => (int)x.ChrSlotIndex).ToHashSet();

                var nonMatchingGuards = _guardLayer!.GetEntities()
                    .Where(x =>
                        //// "targetpos" items will have parent set, so filter those out
                        x.Parent == null
                        //// only want guard items
                        && x.DataSource != null
                        && x.DataSource is Chr
                        && !mesgGuardIndexIds.Contains(((Chr)x.DataSource).ChrSlotIndex))
                    .ToList();

                _guardLayer.DispatchRemoveRange(_dispatcher, nonMatchingGuards);
            }
        }

        private void MessageCallbackGuardDead(RmonGuardPosition msgGuard)
        {
            MapObjectResourceImage? mapGuard = null;

            mapGuard = FindMapObjectGuardInLayer(msgGuard);

            if (!object.ReferenceEquals(null, mapGuard))
            {
                // Don't clear children in this thread, the dispatcher thread hasn't processed the children list yet.
                _guardLayer!.DispatchRemoveEntityAndChildren(_dispatcher, mapGuard);
            }
        }

        private void MessageCallbackGuardUpdatePosition(RmonGuardPosition msgGuard)
        {
            MapObjectResourceImage? mapGuard = null;

            mapGuard = FindMapObjectGuardInLayer(msgGuard);

            if (!object.ReferenceEquals(null, mapGuard))
            {
                // If the existing datasource is set, copy that.
                if (mapGuard.DataSource != null && mapGuard.DataSource is Chr cc)
                {
                    ((Chr)mapGuard.DataSource).UpdateFrom(msgGuard);
                }
                else
                {
                    // Otherwise just pull from the message.
                    mapGuard.DataSource = new Chr(_logger, _connectionServiceResolver, msgGuard);
                }

                mapGuard.ScaledOrigin.Y = msgGuard.PropPos.Y;
                mapGuard.SetPositionLessHalf(msgGuard.PropPos.X + _adjustx, msgGuard.PropPos.Z + _adjusty, msgGuard.ModelRotationDegrees);

                AddOrSetGuardTargetPosMapObject(mapGuard, msgGuard);
            }
            else
            {
                var pp = new PropPointPosition();
                mapGuard = (MapObjectResourceImage)GetPropDefaultModelBbox_chr(pp, _adjustx, _adjusty, _levelScale);

                mapGuard.DataSource = new Chr(_logger, _connectionServiceResolver, msgGuard);

                mapGuard.IsVisible = true;
                mapGuard.ScaledOrigin.Y = msgGuard.PropPos.Y;
                mapGuard.SetPositionLessHalf(msgGuard.PropPos.X + _adjustx, msgGuard.PropPos.Z + _adjusty, msgGuard.ModelRotationDegrees);

                _guardLayer!.DispatchAddEntity(_dispatcher, mapGuard);
            }
        }

        private void AddOrSetGuardTargetPosMapObject(MapObjectResourceImage mapGuard, RmonGuardPosition msgGuard)
        {
            if (msgGuard.ActionType == GuardActType.ActGoPos
                || msgGuard.ActionType == GuardActType.ActPatrol
                || msgGuard.ActionType == GuardActType.ActRunPos)
            {
                bool addNew = false;

                MapObjectResourceImage? existingTargetMapObject = mapGuard.Children.FirstOrDefault(x => x is MapObjectResourceImage) as MapObjectResourceImage;

                if (object.ReferenceEquals(null, existingTargetMapObject))
                {
                    // does not copy data source.
                    existingTargetMapObject = mapGuard.Clone();
                    existingTargetMapObject.DataSource = new Chr(_logger, _connectionServiceResolver, (Chr)mapGuard.DataSource!);
                    existingTargetMapObject.ResourcePath = "/Gebug64.Win;component/Resource/Image/targetpos.png";
                    existingTargetMapObject.IsVisible = true;

                    addNew = true;
                }

                var sourcePos = msgGuard.TargetPos;
                existingTargetMapObject.ScaledOrigin.Y = sourcePos.Y;
                existingTargetMapObject.SetPositionLessHalf(sourcePos.X + _adjustx, sourcePos.Z + _adjusty, msgGuard.ModelRotationDegrees);

                if (addNew)
                {
                    existingTargetMapObject.Parent = mapGuard;
                    mapGuard.Children.Add(existingTargetMapObject);

                    _guardLayer!.DispatchAddEntity(_dispatcher, existingTargetMapObject);
                }
            }
        }

        private MapObjectResourceImage? FindMapObjectGuardInLayer(RmonGuardPosition msgGuard)
        {
            MapObjectResourceImage? result = null;

            if (result == null && msgGuard.ChrSlotIndex >= 0)
            {
                result = _guardLayer!.SafeFirstOrDefault(x =>
                    x.DataSource != null
                    && x.DataSource is Chr
                    && ((Chr)x.DataSource).ChrSlotIndex == msgGuard.ChrSlotIndex)
                as MapObjectResourceImage;
            }

            return result;
        }

        private void UpdateMouseOverStatusText()
        {
            var items = _mouseOverItems;

            List<int> bgs = new List<int>();
            List<int> stans = new List<int>();
            List<int> pads = new List<int>();
            List<int> chrs = new List<int>();

            List<string> sectionTexts = new List<string>();

            Dictionary<PropDef, List<int>> setupProps = new Dictionary<PropDef, List<int>>();

            foreach (var item in items)
            {
                if (item is Game.Prop p)
                {
                    if (p.PropDefType != null)
                    {
                        if (setupProps.ContainsKey(p.PropDefType.Value))
                        {
                            setupProps[p.PropDefType.Value].Add(item.PreferredId);
                        }
                        else
                        {
                            var listy = new List<int>();
                            listy.Add(item.PreferredId);
                            setupProps.Add(p.PropDefType.Value, listy);
                        }
                    }
                    else
                    {
                        // Bond
                        // guard target position
                        // ...
                    }
                }
                else if (item is Game.Chr c)
                {
                    chrs.Add(item.PreferredId);
                }
                else if (item is Game.Pad pad)
                {
                    pads.Add(item.PreferredId);
                }
                else if (item is Game.Stan stan)
                {
                    stans.Add(item.PreferredId);
                }
                else if (item is Game.Bg bg)
                {
                    bgs.Add(item.PreferredId);
                }
            }

            if (bgs.Any())
            {
                var idText = string.Join(", ", bgs);
                sectionTexts.Add($"bg: {idText}");
            }

            if (stans.Any())
            {
                var idText = string.Join(", ", stans.Select(x => "0x" + x.ToString("X6")));
                sectionTexts.Add($"stan: {idText}");
            }

            if (pads.Any())
            {
                var idText = string.Join(", ", pads);
                sectionTexts.Add($"pad: {idText}");
            }

            if (chrs.Any())
            {
                var idText = string.Join(", ", chrs);
                sectionTexts.Add($"chr: {idText}");
            }

            if (setupProps.Any())
            {
                foreach (var kvp in setupProps)
                {
                    string label = Getools.Lib.Formatters.EnumFormat.PropDefFriendlyName(kvp.Key);
                    var idText = string.Join(", ", kvp.Value);
                    sectionTexts.Add($"{label}: {idText}");
                }
            }

            var totalString = string.Join(", ", sectionTexts);
            StatusBarTextMouseOver = totalString;
        }

        private void SelectMapObject(GameObject? go)
        {
            SelectedMapObject = go;
        }

        private void UpdateContextMenuSelect()
        {
            if (ContextMenuIsOpen)
            {
                return;
            }

            var items = _mouseOverItems;

            ContextMenuSelectList.Clear();

            Action<object?> dddd = x => SelectMapObject((GameObject?)((MenuItemViewModel)x!).Value);

            MenuItemViewModel mivm;
            string menuLabel;

            foreach (var item in items)
            {
                if (item is Game.Prop p)
                {
                    if (p.PropDefType != null)
                    {
                        string label = Getools.Lib.Formatters.EnumFormat.PropDefFriendlyName(p.PropDefType.Value);
                        menuLabel = $"{label}: {item.PreferredId}";
                    }
                    else
                    {
                        // Bond
                        // guard target position
                        // ...
                        continue;
                    }
                }
                else if (item is Game.Chr c)
                {
                    menuLabel = $"chr: {item.PreferredId}";
                }
                else if (item is Game.Pad pad)
                {
                    menuLabel = $"pad: {item.PreferredId}";
                }
                else if (item is Game.Stan stan)
                {
                    menuLabel = $"stan: {item.PreferredDisplayId}";
                }
                else if (item is Game.Bg bg)
                {
                    menuLabel = $"bg: {item.PreferredId}";
                }
                else
                {
                    continue;
                }

                mivm = new MenuItemViewModel() { Header = menuLabel };
                mivm.Command = new CommandHandler(dddd, () => true);
                mivm.Value = item;

                ContextMenuSelectList.Add(mivm);
            }
        }

        private void TeleportToCommandHandler()
        {
            Game.Stan? stan = (Game.Stan?)_contextMenuItems.FirstOrDefault(x => x is Game.Stan);

            // stan.LayerInstanceId is set to Getools.Lib.Game.Asset.Stan.StandTile.InternalName .
            if (!object.ReferenceEquals(null, stan) && stan.LayerInstanceId >= 0)
            {
                ////_logger.Log(LogLevel.Information, $"Teleport to {_currentContextMenuMouseGamePositionX}, {_currentContextMenuMouseGamePositionY}, stan: {stan.LayerIndexId}");

                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceResolver.GetDeviceManager();

                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return;
                }

                var msg = new GebugBondTeleportToPositionMessage()
                {
                    PosX = (Single)_currentContextMenuMouseGamePositionX,
                    PosZ = (Single)_currentContextMenuMouseGamePositionY,
                    StanId = stan.LayerInstanceId,
                };

                _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

                connectionServiceProvider.SendMessage(msg);
            }
            else
            {
                _logger.Log(LogLevel.Information, $"TeleportTo: invalid stan");
            }
        }
    }
}
