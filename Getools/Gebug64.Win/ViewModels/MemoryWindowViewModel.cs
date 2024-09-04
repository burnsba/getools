using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Win.Mvvm;
using Gebug64.Win.ViewModels.Config;
using Gebug64.Win.ViewModels.Game;
using Gebug64.Win.ViewModels.Map;
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
        private readonly ILogger _logger;
        private readonly IConnectionServiceProviderResolver _connectionServiceResolver;
        private readonly Dispatcher _dispatcher;
        private readonly MessageBus<IGebugMessage>? _appGebugMessageBus;
        private readonly Guid _memoryGebugMessageSubscription;

        private string? _mapBuildFile;

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

            SetMapBuildFileCommand = new CommandHandler(
                () => Workspace.Instance.SetFileCommandHandler(
                    this,
                    nameof(MapBuildFile),
                    () => System.IO.Path.GetDirectoryName(System.AppContext.BaseDirectory)!),
                () => true);

            _mapBuildFile = _appConfig.Memory.MapBuildFile;

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
        /// Command to set the <see cref="MapBuildFile"/>.
        /// </summary>
        public ICommand SetMapBuildFileCommand { get; set; }

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
            if (msg.Category == GebugMessageCategory.Bond)
            {
                ///////
            }
        }
    }
}
