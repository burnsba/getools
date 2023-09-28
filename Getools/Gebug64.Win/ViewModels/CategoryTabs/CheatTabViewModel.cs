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
    public class CheatTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Cheat";

        // Choose dk mode by default
        public CheatIdX SelectedCheatStatusItem { get; set; } = CheatIdX.DkMode;

        public ObservableCollection<CheatIdX> RuntimeCheats { get; set; } = new ObservableCollection<CheatIdX>();

        public ICommand SetCheatStatusOnCommand { get; set; }
        public ICommand SetCheatStatusOffCommand { get; set; }

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

        // Choose invisibility by default
        public CheatIdX SelectedUnlockRuntimeCheat { get; set; } = CheatIdX.UnlockInvis;

        public ObservableCollection<CheatIdX> UnlockRuntimeCheats { get; set; } = new ObservableCollection<CheatIdX>();

        public ICommand UnlockRuntimeCheatCommand { get; set; }

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

        // Choose facility by default
        public CheatIdX SelectedUnlockStageCheat { get; set; } = CheatIdX.UnlockFacility;

        public ObservableCollection<CheatIdX> UnlockStageCheats { get; set; } = new ObservableCollection<CheatIdX>();

        public ICommand UnlockStageCheatCommand { get; set; }

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

        public ICommand DisableAllCommand { get; set; }

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
