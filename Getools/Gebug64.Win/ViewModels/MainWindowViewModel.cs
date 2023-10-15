using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Unfloader;
using Gebug64.Unfloader.Protocol.Unfloader.Message;
using Gebug64.Win.Config;
using Gebug64.Win.Enum;
using Gebug64.Win.Mvvm;
using Gebug64.Win.QueryTask;
using Gebug64.Win.Session;
using Gebug64.Win.Ui;
using Gebug64.Win.ViewModels.CategoryTabs;
using Gebug64.Win.ViewModels.Config;
using Getools.Lib.Game.Asset.SetupObject;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// Primary class of the application.
    /// Handles all "backend" logic routed from main ui window.
    /// </summary>
    public class MainWindowViewModel : WindowViewModelBase
    {
        /// <summary>
        /// Baseline memory size.
        /// </summary>
        private const int BaseMemSizeBytes = 0x00400000;

        /// <summary>
        /// Size in bytes if expansion pak is installed.
        /// </summary>
        private const int ExpansionPakMemSizeBytes = 0x00800000;

        /// <summary>
        /// Number of menu entries before the send rom list begins.
        /// </summary>
        private const int RecentSendRomPermanentCount = 3;

        /// <summary>
        /// If the device is connected, and no communication occurs for this many seconds,
        /// then try to send the ping command.
        /// </summary>
        private const int PingIntervalSec = 20;

        /// <summary>
        /// If no communiation occurs for this many seconds, assume the device is no
        /// longer connected.
        /// </summary>
        private const int TimeoutDisconnectSec = 70;

        /// <summary>
        /// Max number of "recent files" to show in "send ROM" dropdown.
        /// </summary>
        private const int MaxRecentlySentFiles = 10;

        private readonly object _availableSerialPortsLock = new object();
        private readonly object _availableFlashcartsLock = new object();
        private readonly object _logMessagesLock = new object();
        private readonly object _queryTasksLock = new object();
        private readonly object _recentlySentFilesLock = new object();

        private readonly ILogger _logger;
        private readonly IConnectionServiceProviderResolver _connectionServiceResolver;
        private readonly Dispatcher _dispatcher;

        /// <summary>
        /// Size in bytes of attached ROM memory.
        /// </summary>
        private int _memSize = 0;

        /// <summary>
        /// Flag to disable saving app settings. Used during startup.
        /// </summary>
        private bool _ignoreAppSettingsChange = false;

        /// <summary>
        /// Current known connected gebug ROM version.
        /// </summary>
        private Version? _romVersion = null;

        /// <summary>
        /// Flag to indicate the app is currently connected to a flashcart (any connection level).
        /// </summary>
        private bool _isConnected = false;

        /// <summary>
        /// Flag to indicate the app is attempting to connect to a flashcart.
        /// </summary>
        private bool _isConnecting = false;

        /// <summary>
        /// Flag to indicate the app is currently scanning the computer for available serial ports.
        /// </summary>
        private bool _isRefreshingSerialPorts = false;

        /// <summary>
        /// Flag to indicate the connection is currently in an error state.
        /// </summary>
        private bool _connectionError = false;

        /// <summary>
        /// Currently connected flashcart device type or null.
        /// </summary>
        private IFlashcart? _currentFlashCart;

        /// <summary>
        /// If a ROM was sent since the application started, this is the last used file path. Or null.
        /// </summary>
        private string? _selectedRomPath;

        /// <summary>
        /// When the app starts, the last used flashcart is deserialized from a string.
        /// This value indicates whether that is a valid flashcart or not.
        /// </summary>
        private bool _flashcartIsValid = false;

        /// <summary>
        /// Gets a value indicating whether the app is currently sending a ROM.
        /// </summary>
        private bool _currentlySendingRom = false;

        /// <summary>
        /// Startup flag to indicate the list of available flashcarts has been loaded.
        /// </summary>
        private bool _setAvailableFlashcarts = false;

        /// <summary>
        /// Device manager subscription for gebug level messages.
        /// This sends the message to the log output.
        /// </summary>
        private Guid _messageBusGebugLogSubscription = Guid.Empty;

        /// <summary>
        /// Device manager subscription for UNFLoader level messages.
        /// This sends the message to the log output.
        /// </summary>
        private Guid _messageBusUnfloaderLogSubscription = Guid.Empty;

        /// <summary>
        /// List of available serial ports.
        /// </summary>
        private ObservableCollection<string> _availableSerialPorts = new ObservableCollection<string>();

        /// <summary>
        /// List of available flashcarts.
        /// </summary>
        private ObservableCollection<IFlashcart> _availableFlashcarts = new ObservableCollection<IFlashcart>();

        /// <summary>
        /// List of items send to the logger.
        /// </summary>
        private ObservableCollection<string> _logMessages = new ObservableCollection<string>();

        /// <summary>
        /// The long running query tasks.
        /// </summary>
        private ObservableCollection<QueryTaskViewModel> _queryTasks = new ObservableCollection<QueryTaskViewModel>();

        /// <summary>
        /// Core connection manager.
        /// </summary>
        private IConnectionServiceProvider? _connectionServiceManager;

        /// <summary>
        /// Thread flag to indicate safe shutdown.
        /// </summary>
        private bool _shutdown = false;

        /// <summary>
        /// Main worker thread. Sends and receives messages between pc and console.
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// Tracks duration since the last time a message was sent from pc to console.
        /// </summary>
        private Stopwatch? _lastMessageSent = null;

        /// <summary>
        /// Current connection level.
        /// </summary>
        private ConnectionLevel _connectionLevel = ConnectionLevel.NotConnected;

        /// <summary>
        /// Cancellation token for active long running events (sending ROM).
        /// </summary>
        private CancellationTokenSource? _sendRomCancellation = null;

        /// <summary>
        /// Window menu group for the list of flashcarts, such that only one element in the group can be checked.
        /// </summary>
        private OnlyOneChecked<MenuItemViewModel, Guid> _menuDeviceGroup = new OnlyOneChecked<MenuItemViewModel, Guid>();

        /// <summary>
        /// Window menu group for the list of serial ports, such that only one element in the group can be checked.
        /// </summary>
        private OnlyOneChecked<MenuItemViewModel, Guid> _menuSerialPortGroup = new OnlyOneChecked<MenuItemViewModel, Guid>();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="deviceManagerResolver">Device manager.</param>
        /// <param name="appConfig">Main app config.</param>
        public MainWindowViewModel(ILogger logger, IConnectionServiceProviderResolver deviceManagerResolver, AppConfigViewModel appConfig)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _ignoreAppSettingsChange = true;

            _logger = logger;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _connectionServiceResolver = deviceManagerResolver;

            // Need to supply AppConfig early because some properties reference this.
            AppConfig = appConfig;

            // MainWindowViewModel is singleton, so don't need to worry about attaching multiple callbacks.
            ((Logger)_logger).AddCallback((level, msg) =>
            {
                _dispatcher.BeginInvoke(() => LogMessages.Add(msg));
            });

            RefreshAvailableSerialPortsCommand = new CommandHandler(RefreshAvailableSerialPortsCommandHandler, () => CanRefresh);
            ConnectDeviceCommand = new CommandHandler(ConnectDeviceCommandHandler, () => CanConnect);
            ResetConnectionCommand = new CommandHandler(ResetConnectionCommandHandler, () => CanResetConnection);
            ChooseSelectedRomCommand = new CommandHandler(ChooseSelectedRomCommandHandler);
            SendRomCommand = new CommandHandler(SendRomCommandHandler, () => CanSendRom);
            SaveLogCommand = new CommandHandler(SaveLogCommandHandler);
            ClearLogCommand = new CommandHandler(ClearLogCommandHandler);

            RefreshAvailableSerialPortsCommandHandler();
            SetConnectCommandText();
            SetAvailableFlashcarts();
            BuildMenuSendRom();

            StartThread();

            _ignoreAppSettingsChange = false;
        }

        /// <summary>
        /// Window menu items for the list of flashcarts.
        /// </summary>
        public ObservableCollection<MenuItemViewModel> MenuDevice { get; set; } = new ObservableCollection<MenuItemViewModel>();

        /// <summary>
        /// Window menu items for the list of serial ports.
        /// </summary>
        public ObservableCollection<MenuItemViewModel> MenuSerialPorts { get; set; } = new ObservableCollection<MenuItemViewModel>();

        /// <summary>
        /// Window menu items for send rom, and recently sent roms.
        /// </summary>
        public ObservableCollection<MenuItemViewModel> MenuSendRom { get; set; } = new ObservableCollection<MenuItemViewModel>();

        /// <summary>
        /// Currently selected serial port.
        /// </summary>
        public string? CurrentSerialPort
        {
            get
            {
                return AppConfig?.Connection?.SerialPort ?? string.Empty;
            }

            set
            {
                if ((AppConfig?.Connection?.SerialPort ?? string.Empty) == value)
                {
                    return;
                }

                AppConfig!.Connection.SerialPort = value ?? string.Empty;
                OnPropertyChanged(nameof(CurrentSerialPort));
                OnPropertyChanged(nameof(CanConnect));

                SaveAppSettings();
            }
        }

        /// <summary>
        /// List of available serial ports.
        /// </summary>
        public ObservableCollection<string> AvailableSerialPorts
        {
            get
            {
                return _availableSerialPorts;
            }

            set
            {
                _availableSerialPorts = value;
                BindingOperations.EnableCollectionSynchronization(_availableSerialPorts, _availableSerialPortsLock);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the list of serial ports is currently refreshing.
        /// </summary>
        public bool CanRefresh => !_isRefreshingSerialPorts;

        /// <summary>
        /// Command to refresh serial ports.
        /// </summary>
        public ICommand RefreshAvailableSerialPortsCommand { get; set; }

        /// <summary>
        /// Currently selected flashcart.
        /// </summary>
        public IFlashcart? CurrentFlashcart
        {
            get
            {
                return _currentFlashCart;
            }

            set
            {
                if (object.ReferenceEquals(_currentFlashCart, value))
                {
                    return;
                }

                _currentFlashCart = value;

                if (!object.ReferenceEquals(value, null))
                {
                    AppConfig.Device.Flashcart = value.GetType().Name;
                }
                else
                {
                    AppConfig.Device.Flashcart = string.Empty;
                }

                _flashcartIsValid = !object.ReferenceEquals(null, value);
                OnPropertyChanged(nameof(CurrentFlashcart));
                OnPropertyChanged(nameof(CanConnect));

                SaveAppSettings();
            }
        }

        /// <summary>
        /// List of available flashcarts.
        /// </summary>
        public ObservableCollection<IFlashcart> AvailableFlashcarts
        {
            get
            {
                return _availableFlashcarts;
            }

            set
            {
                _availableFlashcarts = value;
                BindingOperations.EnableCollectionSynchronization(_availableFlashcarts, _availableFlashcartsLock);
            }
        }

        /// <summary>
        /// Flag to indicate the app is currently connected to a flashcart (any connection level).
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }

            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the app can attempt to connect to the flashcart.
        /// </summary>
        public bool CanConnect => !_isConnecting && !string.IsNullOrEmpty(CurrentSerialPort) && _flashcartIsValid && _connectionError == false;

        /// <summary>
        /// Connection level text to show in the status b ar.
        /// </summary>
        public string ConnectCommandText { get; set; }

        /// <summary>
        /// Command to connect to the flashcart.
        /// </summary>
        public ICommand ConnectDeviceCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current connection can be reset.
        /// </summary>
        public bool CanResetConnection => _connectionError;

        /// <summary>
        /// Command to reset the connection.
        /// </summary>
        public ICommand ResetConnectionCommand { get; set; }

        /// <summary>
        /// Currently selected ROM path.
        /// </summary>
        public string? SelectedRom
        {
            get
            {
                return _selectedRomPath;
            }

            set
            {
                _selectedRomPath = value;
                OnPropertyChanged(nameof(SelectedRom));
                OnPropertyChanged(nameof(CanSendRom));
            }
        }

        /// <summary>
        /// Command to open a dialog and select a ROM.
        /// </summary>
        public ICommand ChooseSelectedRomCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether it's possible to send a ROM.
        /// </summary>
        public bool CanSendRom =>
            _isConnected
            && !_currentlySendingRom
            && _connectionError == false
            && _connectionLevel == ConnectionLevel.Flashcart;

        /// <summary>
        /// Command to send ROM to flashcart.
        /// </summary>
        public ICommand SendRomCommand { get; set; }

        /// <summary>
        /// List of known log messages.
        /// </summary>
        public ObservableCollection<string> LogMessages
        {
            get
            {
                return _logMessages;
            }

            set
            {
                _logMessages = value;
                BindingOperations.EnableCollectionSynchronization(_logMessages, _logMessagesLock);
            }
        }

        /// <summary>
        /// Command to save <see cref="LogMessages"/> to a file on disk.
        /// </summary>
        public ICommand SaveLogCommand { get; set; }

        /// <summary>
        /// Command to clear <see cref="LogMessages"/>.
        /// </summary>
        public ICommand ClearLogCommand { get; set; }

        /// <summary>
        /// Different category tabs.
        /// </summary>
        public ObservableCollection<TabViewModelBase> Tabs { get; set; } = new ObservableCollection<TabViewModelBase>();

        /// <summary>
        /// Status bar text to display whether the app is currently connected to the flashcart or not.
        /// </summary>
        public string StatusConnectedText
        {
            get
            {
                if (IsConnected)
                {
                    return "connected";
                }

                return "disconnected";
            }
        }

        /// <summary>
        /// Status bar text to display the connection level to the flashcart.
        /// </summary>
        public string StatusConnectionLevelText
        {
            get
            {
                if (_connectionLevel == ConnectionLevel.Flashcart)
                {
                    return "menu";
                }
                else if (_connectionLevel == ConnectionLevel.Rom)
                {
                    return "rom";
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Current connection level.
        /// </summary>
        public ConnectionLevel ConnectionLevel => _connectionLevel;

        /// <summary>
        /// Status bar text listing current serial port.
        /// </summary>
        public string StatusSerialPort
        {
            get
            {
                if (IsConnected)
                {
                    return CurrentSerialPort!;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Flag to indicate the app is currently scanning the computer for available serial ports.
        /// </summary>
        private bool IsRefreshing
        {
            get
            {
                return _isRefreshingSerialPorts;
            }

            set
            {
                _isRefreshingSerialPorts = value;
                OnPropertyChanged(nameof(IsRefreshing));
                OnPropertyChanged(nameof(CanRefresh));
            }
        }

        /// <summary>
        /// Recently sent files.
        /// </summary>
        public ObservableCollection<string> RecentlySentFiles
        {
            get
            {
                return AppConfig.RecentPath.RecentSendRom;
            }

            set
            {
                BindingOperations.EnableCollectionSynchronization(AppConfig.RecentPath.RecentSendRom, _recentlySentFilesLock);
            }
        }

        /// <summary>
        /// Current app settings.
        /// </summary>
        public AppConfigViewModel AppConfig { get; init; }

        /// <summary>
        /// Current known connected gebug ROM version.
        /// </summary>
        public Version? RomVersion
        {
            get => _romVersion;

            set
            {
                _romVersion = value;
                OnPropertyChanged(nameof(RomVersion));
                OnPropertyChanged(nameof(RomVersionString));
            }
        }

        /// <summary>
        /// Current known connected gebug ROM version, as a string.
        /// </summary>
        public string RomVersionString
        {
            get
            {
                if (object.ReferenceEquals(null, _romVersion))
                {
                    return "unk";
                }

                return _romVersion.ToString() + (HasExpansionBack ? "+xpak" : string.Empty);
            }
        }

        /// <summary>
        /// Gets the attached ROM memory size.
        /// </summary>
        public int RomMemorySize => _memSize;

        /// <summary>
        /// Gets a value indicating whether the console has an expansion pak installed or not.
        /// </summary>
        public bool HasExpansionBack => _isConnected && _memSize >= ExpansionPakMemSizeBytes;

        /// <summary>
        /// The long running query tasks.
        /// </summary>
        public ObservableCollection<QueryTaskViewModel> QueryTasks
        {
            get
            {
                return _queryTasks;
            }

            set
            {
                BindingOperations.EnableCollectionSynchronization(_queryTasks, _queryTasksLock);
            }
        }

        /// <summary>
        /// Attempts to stop all threads and gracefully shutdown.
        /// </summary>
        public void Shutdown()
        {
            _shutdown = true;

            // cleanup handled at end of ThreadMain
        }

        /// <summary>
        /// Accepts a task context. Checks for <see cref="QueryTaskContext.TaskIsUnique"/>, and if task is already
        /// running does nothing. Otherwise appends a new viewmodel and calls <see cref="QueryTaskContext.Begin"/>
        /// on the task.
        /// </summary>
        /// <param name="context">Query task context.</param>
        /// <returns>True if the task was started, false otherwise.</returns>
        public bool RegisterBegin(QueryTaskContext context)
        {
            if (context.TaskIsUnique)
            {
                var type = context.GetType();
                if (QueryTasks.Any(x => x.TaskType == type && x.State == QueryTask.TaskState.Running))
                {
                    return false;
                }
            }

            Action<QueryTaskViewModel> deleteAction = x =>
            {
                if (x.DeleteCommand.CanExecute(context))
                {
                    QueryTasks.Remove(x);
                }
            };

            var task = new QueryTaskViewModel(context, deleteAction);

            QueryTasks.Add(task);
            Task.Run(() => task.Begin());

            return true;
        }

        /// <summary>
        /// Clears existing <see cref="MenuSendRom"/> then rebuilds the menu, adding recently sent rom paths.
        /// </summary>
        private void BuildMenuSendRom()
        {
            MenuSendRom.Clear();

            var mivm = new MenuItemViewModel() { Header = "Open ..." };
            mivm.Command = new CommandHandler(ChooseAndSendRom, () => CanSendRom);

            MenuSendRom.Add(mivm);

            mivm = new MenuItemViewModel() { Header = "-----", IsEnabled = false };
            MenuSendRom.Add(mivm);

            mivm = new MenuItemViewModel() { Header = "Clear" };
            mivm.Command = new CommandHandler(ClearRecentlySentFiles);

            MenuSendRom.Add(mivm);

            Action<object?> dddd = x => SendSelectedRom((MenuItemViewModel)x!);

            foreach (var recent in RecentlySentFiles)
            {
                mivm = new MenuItemViewModel() { Header = recent };
                mivm.Command = new CommandHandler(dddd, () => CanSendRom);
                mivm.Value = (string)recent;

                MenuSendRom.Add(mivm);
            }
        }

        /// <summary>
        /// Clears existing <see cref="MenuSerialPorts"/> then rebuilds the menu.
        /// </summary>
        private void RefreshAvailableSerialPortsCommandHandler()
        {
            if (_isRefreshingSerialPorts)
            {
                return;
            }

            _dispatcher.BeginInvoke(() =>
            {
                string loadPort = AppConfig?.Connection?.SerialPort ?? string.Empty;
                MenuItemViewModel? loadInstance = null;
                MenuItemViewModel? last = null;

                foreach (var x in MenuSerialPorts)
                {
                    _menuSerialPortGroup.RemoveItem(x.Id);
                }

                MenuSerialPorts.Clear();

                var mivm = new MenuItemViewModel() { Header = "Refresh" };
                mivm.Command = new CommandHandler(RefreshAvailableSerialPortsCommandHandler, () => CanRefresh);

                MenuSerialPorts.Add(mivm);

                mivm = new MenuItemViewModel() { Header = "-----", IsEnabled = false };
                MenuSerialPorts.Add(mivm);

                // Don't write empty serial port to appsettings.
                // CurrentSerialPort = ...
                IsRefreshing = true;

                // Clearing the list sometimes clears AppConfig.Connection.SerialPort ?
                AvailableSerialPorts.Clear();

                List<string> foundPorts = new List<string>();

                var ports = SerialPort.GetPortNames();
                foreach (var port in ports)
                {
                    AvailableSerialPorts.Add(port);

                    Action<object?> dddd = x => MenuSerialPortClick((MenuItemViewModel)x!);

                    mivm = new MenuItemViewModel() { Header = port, IsCheckable = true, IsChecked = false };
                    mivm.Command = new CommandHandler(dddd);
                    mivm.Value = port;

                    // check each instance if this is what was saved in the config.
                    if (loadPort == port)
                    {
                        loadInstance = mivm;
                    }

                    _menuSerialPortGroup.AddItem(mivm, mivm.Id);
                    MenuSerialPorts.Add(mivm);

                    last = mivm;

                    foundPorts.Add(port);
                }

                _logger.Log(LogLevel.Information, "Found ports: " + String.Join(", ", foundPorts));

                IsRefreshing = false;

                if (object.ReferenceEquals(null, loadInstance))
                {
                    loadInstance = last;
                }

                if (!object.ReferenceEquals(null, loadInstance))
                {
                    MenuSerialPortClick(loadInstance);
                }
            });
        }

        /// <summary>
        /// Click handler for serial port menu item. Changes the <see cref="CurrentSerialPort"/>.
        /// </summary>
        /// <param name="self">Menu item that was clicked.</param>
        /// <exception cref="NullReferenceException">Throw if <see cref="MenuItemViewModel.Value"/> isn't string.</exception>
        private void MenuSerialPortClick(MenuItemViewModel self)
        {
            if (object.ReferenceEquals(null, self))
            {
                throw new NullReferenceException();
            }

            if (!typeof(string).IsAssignableFrom(self.Value?.GetType()))
            {
                throw new NullReferenceException("Incorrect self.Value");
            }

            _menuSerialPortGroup.CheckOne(self.Id);
            CurrentSerialPort = (string)self.Value;
        }

        /// <summary>
        /// Disconnects from flashcart.
        /// Unsubscribes from device manager message bus.
        /// Attempts to gracefully shutdown the device manager worker thread.
        /// </summary>
        /// <exception cref="Exception">Throw if could not gracefully shutdown device manager.</exception>
        private void Disconnect()
        {
            if (object.ReferenceEquals(null, _connectionServiceManager))
            {
                return;
            }

            if (!object.ReferenceEquals(null, _sendRomCancellation))
            {
                _sendRomCancellation.Cancel();

                System.Threading.Thread.Sleep(1000);
            }

            _connectionServiceManager.GebugUnsubscribe(_messageBusGebugLogSubscription);
            _connectionServiceManager.UnfloaderUnsubscribe(_messageBusUnfloaderLogSubscription);
            _connectionServiceManager.Stop();

            var sw = Stopwatch.StartNew();

            while (!_connectionServiceManager.IsShutdown)
            {
                System.Threading.Thread.Sleep(10);

                if (sw.Elapsed.TotalSeconds > 5)
                {
                    throw new Exception("Could not safely terminate device manager thread.");
                }
            }

            _connectionServiceManager = null;
            _connectionLevel = ConnectionLevel.NotConnected;

            if (!object.ReferenceEquals(null, _lastMessageSent))
            {
                _lastMessageSent.Stop();
                _lastMessageSent = null;
            }

            RomVersion = null;

            _isConnected = false;
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(CanConnect));
            OnPropertyChanged(nameof(StatusConnectedText));
            OnPropertyChanged(nameof(StatusConnectionLevelText));
            OnPropertyChanged(nameof(StatusSerialPort));
            OnPropertyChanged(nameof(CanSendRom));

            SetConnectCommandText();

            _logger.Log(LogLevel.Information, $"Disconnected");

            _sendRomCancellation = null;

            SetKnownMemorySize(0);
        }

        /// <summary>
        /// Sets the connection status text in the status bar.
        /// </summary>
        private void SetConnectCommandText()
        {
            if (_connectionError)
            {
                ConnectCommandText = "Error";
            }
            else if (_isConnecting)
            {
                ConnectCommandText = "Connecting";
            }
            else if (_isConnected == false)
            {
                ConnectCommandText = "Connect";
            }
            else
            {
                ConnectCommandText = "Disconnect";
            }

            OnPropertyChanged(nameof(ConnectCommandText));
        }

        /// <summary>
        /// Trigger a toggle for the device manager, either connect or disconnect.
        /// </summary>
        private void ConnectDeviceCommandHandler()
        {
            if (_isConnecting)
            {
                return;
            }

            if (_isConnected == false)
            {
                ConnectDeviceCommandHandler_Connect();
            }
            else
            {
                ConnectDeviceCommandHandler_Disconnect();
            }
        }

        /// <summary>
        /// Command to connect to the flashcart.
        /// Will send test messages to establish connection level.
        /// </summary>
        /// <exception cref="NullReferenceException">Throw if current flashcart not set.</exception>
        private void ConnectDeviceCommandHandler_Connect()
        {
            _dispatcher.BeginInvoke(() =>
            {
                if (object.ReferenceEquals(null, CurrentFlashcart))
                {
                    throw new NullReferenceException();
                }

                Disconnect();
                _isConnecting = true;
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(ConnectCommandText));
                OnPropertyChanged(nameof(CanSendRom));

                _connectionServiceResolver.CreateOnceDeviceManager(CurrentFlashcart, _logger);
                _connectionServiceManager = _connectionServiceResolver.GetDeviceManager();

                _messageBusGebugLogSubscription = _connectionServiceManager!.Subscribe(MessageBusLogGebugCallback);
                _messageBusUnfloaderLogSubscription = _connectionServiceManager!.Subscribe(MessageBusLogUnfloaderCallback);

                _connectionServiceManager.Start(CurrentSerialPort!);

                var testResult = _connectionServiceManager.TestInMenu();

                _logger.Log(LogLevel.Information, $"Connection level test, checking if in flashcart menu: {testResult}");

                if (testResult)
                {
                    _connectionLevel = ConnectionLevel.Flashcart;
                }

                if (false && !testResult)
                {
                    testResult = _connectionServiceManager.TestInRom();
                    _logger.Log(LogLevel.Information, $"Connection level test, checking if in rom: {testResult}");

                    if (testResult)
                    {
                        _connectionLevel = ConnectionLevel.Rom;

                        _logger.Log(LogLevel.Information, $"Send ping");
                        _connectionServiceManager.SendMessage(new GebugMetaPingMessage());

                        _logger.Log(LogLevel.Information, $"Send version request");
                        _connectionServiceManager.SendMessage(new GebugMetaVersionMessage());
                    }

                    if (testResult == false)
                    {
                        _connectionServiceManager.Stop();
                        _logger.Log(LogLevel.Error, $"Connection level test: test command failed");

                        _connectionError = true;
                        _isConnected = false;

                        _isConnecting = false;
                        OnPropertyChanged(nameof(IsConnected));
                        OnPropertyChanged(nameof(CanResetConnection));
                        OnPropertyChanged(nameof(CanConnect));
                        OnPropertyChanged(nameof(CanSendRom));

                        OnPropertyChanged(nameof(StatusConnectedText));
                        OnPropertyChanged(nameof(StatusConnectionLevelText));
                        OnPropertyChanged(nameof(StatusSerialPort));

                        SetConnectCommandText();

                        return;
                    }
                }

                _isConnected = true;
                _isConnecting = false;
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanSendRom));

                OnPropertyChanged(nameof(StatusConnectedText));
                OnPropertyChanged(nameof(StatusConnectionLevelText));
                OnPropertyChanged(nameof(StatusSerialPort));

                SetConnectCommandText();
            });
        }

        /// <summary>
        /// Triggers <see cref="Disconnect"/> and notifies property changed.
        /// </summary>
        private void ConnectDeviceCommandHandler_Disconnect()
        {
            _dispatcher.BeginInvoke(() =>
            {
                Disconnect();

                _isConnected = false;
                _isConnecting = false;
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanSendRom));

                OnPropertyChanged(nameof(StatusConnectedText));
                OnPropertyChanged(nameof(StatusConnectionLevelText));
                OnPropertyChanged(nameof(StatusSerialPort));

                SetConnectCommandText();
            });
        }

        /// <summary>
        /// Command to disconnect from the flashcart and set the app back into a state
        /// that it can connect again.
        /// </summary>
        private void ResetConnectionCommandHandler()
        {
            Disconnect();

            _isConnected = false;
            _isConnecting = false;

            _connectionError = false;
            OnPropertyChanged(nameof(CanResetConnection));

            SetConnectCommandText();
        }

        /// <summary>
        /// Adds all supported flashcart types to the menu.
        /// This can only be run once.
        /// Available flashcarts are hard coded below.
        /// </summary>
        private void SetAvailableFlashcarts()
        {
            if (_setAvailableFlashcarts)
            {
                return;
            }

            _setAvailableFlashcarts = true;

            string loadTypeName = AppConfig?.Device?.Flashcart ?? string.Empty;
            MenuItemViewModel? loadInstance = null;

            MenuDevice.Clear();

            var mivm = new MenuItemViewModel() { Header = "Everdrive", IsCheckable = true, IsChecked = true };
            mivm.Command = new CommandHandler(() => MenuFlashcartClick(mivm));
            mivm.Value = (Everdrive)Workspace.Instance.ServiceProvider.GetService(typeof(Everdrive))!;

            // If this were iterating a loop, check each instance if this is what was saved in the config.
            if (loadTypeName == typeof(Everdrive).Name)
            {
                loadInstance = mivm;
            }

            _menuDeviceGroup.AddItem(mivm, mivm.Id);
            MenuDevice.Add(mivm);

            AvailableFlashcarts.Add((Everdrive)mivm.Value);

            if (object.ReferenceEquals(null, loadInstance))
            {
                loadInstance = mivm;
            }

            if (object.ReferenceEquals(null, CurrentFlashcart) && !object.ReferenceEquals(null, loadInstance))
            {
                MenuFlashcartClick(loadInstance);
            }
        }

        /// <summary>
        /// Click command handler for clicking on a flashcart from the menu.
        /// </summary>
        /// <param name="self">Menu item that was clicked.</param>
        /// <exception cref="NullReferenceException">Throw if <see cref="MenuItemViewModel.Value"/> isn't <see cref="IFlashcart"/>.</exception>
        private void MenuFlashcartClick(MenuItemViewModel self)
        {
            if (object.ReferenceEquals(null, self))
            {
                throw new NullReferenceException();
            }

            if (!typeof(IFlashcart).IsAssignableFrom(self.Value?.GetType()))
            {
                throw new NullReferenceException("Incorrect self.Value");
            }

            _menuDeviceGroup.CheckOne(self.Id);
            CurrentFlashcart = (IFlashcart)self.Value;
        }

        /// <summary>
        /// Clears recently sent rom paths from <see cref="RecentlySentFiles"/>.
        /// The first several entries in the menu are not paths.
        /// </summary>
        private void ClearRecentlySentFiles()
        {
            RecentlySentFiles.Clear();

            while (MenuSendRom.Count > RecentSendRomPermanentCount)
            {
                MenuSendRom.RemoveAt(RecentSendRomPermanentCount);
            }

            SaveAppSettings();
        }

        /// <summary>
        /// Meta method, calls <see cref="ChooseSelectedRomCommandHandler"/> and if a <see cref="SelectedRom"/>
        /// is set then calls <see cref="SendSelectedRom"/>.
        /// </summary>
        private void ChooseAndSendRom()
        {
            ChooseSelectedRomCommandHandler();

            // If the user cancels, don't try to send the last file.
            if (string.IsNullOrEmpty(SelectedRom))
            {
                return;
            }

            var mivm = new MenuItemViewModel()
            {
                Value = SelectedRom,
            };

            SendSelectedRom(mivm);
        }

        /// <summary>
        /// Updates app state, user interface, and app settings.
        /// If the selected path is valid, and the above updates succeed, then calls
        /// <see cref="SendRomCommandHandler"/> to send the selected rom to the flashcart.
        /// </summary>
        /// <param name="self">Menu item that was clicked.</param>
        /// <exception cref="NullReferenceException">Throw if <see cref="MenuItemViewModel.Value"/> isn't <see cref="string"/>.</exception>
        private void SendSelectedRom(MenuItemViewModel self)
        {
            if (object.ReferenceEquals(null, self))
            {
                throw new NullReferenceException();
            }

            if (!typeof(string).IsAssignableFrom(self.Value?.GetType()))
            {
                throw new NullReferenceException("Incorrect self.Value");
            }

            string path = (string)self.Value;

            if (!File.Exists(path))
            {
                string messageBoxText = $"File no longer exists: {path}";
                string caption = "File not found";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);

                RecentlySentFiles.Remove(path);
                return;
            }

            if (RecentlySentFiles.Contains(path))
            {
                RecentlySentFiles.Remove(path);
            }

            RecentlySentFiles.Insert(0, path);

            while (RecentlySentFiles.Count > MaxRecentlySentFiles)
            {
                RecentlySentFiles.RemoveAt(MaxRecentlySentFiles);
            }

            SaveAppSettings();

            int i;
            bool found = false;
            for (i = RecentSendRomPermanentCount; i < MenuSendRom.Count; i++)
            {
                if (object.ReferenceEquals(null, MenuSendRom[i]))
                {
                    throw new NullReferenceException();
                }

                if (!typeof(string).IsAssignableFrom(MenuSendRom[i].Value?.GetType()))
                {
                    throw new NullReferenceException("Incorrect self.Value");
                }

                if (string.Compare(path, (string)MenuSendRom[i].Value!, true) == 0)
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                MenuSendRom.RemoveAt(i);
            }

            Action<object?> dddd = x => SendSelectedRom((MenuItemViewModel)x!);
            var mivm = new MenuItemViewModel() { Header = path };
            mivm.Command = new CommandHandler(dddd, () => CanSendRom);
            mivm.Value = (string)path;

            // Insert at top of list.
            MenuSendRom.Insert(RecentSendRomPermanentCount, mivm);

            while (MenuSendRom.Count > RecentSendRomPermanentCount + MaxRecentlySentFiles)
            {
                MenuSendRom.RemoveAt(RecentSendRomPermanentCount + MaxRecentlySentFiles);
            }

            SelectedRom = path;
            SendRomCommandHandler();
        }

        /// <summary>
        /// Sends the selected rom to the flashcart.
        /// </summary>
        /// <exception cref="NullReferenceException">Throw if <see cref="SelectedRom"/> is null.</exception>
        private void SendRomCommandHandler()
        {
            if (_currentlySendingRom)
            {
                return;
            }

            _dispatcher.BeginInvoke(() =>
            {
                Task.Run(() =>
                {
                    _dispatcher.BeginInvoke(() =>
                    {
                        _currentlySendingRom = true;
                        OnPropertyChanged(nameof(CanSendRom));
                    });

                    _logger.Log(LogLevel.Information, $"Send file: {SelectedRom}");

                    if (object.ReferenceEquals(null, _sendRomCancellation))
                    {
                        _sendRomCancellation = new CancellationTokenSource();
                    }

                    if (object.ReferenceEquals(null, SelectedRom))
                    {
                        throw new NullReferenceException();
                    }

                    _connectionServiceManager!.SendRom(SelectedRom, _sendRomCancellation.Token);

                    _sendRomCancellation = null;

                    Task.Run(() =>
                    {
                        // need to delay after loading rom or everdrive gets cranky
                        System.Threading.Thread.Sleep(5000);

                        // if disconnected during above sleep, don't send ping.
                        if (!object.ReferenceEquals(null, _connectionServiceManager))
                        {
                            _logger.Log(LogLevel.Information, $"Send ping");
                            _connectionServiceManager.SendMessage(new GebugMetaPingMessage());

                            _logger.Log(LogLevel.Information, $"Send version request");
                            _connectionServiceManager.SendMessage(new GebugMetaVersionMessage());
                        }
                    });

                    _dispatcher.BeginInvoke(() =>
                    {
                        _currentlySendingRom = false;
                        OnPropertyChanged(nameof(CanSendRom));
                    });
                }).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.Log(LogLevel.Information, $"SendRom failed");
                    }
                });
            });
        }

        /// <summary>
        /// Opens dialog box to choose a rom.
        /// </summary>
        private void ChooseSelectedRomCommandHandler()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".z64", // Default file extension
                Filter = "z64 format | *.z64", // Filter files by extension
            };

            if (System.IO.Directory.Exists(AppConfig.RecentPath.SendRomFolder))
            {
                dialog.InitialDirectory = AppConfig.RecentPath.SendRomFolder;
            }

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true && !string.IsNullOrEmpty(dialog.FileName))
            {
                SelectedRom = dialog.FileName;

                AppConfig.RecentPath.SendRomFolder = System.IO.Path.GetDirectoryName(dialog.FileName);
                SaveAppSettings();
            }
            else
            {
                // clear last selected rom if user cancels.
                // This is used to communicate status in the "send rom" menu click ...
                SelectedRom = string.Empty;
            }
        }

        /// <summary>
        /// Saves log messages to a file.
        /// </summary>
        private void SaveLogCommandHandler()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "gebug64-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"), // Default file name
                DefaultExt = ".txt", // Default file extension
                Filter = "TXT format | *.txt", // Filter files by extension
            };

            // Show save file dialog box
            bool? result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result == true && !string.IsNullOrEmpty(dialog.FileName))
            {
                string filename = dialog.FileName;
                var content = LogMessages.Select(x => x).ToList();
                System.IO.File.WriteAllLines(filename, content);
            }
        }

        /// <summary>
        /// Clears all log messages.
        /// </summary>
        private void ClearLogCommandHandler()
        {
            LogMessages.Clear();
        }

        /// <summary>
        /// If <see cref="_thread"/> is null, then starts a new instance of the thread to run <see cref="ThreadMain"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throw if attempted to restart a dead thread.</exception>
        private void StartThread()
        {
            if (object.ReferenceEquals(null, _thread))
            {
                _thread = new Thread(new ThreadStart(ThreadMain));
                _thread.IsBackground = true;

                _thread.Start();

                return;
            }

            if (_thread.IsAlive)
            {
                return;
            }

            throw new InvalidOperationException("DeviceManager thread cannot be restarted.");
        }

        /// <summary>
        /// Call back for all gebug messages.
        /// Writes message to the log.
        /// </summary>
        /// <param name="msg">Gebug level message from flashcart.</param>
        private void MessageBusLogGebugCallback(IGebugMessage msg)
        {
            _dispatcher.BeginInvoke(() =>
            {
                if (_connectionLevel != ConnectionLevel.Rom)
                {
                    _connectionLevel = ConnectionLevel.Rom;

                    OnPropertyChanged(nameof(CanSendRom));
                    OnPropertyChanged(nameof(StatusConnectionLevelText));
                }

                // If the message is a version message, then the connected rom version is now known.
                if (object.ReferenceEquals(null, RomVersion)
                    && msg.Category == GebugMessageCategory.Meta
                    && msg.Command == (int)GebugCmdMeta.Version)
                {
                    var versionMessage = (GebugMetaVersionMessage)msg;
                    if (!object.ReferenceEquals(null, versionMessage))
                    {
                        RomVersion = new Version(
                            versionMessage.VersionA,
                            versionMessage.VersionB,
                            versionMessage.VersionC,
                            versionMessage.VersionD);

                        SetKnownMemorySize((int)versionMessage.Size);
                    }
                }

                _logger.Log(LogLevel.Information, "Receive " + msg.ToString());
                ////System.Diagnostics.Debug.WriteLine("Receive " + msg.ToString());
            });
        }

        /// <summary>
        /// Call back for all UNFLoader messages.
        /// Writes message to the log.
        /// </summary>
        /// <param name="packet">UNFLoader level message from flashcart.</param>
        private void MessageBusLogUnfloaderCallback(IUnfloaderPacket packet)
        {
            _dispatcher.BeginInvoke(() =>
            {
                if (_connectionLevel == ConnectionLevel.NotConnected)
                {
                    _connectionLevel = ConnectionLevel.Flashcart;

                    OnPropertyChanged(nameof(CanSendRom));
                    OnPropertyChanged(nameof(StatusConnectionLevelText));
                }

                if (packet.MessageType == Unfloader.Protocol.Unfloader.Message.MessageType.UnfloaderMessageType.Text)
                {
                    var textMessage = (TextPacket)packet;
                    if (!object.ReferenceEquals(null, textMessage) && !string.IsNullOrEmpty(textMessage.Content))
                    {
                        _logger.Log(LogLevel.Information, Gebug64.Win.Formatters.Text.RemoveTrailingNonVisible(textMessage.Content));
                        ////System.Diagnostics.Debug.WriteLine(Gebug64.Win.Formatters.Text.RemoveTrailingNonVisible(textMessage.Content));
                    }
                    else
                    {
                        _logger.Log(LogLevel.Information, "Received empty UNFLoader text packet");
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Information, "Receive " + packet.ToString());
                    ////System.Diagnostics.Debug.WriteLine("Receive " + packet.ToString());
                }
            });
        }

        /// <summary>
        /// Saves app settings to disk and calls <see cref="ConfigViewModelBase.ClearIsDirty"/>.
        /// </summary>
        private void SaveAppSettings()
        {
            if (_ignoreAppSettingsChange)
            {
                return;
            }

            Workspace.Instance.SaveAppSettings();

            AppConfig.ClearIsDirty();
        }

        /// <summary>
        /// View model main thread.
        /// Tracks how long since a message was last received from the flashcart.
        /// If too much time has elapsed, attempts to ping the flashcart.
        /// If no messages are received after <see cref="TimeoutDisconnectSec"/> then
        /// the connection is disconnected.
        /// </summary>
        private void ThreadMain()
        {
            while (_shutdown == false)
            {
                if (object.ReferenceEquals(null, _connectionServiceManager))
                {
                    System.Threading.Thread.Sleep(500);
                    continue;
                }

                if (_connectionLevel == ConnectionLevel.Rom
                    && _connectionServiceManager.SinceRomMessageReceived.TotalSeconds > PingIntervalSec)
                {
                    if (object.ReferenceEquals(null, _lastMessageSent))
                    {
                        _lastMessageSent = Stopwatch.StartNew();
                    }

                    if (_lastMessageSent.Elapsed.TotalSeconds > PingIntervalSec)
                    {
                        _logger.Log(LogLevel.Information, $"Send ping");
                        _connectionServiceManager.SendMessage(new GebugMetaPingMessage());

                        _lastMessageSent.Stop();
                        _lastMessageSent = Stopwatch.StartNew();
                    }
                }

                if (_connectionLevel == ConnectionLevel.Rom
                    && _connectionServiceManager.SinceRomMessageReceived.TotalSeconds > TimeoutDisconnectSec)
                {
                    _dispatcher.BeginInvoke(() => Task.Run(() => Disconnect()));
                }

                System.Threading.Thread.Sleep(100);
            }

            // shutdown notification received.
            if (!object.ReferenceEquals(null, _connectionServiceManager))
            {
                _connectionServiceManager.Stop();
            }
        }

        /// <summary>
        /// Update tracked memory related values.
        /// </summary>
        /// <param name="size">Size in bytes of console memory.</param>
        private void SetKnownMemorySize(int size)
        {
            _memSize = size;

            OnPropertyChanged(nameof(RomMemorySize));
            OnPropertyChanged(nameof(HasExpansionBack));
            OnPropertyChanged(nameof(RomVersionString));
        }
    }
}
