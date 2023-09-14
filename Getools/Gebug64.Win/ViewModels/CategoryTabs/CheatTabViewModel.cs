using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Message;
using Gebug64.Unfloader.Message.CommandParameter;
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
                IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, deviceManager))
                {
                    return false;
                }

                return !deviceManager.IsShutdown;
            }
        }

        public bool CanSetCheatStatusOff
        {
            get
            {
                IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, deviceManager))
                {
                    return false;
                }

                return !deviceManager.IsShutdown;
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
                IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, deviceManager))
                {
                    return false;
                }

                return !deviceManager.IsShutdown;
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
                IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, deviceManager))
                {
                    return false;
                }

                return !deviceManager.IsShutdown;
            }
        }

        public ICommand DisableAllCommand { get; set; }

        public bool CanDisableAll
        {
            get
            {
                IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, deviceManager))
                {
                    return false;
                }

                return !deviceManager.IsShutdown;
            }
        }

        public CheatTabViewModel(ILogger logger, IDeviceManagerResolver deviceManagerResolver)
            : base(_tabName, logger, deviceManagerResolver)
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
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var v1 = new U8Parameter(1);
            var v2 = new U8Parameter(SelectedCheatStatusItem.Id);

            var msg = new RomCheatMessage(Unfloader.Message.MessageType.GebugCmdCheat.SetCheatStatus) { Source = CommunicationSource.Pc };
            msg.Parameters.Add(v1);
            msg.Parameters.Add(v2);

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }

        private void SetCheatStatusOffCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var v1 = new U8Parameter(0);
            var v2 = new U8Parameter(SelectedCheatStatusItem.Id);

            var msg = new RomCheatMessage(Unfloader.Message.MessageType.GebugCmdCheat.SetCheatStatus) { Source = CommunicationSource.Pc };
            msg.Parameters.Add(v1);
            msg.Parameters.Add(v2);

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }

        private void UnlockRuntimeCheatCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var v1 = new U8Parameter(1);
            var v2 = new U8Parameter(SelectedUnlockRuntimeCheat.Id);

            var msg = new RomCheatMessage(Unfloader.Message.MessageType.GebugCmdCheat.SetCheatStatus) { Source = CommunicationSource.Pc };
            msg.Parameters.Add(v1);
            msg.Parameters.Add(v2);

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }

        private void UnlockStageCheatCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var v1 = new U8Parameter(1);
            var v2 = new U8Parameter(SelectedUnlockStageCheat.Id);

            var msg = new RomCheatMessage(Unfloader.Message.MessageType.GebugCmdCheat.SetCheatStatus) { Source = CommunicationSource.Pc };
            msg.Parameters.Add(v1);
            msg.Parameters.Add(v2);

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }
        
        private void DisableAllCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var msg = new RomCheatMessage(Unfloader.Message.MessageType.GebugCmdCheat.DisableAll) { Source = CommunicationSource.Pc };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }
    }
}
