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
using Antlr4.Runtime.Atn;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Unfloader;
using Gebug64.Unfloader.Protocol.Unfloader.Message;
using Gebug64.Win.Config;
using Gebug64.Win.Mvvm;
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
        /// If the device is connected, and no communication occurs for this many seconds,
        /// then try to send the ping command.
        /// </summary>
        private const int PingIntervalSec = 20;

        /// <summary>
        /// If no communiation occurs for this many seconds, assume the device is no
        /// longer connected.
        /// </summary>
        private const int TimeoutDisconnectSec = 70;

        private const int MaxRecentlySentFiles = 10;

        private bool _ignoreAppSettingsChange = false;

        private Version? _romVersion = null;

        private bool _isConnected = false;
        private bool _isConnecting = false;
        private bool _isRefreshing = false;
        private bool _connectionError = false;
        private IFlashcart _currentFlashCart;
        private string _selectedRomPath;

        private bool _flashcartIsValid = false;
        private bool _currentlySendingRom = false;

        private bool _setAvailableFlashcarts = false;

        private Guid _messageBusGebugLogSubscription = Guid.Empty;
        private Guid _messageBusUnfloaderLogSubscription = Guid.Empty;

        private readonly object _availableSerialPortsLock = new object();
        private ObservableCollection<string> _availableSerialPorts = new ObservableCollection<string>();

        private readonly object _availableFlashcartsLock = new object();
        private ObservableCollection<IFlashcart> _availableFlashcarts = new ObservableCollection<IFlashcart>();

        private readonly object _logMessagesLock = new object();
        private ObservableCollection<string> _logMessages = new ObservableCollection<string>();

        private readonly object _recentlySentFilesLock = new object();

        private readonly ILogger _logger;

        private IConnectionServiceProvider? _connectionServiceManager;
        private readonly IConnectionServiceProviderResolver _connectionServiceResolver;

        private readonly Dispatcher _dispatcher;
        private bool _shutdown = false;
        private Thread _thread;

        private Stopwatch? _lastMessageSent = null;

        private ConnectionLevel _connectionLevel = ConnectionLevel.NotConnected;

        private CancellationTokenSource? _sendRomCancellation = null;

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

                AppConfig.Connection.SerialPort = value ?? string.Empty;
                OnPropertyChanged(nameof(CurrentSerialPort));
                OnPropertyChanged(nameof(CanConnect));

                SaveAppSettings();
            }
        }

        public ObservableCollection<string> AvailableSerialPorts
        {
            get { return _availableSerialPorts; }
            set
            {
                _availableSerialPorts = value;
                BindingOperations.EnableCollectionSynchronization(_availableSerialPorts, _availableSerialPortsLock);
            }
        }

        public bool CanRefresh => !_isRefreshing;

        public ICommand RefreshAvailableSerialPortsCommand { get; set; }

        public IFlashcart CurrentFlashcart
        {
            get { return _currentFlashCart; }
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

        public ObservableCollection<IFlashcart> AvailableFlashcarts
        {
            get { return _availableFlashcarts; }
            set
            {
                _availableFlashcarts = value;
                BindingOperations.EnableCollectionSynchronization(_availableFlashcarts, _availableFlashcartsLock);
            }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; OnPropertyChanged(nameof(IsConnected)); }
        }

        public bool CanConnect => !_isConnecting && !string.IsNullOrEmpty(CurrentSerialPort) && _flashcartIsValid && _connectionError == false;

        public string ConnectCommandText { get; set; }

        public ICommand ConnectDeviceCommand { get; set; }

        public bool CanResetConnection => _connectionError;

        public ICommand ResetConnectionCommand { get; set; }

        public string SelectedRom
        {
            get { return _selectedRomPath; }
            set
            {
                _selectedRomPath = value;
                OnPropertyChanged(nameof(SelectedRom));
                OnPropertyChanged(nameof(CanSendRom));
            }
        }

        public ICommand ChooseSelectedRomCommand { get; set; }

        public bool CanSendRom =>
            _isConnected
            && !_currentlySendingRom
            && _connectionError == false
            && _connectionLevel == ConnectionLevel.Everdrive;

        public ICommand SendRomCommand { get; set; }

        public ObservableCollection<string> LogMessages
        {
            get { return _logMessages; }
            set
            {
                _logMessages = value;
                BindingOperations.EnableCollectionSynchronization(_logMessages, _logMessagesLock);
            }
        }

        public ICommand SaveLogCommand { get; set; }

        public ICommand ClearLogCommand { get; set; }

        public ObservableCollection<TabViewModelBase> Tabs { get; set; } = new ObservableCollection<TabViewModelBase>();

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

        public string StatusConnectionLevelText
        {
            get
            {
                if (_connectionLevel == ConnectionLevel.Everdrive)
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

        private bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
                OnPropertyChanged(nameof(CanRefresh));
            }
        }

        private OnlyOneChecked<MenuItemViewModel, Guid> _menuDeviceGroup = new OnlyOneChecked<MenuItemViewModel, Guid>();
        private OnlyOneChecked<MenuItemViewModel, Guid> _menuSerialPortGroup = new OnlyOneChecked<MenuItemViewModel, Guid>();

        public ObservableCollection<MenuItemViewModel> MenuDevice { get; set; } = new ObservableCollection<MenuItemViewModel>();
        public ObservableCollection<MenuItemViewModel> MenuSerialPorts { get; set; } = new ObservableCollection<MenuItemViewModel>();
        public ObservableCollection<MenuItemViewModel> MenuSendRom { get; set; } = new ObservableCollection<MenuItemViewModel>();

        public ObservableCollection<string> RecentlySentFiles
        {
            get { return AppConfig.RecentSendRom; }
            set
            {
                BindingOperations.EnableCollectionSynchronization(AppConfig.RecentSendRom, _recentlySentFilesLock);
            }
        }

        public AppConfigViewModel AppConfig { get; set; }

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

        public string RomVersionString => _romVersion?.ToString() ?? "unk";

        public MainWindowViewModel(ILogger logger, IConnectionServiceProviderResolver deviceManagerResolver, AppConfigViewModel appConfig)
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

            // Look up the tabs to add based on the interface ICategoryTabViewModel.
            var assemblyTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            var tabViewmodelTypes = assemblyTypes.Where(x =>
                typeof(ICategoryTabViewModel).IsAssignableFrom(x)
                && !x.IsAbstract
                && x.IsClass);
            var tabViewmodels = new List<TabViewModelBase>();
            foreach (var t in tabViewmodelTypes)
            {
                tabViewmodels.Add((TabViewModelBase)Workspace.Instance.ServiceProvider.GetService(t)!);
            }

            foreach (var t in tabViewmodels.OrderBy(x => x.DisplayOrder))
            {
                Tabs.Add(t);
            }

            StartThread();

            _ignoreAppSettingsChange = false;
        }

        private enum ConnectionLevel
        {
            NotConnected,
            Everdrive,
            Rom,
        }

        public void Shutdown()
        {
            _shutdown = true;

            // cleanup handled at end of ThreadMain
        }

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

        private void RefreshAvailableSerialPortsCommandHandler()
        {
            if (_isRefreshing)
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

        private void MenuSerialPortClick(MenuItemViewModel self)
        {
            _menuSerialPortGroup.CheckOne(self.Id);
            CurrentSerialPort = (string)self.Value;
        }

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
        }

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

        private void ConnectDeviceCommandHandler_Connect()
        {
            _dispatcher.BeginInvoke(() =>
            {
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
                    _connectionLevel = ConnectionLevel.Everdrive;
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

        private void ResetConnectionCommandHandler()
        {
            Disconnect();

            _isConnected = false;
            _isConnecting = false;

            _connectionError = false;
            OnPropertyChanged(nameof(CanResetConnection));

            SetConnectCommandText();
        }

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

        private void MenuFlashcartClick(MenuItemViewModel self)
        {
            _menuDeviceGroup.CheckOne(self.Id);
            CurrentFlashcart = (IFlashcart)self.Value;
        }

        private void ClearRecentlySentFiles()
        {
            RecentlySentFiles.Clear();

            while (MenuSendRom.Count > 3)
            {
                MenuSendRom.RemoveAt(3);
            }

            SaveAppSettings();
        }

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

        private void SendSelectedRom(MenuItemViewModel self)
        {
            string path = (string)self.Value;

            if (!File.Exists(path))
            {
                string messageBoxText = $"File no longer exists: {path}";
                string caption = "File not found";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBoxResult result;

                MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);

                RecentlySentFiles.Remove(path);
                return;
            }

            if (RecentlySentFiles.Contains(path))
            {
                RecentlySentFiles.Remove(path);
            }

            RecentlySentFiles.Insert(0, path);

            if (RecentlySentFiles.Count > MaxRecentlySentFiles)
            {
                RecentlySentFiles.RemoveAt(MaxRecentlySentFiles);
            }

            SaveAppSettings();

            int i;
            bool found = false;
            for (i = 0; i < MenuSendRom.Count; i++)
            {
                if (string.Compare(path, (string)MenuSendRom[i].Value, true) == 0)
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
            MenuSendRom.Insert(3, mivm);

            SelectedRom = path;
            SendRomCommandHandler();
        }

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

        private void ChooseSelectedRomCommandHandler()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".z64", // Default file extension
                Filter = "z64 format | *.z64" // Filter files by extension
            };

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true && !string.IsNullOrEmpty(dialog.FileName))
            {
                SelectedRom = dialog.FileName;
            }
            else
            {
                // clear last selected rom if user cancels.
                // This is used to communicate status in the "send rom" menu click ...
                SelectedRom = string.Empty;
            }
        }

        private void SaveLogCommandHandler()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "gebug64-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"), // Default file name
                DefaultExt = ".txt", // Default file extension
                Filter = "TXT format | *.txt" // Filter files by extension
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

        private void ClearLogCommandHandler()
        {
            LogMessages.Clear();
        }

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

                    }
                }

                _logger.Log(LogLevel.Information, "Receive " + msg.ToString());
                ////System.Diagnostics.Debug.WriteLine("Receive " + msg.ToString());
            });
        }

        private void MessageBusLogUnfloaderCallback(IUnfloaderPacket packet)
        {
            _dispatcher.BeginInvoke(() =>
            {
                if (_connectionLevel == ConnectionLevel.NotConnected)
                {
                    _connectionLevel = ConnectionLevel.Everdrive;

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

        private void SaveAppSettings()
        {
            if (_ignoreAppSettingsChange)
            {
                return;
            }

            Workspace.Instance.SaveAppSettings();

            AppConfig.ClearIsDirty();
        }

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
    }
}
