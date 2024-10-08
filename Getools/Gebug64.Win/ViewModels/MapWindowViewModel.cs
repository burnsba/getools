﻿using System;
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
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// Viewmodel for main map control and UI state.
    /// This is for drawing a map in 2d (top down projection).
    /// </summary>
    public partial class MapWindowViewModel : ViewModelBase
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

        private bool _autoLoadLevel;
        private bool _followBond;

        /// <summary>
        /// Flag to disable saving app settings. Used during startup.
        /// </summary>
        private bool _ignoreAppSettingsChange = false;

        /// <summary>
        /// Current app settings.
        /// </summary>
        protected AppConfigViewModel _appConfig;

        public delegate void NotifyBondMoveHandler(object sender, NotifyBondMoveEventArgs e);

        public event NotifyBondMoveHandler BondMoveEvent;

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

            _autoLoadLevel = _appConfig.Map.AutoLoadLevel;
            _followBond = _appConfig.Map.FollowBond;

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
        /// Whether stage should chnage automatically, or user should manually change the stage.
        /// </summary>
        public bool AutoLoadLevel
        {
            get => _autoLoadLevel;

            set
            {
                if (_autoLoadLevel == value)
                {
                    return;
                }

                _autoLoadLevel = value;

                _appConfig.Map.AutoLoadLevel = value;

                SaveAppSettings();
            }
        }

        /// <summary>
        /// Whether the map should automatically scroll to Bond's position.
        /// </summary>
        public bool FollowBond
        {
            get => _followBond;

            set
            {
                if (_followBond == value)
                {
                    return;
                }

                _followBond = value;

                _appConfig.Map.FollowBond = value;

                SaveAppSettings();
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

        /// <summary>
        /// After the z-slice slider timer has finally elapsed, recalculate visible items.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Parameters.</param>
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

        /// <summary>
        /// Entry point for when user adjusts the z-slice slider in the user interface.
        /// This starts a timer which will recompute visible layers once it elapses.
        /// If the user adjusts the slider before the timer elapses then the timer
        /// is restarted.
        /// </summary>
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
            if (msg.Category == GebugMessageCategory.Stage)
            {
                if (msg.Command == (int)GebugCmdStage.NotifyLevelSelected)
                {
                    // Setting SelectedStage will automatically load the stage.
                    // Only load the stage if the user has configured it to.
                    if (_autoLoadLevel)
                    {
                        var levelSelectedMsg = (GebugStageNotifyLevelSelected)msg;
                        var xlevelId = LevelIdX.ToLevelIdXSafe((int)levelSelectedMsg.LevelId);
                        if (xlevelId.IsSinglePlayerLevel)
                        {
                            _dispatcher.BeginInvoke(() =>
                            {
                                SelectedStage = xlevelId;
                            });
                        }
                    }
                }
                else if (msg.Command == (int)GebugCmdStage.NotifyLevelLoaded)
                {
                    var levelLoadedMsg = (GebugStageNotifyLevelLoaded)msg;
                    var xlevelId = LevelIdX.ToLevelIdXSafe((int)levelLoadedMsg.LevelId);

                    // If quit to title, don't clear last data.
                    if (xlevelId.IsSinglePlayerLevel)
                    {
                        RemoveRuntimeItems();
                    }
                }
            }

            // The following messages require the map to already be loaded.
            if (!_isMapLoaded)
            {
                return;
            }

            // If a map is loaded and Bond's position is received, update the local Bond mapobject.
            if (msg.Category == GebugMessageCategory.Bond)
            {
                if (!object.ReferenceEquals(null, Bond)
                    && msg.Command == (int)GebugCmdBond.SendPosition)
                {
                    var posMessage = (GebugBondSendPositionMessage)msg;
                    if (!object.ReferenceEquals(null, posMessage))
                    {
                        var bondimg = (MapObjectResourceImage)Bond;
                        bondimg.ScaledOrigin.Y = posMessage.PosY;
                        bondimg.SetPositionLessHalf(posMessage.PosX + _adjustx, posMessage.PosZ + _adjusty, posMessage.VVTheta);

                        BondMoveEvent?.Invoke(this, new NotifyBondMoveEventArgs()
                        {
                            Position = new Point(posMessage.PosX + _adjustx, posMessage.PosZ + _adjusty),
                        });
                    }
                }
            }
            else if (msg.Category == GebugMessageCategory.Chr)
            {
                if (msg.Command == (int)GebugCmdChr.SendAllGuardInfo)
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
                else if (msg.Command == (int)GebugCmdChr.NotifyChrSpawn)
                {
                    var notifyMesg = (GebugChrNotifyChrSpawn)msg;
                    var positionData = notifyMesg.ParsePositionData();
                    foreach (var pdata in positionData)
                    {
                        _logger.Log(LogLevel.Information, $"Chr spawn at {pdata}");
                    }
                }
            }
            else if (msg.Category == GebugMessageCategory.Objects)
            {
                if (msg.Command == (int)GebugCmdObjects.NotifyExplosionCreate)
                {
                    var notifyMesg = (GebugObjectsNotifyExplosionCreate)msg;
                    var positionData = notifyMesg.ParsePositionData();
                    foreach (var pdata in positionData)
                    {
                        _logger.Log(LogLevel.Information, $"Explosion create at {pdata}");
                    }
                }
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
