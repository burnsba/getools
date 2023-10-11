using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Win.Mvvm;
using Getools.Lib.Game.EnumModel;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.CategoryTabs
{
    /// <summary>
    /// View model for "cheat" tab.
    /// </summary>
    public class CheatTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Cheat";

        /// <summary>
        /// Initializes a new instance of the <see cref="CheatTabViewModel"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public CheatTabViewModel(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(_tabName, logger, connectionServiceProviderResolver)
        {
            SetCheatStatusOnCommand = new CommandHandler(SetCheatStatusOnCommandHandler, () => CanSetCheatStatusOn);
            SetCheatStatusOffCommand = new CommandHandler(SetCheatStatusOffCommandHandler, () => CanSetCheatStatusOff);
            UnlockRuntimeCheatCommand = new CommandHandler(UnlockRuntimeCheatCommandHandler, () => CanUnlockRuntimeCheat);
            UnlockStageCheatCommand = new CommandHandler(UnlockStageCheatCommandHandler, () => CanUnlockStageCheat);
            DisableAllCommand = new CommandHandler(DisableAllCommandHandler, () => CanDisableAll);

            DisplayOrder = 35;

            CheatIdX.AllRuntime.ForEach(x => RuntimeCheats.Add(x));
            CheatIdX.AllUnlockRuntime.ForEach(x => UnlockRuntimeCheats.Add(x));
            CheatIdX.AllUnlockStage.ForEach(x => UnlockStageCheats.Add(x));
        }

        /// <summary>
        /// Gets or sets the currently selected "runtime" cheat to toggle. Choose dk mode by default.
        /// </summary>
        public CheatIdX SelectedCheatStatusItem { get; set; } = CheatIdX.DkMode;

        /// <summary>
        /// List of available runtime cheats.
        /// </summary>
        public ObservableCollection<CheatIdX> RuntimeCheats { get; set; } = new ObservableCollection<CheatIdX>();

        /// <summary>
        /// Command to turn a "runtime" cheat on.
        /// </summary>
        public ICommand SetCheatStatusOnCommand { get; set; }

        /// <summary>
        /// Command to turn a "runtime" cheat on.
        /// </summary>
        public ICommand SetCheatStatusOffCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="SetCheatStatusOnCommand"/> can execute.
        /// </summary>
        public bool CanSetCheatStatusOn
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown;
            }
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="SetCheatStatusOffCommand"/> can execute.
        /// </summary>
        public bool CanSetCheatStatusOff
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown;
            }
        }

        /// <summary>
        /// Currently selected "unlock runtime" cheat. Choose invisibility by default.
        /// </summary>
        public CheatIdX SelectedUnlockRuntimeCheat { get; set; } = CheatIdX.UnlockInvis;

        /// <summary>
        /// List of available "unlock runtime" cheats.
        /// </summary>
        public ObservableCollection<CheatIdX> UnlockRuntimeCheats { get; set; } = new ObservableCollection<CheatIdX>();

        /// <summary>
        /// Command to perform "unlock runtime".
        /// </summary>
        public ICommand UnlockRuntimeCheatCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="UnlockRuntimeCheatCommand"/> can execute.
        /// </summary>
        public bool CanUnlockRuntimeCheat
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown;
            }
        }

        /// <summary>
        /// Currently selected "unlock level" cheat. Choose facility by default.
        /// </summary>
        public CheatIdX SelectedUnlockStageCheat { get; set; } = CheatIdX.UnlockFacility;

        /// <summary>
        /// List of available "unlock level" cheats.
        /// </summary>
        public ObservableCollection<CheatIdX> UnlockStageCheats { get; set; } = new ObservableCollection<CheatIdX>();

        /// <summary>
        /// Command to perform "unlock level".
        /// </summary>
        public ICommand UnlockStageCheatCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="UnlockStageCheatCommand"/> can execute.
        /// </summary>
        public bool CanUnlockStageCheat
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown;
            }
        }

        /// <summary>
        /// Command to disable all cheats.
        /// </summary>
        public ICommand DisableAllCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="DisableAllCommand"/> can execute.
        /// </summary>
        public bool CanDisableAll
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown;
            }
        }

        private void SetCheatStatusOnCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugCheatSetCheatStatusMessage()
            {
                Enable = 1,
                CheatId = (byte)SelectedCheatStatusItem.Id,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void SetCheatStatusOffCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugCheatSetCheatStatusMessage()
            {
                Enable = 0,
                CheatId = (byte)SelectedCheatStatusItem.Id,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void UnlockRuntimeCheatCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugCheatSetCheatStatusMessage()
            {
                Enable = 1,
                CheatId = (byte)SelectedUnlockRuntimeCheat.Id,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void UnlockStageCheatCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugCheatSetCheatStatusMessage()
            {
                Enable = 1,
                CheatId = (byte)SelectedUnlockStageCheat.Id,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void DisableAllCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugCheatDisableAllMessage();

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }
    }
}
