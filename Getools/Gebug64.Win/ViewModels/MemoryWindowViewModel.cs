using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Win.Converters;
using Gebug64.Win.Mvvm;
using Gebug64.Win.ViewModels.Config;
using Gebug64.Win.ViewModels.Game;
using Gebug64.Win.ViewModels.Map;
using Getools.Lib.Compiler.Map;
using Getools.Lib.Game.Asset.Stan;
using Getools.Lib.Game.EnumModel;
using Getools.Lib.Game.Enums;
using Getools.Utility.Logging;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// View model for managing memory and memory watches.
    /// </summary>
    public class MemoryWindowViewModel : ViewModelBase
    {
        private const int MaxAllowedMemoryWatch = 10; // console limit
        private const UInt32 MinValidAddress = 0x80024470;
        private const UInt32 MaxValidAddress = 0x80400000;

        private readonly ILogger _logger;
        private readonly IConnectionServiceProviderResolver _connectionServiceResolver;
        private readonly Dispatcher _dispatcher;
        private readonly MessageBus<IGebugMessage>? _appGebugMessageBus;
        private readonly Guid _memoryGebugMessageSubscription;

        private HashSet<byte> _activeMemoryWatchIds = new HashSet<byte>();
        private string? _mapBuildFile;
        private string? _addWatchSourceText;
        private string? _addWatchSizeText;
        private MapDetail _selectedWatchSource = new MapDetail();
        private string _selectedWatchSourceComboText = string.Empty;

        private MapDetailToStringConverter _mapDetailStringConverter = new MapDetailToStringConverter();

        /// <summary>
        /// Flag to disable saving app settings. Used during startup.
        /// </summary>
        private bool _ignoreAppSettingsChange = false;

        /// <summary>
        /// Current app settings.
        /// </summary>
        protected AppConfigViewModel _appConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryWindowViewModel"/> class.
        /// </summary>
        /// <param name="logger">App logger.</param>
        /// <param name="deviceManagerResolver">Device resolver.</param>
        /// <param name="appConfig">App config.</param>
        /// <param name="appGebugMessageBus">Message bus to listen for <see cref="IGebugMessage"/> messages from console.</param>
        public MemoryWindowViewModel(
            ILogger logger,
            IConnectionServiceProviderResolver deviceManagerResolver,
            AppConfigViewModel appConfig,
            MessageBus<IGebugMessage> appGebugMessageBus)
        {
            _ignoreAppSettingsChange = true;

            _logger = logger;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _connectionServiceResolver = deviceManagerResolver;
            _appConfig = appConfig;

            ActiveMemoryWatches = new ObservableCollection<MemoryWatchViewModel>();
            AvailableMapVariables = new ObservableCollection<MapDetail>();
            SelectedWatchSource = new MapDetail();

            SetMapBuildFileCommand = new CommandHandler(
                SetMapBuildFileCommandHandler,
                () => true);

            AddWatchCommand = new CommandHandler(AddWatchCommandHandler, () => CanAddWatch);
            RemoveWatchCommand = new CommandHandler(RemoveWatchCommandHandler, () => true);

            _mapBuildFile = _appConfig.Memory.MapBuildFile;

            if (System.IO.File.Exists(_mapBuildFile))
            {
                LoadMapVariableNames();
            }

            _ignoreAppSettingsChange = false;

            _appGebugMessageBus = appGebugMessageBus;
            _memoryGebugMessageSubscription = _appGebugMessageBus!.Subscribe(MessageBusMemoryGebugCallback);
        }

        /// <summary>
        /// Path to build output file giving memory locations of every ELF component.
        /// </summary>
        public string? MapBuildFile
        {
            get => _mapBuildFile;
            set
            {
                if (_mapBuildFile == value)
                {
                    return;
                }

                _mapBuildFile = value;
                OnPropertyChanged(nameof(MapBuildFile));

                _appConfig.Memory.MapBuildFile = value;

                SaveAppSettings();
            }
        }

        /// <summary>
        /// Number of bytes to read in memory watch. Entered by user.
        /// </summary>
        public string? AddWatchSizeText
        {
            get => _addWatchSizeText;
            set
            {
                if (_addWatchSizeText == value)
                {
                    return;
                }

                _addWatchSizeText = value;
                OnPropertyChanged(nameof(AddWatchSizeText));
            }
        }

        /// <summary>
        /// List of current memory watches.
        /// </summary>
        public ObservableCollection<MemoryWatchViewModel> ActiveMemoryWatches { get; set; }

        /// <summary>
        /// Command check to see if user can add memory watch.
        /// </summary>
        private bool CanAddWatch
        {
            get
            {
                if (ActiveMemoryWatches.Count >= MaxAllowedMemoryWatch)
                {
                    return false;
                }

                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown;
            }
        }

        /// <summary>
        /// List of available/known variables.
        /// This should be loaded from the compiler map file.
        /// </summary>
        public ObservableCollection<MapDetail> AvailableMapVariables { get; set; }

        /// <summary>
        /// Currently selected compiler map value.
        /// If the user entered a memory address that doesn't map to a known variable
        /// then a new instance will be created.
        /// Otherwise this is an item from <see cref="AvailableMapVariables"/>.
        /// </summary>
        public MapDetail SelectedWatchSource
        {
            get => _selectedWatchSource;
            set
            {
                if (_selectedWatchSource == value || object.ReferenceEquals(null, value))
                {
                    return;
                }

                _selectedWatchSource = value;
                OnPropertyChanged(nameof(SelectedWatchSource));
            }
        }

        /// <summary>
        /// Bound to the text input of the combo box for the user to type/select
        /// a variable or memory address.
        /// When this value changes, the method <see cref="TrySetSelectedWatchSourceFromText"/> is called.
        /// </summary>
        public string SelectedWatchSourceComboText
        {
            get => _selectedWatchSourceComboText;
            set
            {
                if (_selectedWatchSourceComboText == value)
                {
                    return;
                }

                _selectedWatchSourceComboText = value;

                TrySetSelectedWatchSourceFromText();
            }
        }

        /// <summary>
        /// Command to set the <see cref="MapBuildFile"/>.
        /// </summary>
        public ICommand SetMapBuildFileCommand { get; set; }

        /// <summary>
        /// Command to add a new memory watch.
        /// </summary>
        public ICommand AddWatchCommand { get; set; }

        /// <summary>
        /// Command to remove a memory watch. Called by child element.
        /// </summary>
        public ICommand RemoveWatchCommand { get; set; }

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
        /// Callback to monitor incoming messages from the console.
        /// </summary>
        /// <param name="msg">Message.</param>
        private void MessageBusMemoryGebugCallback(IGebugMessage msg)
        {
            if (msg.Category == GebugMessageCategory.Memory)
            {
                if (msg.Command == (int)GebugCmdMemory.WatchBulkRead)
                {
                    var bulkWatchMsg = (GebugMemoryWatchBulkRead)msg;

                    foreach (var watch in bulkWatchMsg.WatchResults)
                    {
                        var vmwatch = ActiveMemoryWatches.FirstOrDefault(x => x.Id == watch.Id);

                        if (!object.ReferenceEquals(null, vmwatch))
                        {
                            vmwatch.UpdateFromConsole(watch);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes a memory watch. Called by child element.
        /// </summary>
        /// <param name="arg">Must be <see cref="MemoryWatchViewModel"/>.</param>
        private void RemoveWatchCommandHandler(object? arg)
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            if (arg == null)
            {
                return;
            }

            MemoryWatchViewModel mwvm = (MemoryWatchViewModel)arg;

            if (mwvm == null)
            {
                return;
            }

            ActiveMemoryWatches.Remove(mwvm);

            OnPropertyChanged(nameof(CanAddWatch));

            var msg = new GebugMemoryRemoveWatch()
            {
                Id = (byte)mwvm.Id,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);

            _activeMemoryWatchIds.Remove((byte)mwvm.Id);
        }

        /// <summary>
        /// Helper method to attempt to resolve <see cref="SelectedWatchSourceComboText"/>.
        /// Checks if this is a selected item from <see cref="AvailableMapVariables"/>; if so, sets <see cref="SelectedWatchSource"/>.
        /// Otherwise, compares the text to known variable names and known addresses <see cref="AvailableMapVariables"/>
        /// and sets <see cref="SelectedWatchSource"/>.
        /// If still can't resolve to a known item, and addresses is within range,
        /// creates a new item and sets <see cref="SelectedWatchSource"/>.
        /// Otherwise logs an error message.
        /// </summary>
        private void TrySetSelectedWatchSourceFromText()
        {
            var mapDetail = (MapDetail)_mapDetailStringConverter.ConvertBack(
                _selectedWatchSourceComboText,
                typeof(MapDetail),
                new object(), // unused
                System.Globalization.CultureInfo.CurrentCulture);

            var mapDetailFromMap = AvailableMapVariables.FirstOrDefault(x =>
                x.Address == mapDetail.Address
                || string.Compare(mapDetail.Name, x.Name, StringComparison.InvariantCultureIgnoreCase) == 0);

            if (!object.ReferenceEquals(null, mapDetailFromMap))
            {
                _selectedWatchSource = mapDetailFromMap;
                OnPropertyChanged(nameof(SelectedWatchSource));
            }
            else if (mapDetail.Address > 0)
            {
                // Otherwise, create a new entry. Make the friendly name the address.
                _selectedWatchSource = mapDetail;

                OnPropertyChanged(nameof(SelectedWatchSource));
            }
            else
            {
                _logger.Log(LogLevel.Information, "Could not resolve memory watch to known variable or parse as address.");

                _selectedWatchSource = new MapDetail()
                {
                    Address = 0,
                    Name = _selectedWatchSourceComboText,
                };

                OnPropertyChanged(nameof(SelectedWatchSource));
            }
        }

        private void AddWatchCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            MapDetail workingMapDetail = SelectedWatchSource;

            if (object.ReferenceEquals(null, workingMapDetail))
            {
                _logger.Log(LogLevel.Information, "Selected memory watch source not set.");
                return;
            }

            int size;
            int id = GetNextAvailableMemoryWatchId();
            UInt32 address = workingMapDetail.Address;

            if (!int.TryParse(AddWatchSizeText, out size))
            {
                _logger.Log(LogLevel.Information, "Invalid memory watch size.");
                return;
            }

            if (address == 0 || address < MinValidAddress || address > MaxValidAddress)
            {
                _logger.Log(LogLevel.Information, "Memory watch address out of range.");
                return;
            }

            if (ActiveMemoryWatches.Any(x => x.MemoryAddress == address && x.Size == size))
            {
                _logger.Log(LogLevel.Information, "Memory watch already added for address+size.");
                return;
            }

            var mwmv = new MemoryWatchViewModel()
            {
                Id = id,
                MemoryAddress = address,
                FriendlyAddress = workingMapDetail.Name,
                DisplayFormat = Enum.MemoryDisplayFormat.Decimal,
                Size = size,
            };

            if (size == 1)
            {
                mwmv.DataType = Enum.MemoryDataType.S8;
            }
            else if (size == 2)
            {
                mwmv.DataType = Enum.MemoryDataType.S16;
            }
            else if (size == 4)
            {
                mwmv.DataType = Enum.MemoryDataType.S32;
            }
            else
            {
                mwmv.DataType = Enum.MemoryDataType.Array;
            }

            ActiveMemoryWatches.Add(mwmv);

            OnPropertyChanged(nameof(CanAddWatch));

            var msg = new GebugMemoryAddWatch()
            {
                Id = (byte)mwmv.Id,
                Size = (byte)mwmv.Size,
                Address = mwmv.MemoryAddress,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);

            _activeMemoryWatchIds.Add((byte)mwmv.Id);
        }

        private UInt32 ResolveSourceAddres(string? source)
        {
            UInt32 address = 0;

            if (string.IsNullOrEmpty(source))
            {
                return 0;
            }

            var lower = source.ToLower();

            if (lower.StartsWith("0x"))
            {
                try
                {
                    int intval = (int)new System.ComponentModel.Int32Converter()!.ConvertFromString(source!)!;
                    address = (UInt32)intval;
                }
                catch
                {
                }
            }
            else if (lower.Length == 8)
            {
                try
                {
                    int intval = (int)new System.ComponentModel.Int32Converter()!.ConvertFromString("0x" + source!)!;
                    address = (UInt32)intval;
                }
                catch
                {
                }
            }

            if (address < MinValidAddress || address > MaxValidAddress)
            {
                return 0;
            }

            return address;
        }

        private byte GetNextAvailableMemoryWatchId()
        {
            byte val = 1;

            while (_activeMemoryWatchIds.Contains(val))
            {
                val++;
            }

            return val;
        }

        private void SetMapBuildFileCommandHandler()
        {
            var startFile = _mapBuildFile;

            Workspace.Instance.SetFileCommandHandler(
                this,
                nameof(MapBuildFile),
                () => System.IO.Path.GetDirectoryName(System.AppContext.BaseDirectory)!);

            if (startFile != _mapBuildFile && System.IO.File.Exists(_mapBuildFile))
            {
                LoadMapVariableNames();
            }
        }

        private void LoadMapVariableNames()
        {
            var parser = new IdoMapParser();
            var parseResults = parser.ParseMapFile(_mapBuildFile!, LoadMapSegmentFilter, LoadMapAddressFilter);

            AvailableMapVariables.Clear();
            foreach (var p in parseResults)
            {
                AvailableMapVariables.Add(p);
            }
        }

        private bool LoadMapSegmentFilter(string text)
        {
            if (text.Contains("data") || text.Contains("bss"))
            {
                return true;
            }

            return false;
        }

        private bool LoadMapAddressFilter(UInt32 address)
        {
            return address >= MinValidAddress && address <= MaxValidAddress;
        }
    }
}
