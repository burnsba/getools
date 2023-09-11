using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Flashcart;
using Gebug64.Unfloader.Message;
using Gebug64.Win.Mvvm;
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

        private bool _isConnected = false;
        private bool _isConnecting = false;
        private bool _isRefreshing = false;
        private bool _connectionError = false;
        private IFlashcart _currentFlashCart;
        private string _currentSerialPort;
        private string _selectedRomPath;

        private bool _serialPortIsValid = false;
        private bool _flashcartIsValid = false;
        private bool _selectedRomPathIsValid = false;
        private bool _currentlySendingRom = false;

        private bool _setAvailableFlashcarts = false;

        private readonly object _availableSerialPortsLock = new object();
        private ObservableCollection<string> _availableSerialPorts = new ObservableCollection<string>();

        private readonly object _availableFlashcartsLock = new object();
        private ObservableCollection<IFlashcart> _availableFlashcarts = new ObservableCollection<IFlashcart>();

        private readonly object _logMessagesLock = new object();
        private ObservableCollection<string> _logMessages = new ObservableCollection<string>();

        private readonly ILogger _logger;

        private IDeviceManager? _deviceManager;

        private readonly Dispatcher _dispatcher;
        private bool _shutdown = false;
        private Thread _thread;

        private Stopwatch? _lastMessageReceived = null;
        private Stopwatch? _lastMessageSent = null;

        private ConnectionLevel _connectionLevel = ConnectionLevel.NotConnected;

        public string? CurrentSerialPort
        {
            get { return _currentSerialPort; }
            set
            {
                _currentSerialPort = value;
                _serialPortIsValid = !string.IsNullOrEmpty(value);
                OnPropertyChanged(nameof(CurrentSerialPort));
                OnPropertyChanged(nameof(CanConnect));
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
                _currentFlashCart = value;
                _flashcartIsValid = !object.ReferenceEquals(null, value);
                OnPropertyChanged(nameof(CurrentFlashcart));
                OnPropertyChanged(nameof(CanConnect));
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

        public bool CanConnect => !_isConnecting && _serialPortIsValid && _flashcartIsValid && _connectionError == false;

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
                _selectedRomPathIsValid = !string.IsNullOrEmpty(value) && File.Exists(_selectedRomPath);
                OnPropertyChanged(nameof(SelectedRom));
                OnPropertyChanged(nameof(CanSendRom));
            }
        }

        public ICommand ChooseSelectedRomCommand { get; set; }

        public bool CanSendRom =>
            _isConnected
            && _selectedRomPathIsValid
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

        public MainWindowViewModel(ILogger logger)
        {
            _logger = logger;
            _dispatcher = Dispatcher.CurrentDispatcher;

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

            StartThread();
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

        private void RefreshAvailableSerialPortsCommandHandler()
        {
            if (_isRefreshing)
            {
                return;
            }

            _dispatcher.BeginInvoke(() =>
            {
                IsRefreshing = true;
                CurrentSerialPort = null;
                AvailableSerialPorts.Clear();

                var ports = SerialPort.GetPortNames();
                foreach (var port in ports)
                {
                    AvailableSerialPorts.Add(port);
                }

                IsRefreshing = false;

                if (string.IsNullOrEmpty(CurrentSerialPort) && ports.Any())
                {
                    CurrentSerialPort = AvailableSerialPorts.Last();
                }
            });
        }

        private void Disconnect()
        {
            if (object.ReferenceEquals(null, _deviceManager))
            {
                return;
            }

            _deviceManager.Stop();

            var sw = Stopwatch.StartNew();

            while (!_deviceManager.IsShutdown)
            {
                System.Threading.Thread.Sleep(10);

                if (sw.Elapsed.TotalSeconds > 5)
                {
                    throw new Exception("Could not safely terminate device manager thread.");
                }
            }

            _deviceManager = null;
            _connectionLevel = ConnectionLevel.NotConnected;

            if (!object.ReferenceEquals(null, _lastMessageReceived))
            {
                _lastMessageReceived.Stop();
                _lastMessageReceived = null;
            }

            if (!object.ReferenceEquals(null, _lastMessageSent))
            {
                _lastMessageSent.Stop();
                _lastMessageSent = null;
            }

            _isConnected = false;
            OnPropertyChanged(nameof(CanConnect));
            OnPropertyChanged(nameof(CanSendRom));

            SetConnectCommandText();

            _logger.Log(LogLevel.Information, $"Disconnected");
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

                _deviceManager = new DeviceManager(CurrentFlashcart);

                _deviceManager.Init(CurrentSerialPort);
                _deviceManager.Start();

                var testResult = _deviceManager.TestFlashcartConnected();

                _logger.Log(LogLevel.Information, $"Connection level test, checking if in flashcart menu: {testResult}");

                if (testResult)
                {
                    _connectionLevel = ConnectionLevel.Everdrive;
                }

                if (!testResult)
                {
                    testResult = _deviceManager.TestRomConnected();
                    _logger.Log(LogLevel.Information, $"Connection level test, checking if in rom: {testResult}");

                    if (testResult)
                    {
                        _connectionLevel = ConnectionLevel.Rom;

                        _logger.Log(LogLevel.Information, $"Send ping");
                        _deviceManager.EnqueueMessage(new RomMetaMessage(Unfloader.Message.MessageType.GebugCmdMeta.Ping) { Source = CommunicationSource.Pc });
                    }

                    if (testResult == false)
                    {
                        _deviceManager.Stop();
                        _logger.Log(LogLevel.Error, $"Connection level test: test command failed");

                        _connectionError = true;
                        _isConnected = false;

                        _isConnecting = false;
                        OnPropertyChanged(nameof(CanResetConnection));
                        OnPropertyChanged(nameof(CanConnect));
                        OnPropertyChanged(nameof(CanSendRom));

                        SetConnectCommandText();

                        return;
                    }
                }

                _isConnected = true;
                _isConnecting = false;
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanSendRom));

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
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanSendRom));

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
        }

        private void SetAvailableFlashcarts()
        {
            if (_setAvailableFlashcarts)
            {
                return;
            }

            _setAvailableFlashcarts = true;

            AvailableFlashcarts.Add((Everdrive)Workspace.Instance.ServiceProvider.GetService(typeof(Everdrive))!);

            if (object.ReferenceEquals(null, CurrentFlashcart))
            {
                CurrentFlashcart = AvailableFlashcarts.Last();
            }
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
                    _deviceManager!.SendRom(SelectedRom);

                    // need to delay after loading rom or everdrive gets cranky
                    Task.Run(() =>
                    {
                        System.Threading.Thread.Sleep(5000);
                        _logger.Log(LogLevel.Information, $"Send ping");
                        _deviceManager.EnqueueMessage(new RomMetaMessage(Unfloader.Message.MessageType.GebugCmdMeta.Ping) { Source = CommunicationSource.Pc });
                    });

                    _dispatcher.BeginInvoke(() =>
                    {
                        _currentlySendingRom = false;
                        OnPropertyChanged(nameof(CanSendRom));
                    });
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

            // Show save file dialog box
            bool? result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result == true && !string.IsNullOrEmpty(dialog.FileName))
            {
                SelectedRom = dialog.FileName;
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

        private void ThreadMain()
        {
            while (_shutdown == false)
            {
                if (object.ReferenceEquals(null, _deviceManager))
                {
                    System.Threading.Thread.Sleep(500);
                    continue;
                }

                while (!_deviceManager.MessagesFromConsole.IsEmpty)
                {
                    _dispatcher.BeginInvoke(() =>
                    {
                        IGebugMessage? msg = null;

                        while (!_deviceManager.MessagesFromConsole.TryDequeue(out msg))
                        {
                            if (_deviceManager.MessagesFromConsole.IsEmpty)
                            {
                                return;
                            }
                        }

                        if (!object.ReferenceEquals(null, _lastMessageReceived))
                        {
                            _lastMessageReceived.Stop();
                        }

                        _lastMessageReceived = Stopwatch.StartNew();

                        if (msg is RomMessage)
                        {
                            _connectionLevel = ConnectionLevel.Rom;
                            OnPropertyChanged(nameof(CanSendRom));

                            _logger.Log(LogLevel.Information, ((RomMessage)msg).ToString());
                        }
                        else
                        {
                            _logger.Log(LogLevel.Information, msg.GetUsbPacket().ToString());
                        }

                        if (_shutdown)
                        {
                            return;
                        }
                    });

                    if (_shutdown)
                    {
                        break;
                    }
                }

                if (_connectionLevel == ConnectionLevel.Rom
                    && !object.ReferenceEquals(null, _lastMessageReceived)
                    && _lastMessageReceived.Elapsed.TotalSeconds > PingIntervalSec)
                {
                    if (object.ReferenceEquals(null, _lastMessageSent))
                    {
                        _lastMessageSent = Stopwatch.StartNew();
                    }

                    if (_lastMessageSent.Elapsed.TotalSeconds > PingIntervalSec)
                    {
                        _logger.Log(LogLevel.Information, $"Send ping");
                        _deviceManager.EnqueueMessage(new RomMetaMessage(Unfloader.Message.MessageType.GebugCmdMeta.Ping) { Source = CommunicationSource.Pc });

                        _lastMessageSent.Stop();
                        _lastMessageSent = Stopwatch.StartNew();
                    }
                }

                if (_connectionLevel == ConnectionLevel.Rom
                    && !object.ReferenceEquals(null, _lastMessageReceived)
                    && _lastMessageReceived.Elapsed.TotalSeconds > TimeoutDisconnectSec)
                {
                    _dispatcher.BeginInvoke(() => Task.Run(() => Disconnect()));
                }

                System.Threading.Thread.Sleep(5);
            }

            // shutdown notification received.
            if (!object.ReferenceEquals(null, _deviceManager))
            {
                _deviceManager.Stop();
            }
        }
    }
}
