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
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Win.Mvvm;
using Getools.Lib.Game.EnumModel;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.CategoryTabs
{
    public class DebugTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Debug";

        public ICommand SetDebugMenuOpenOnCommand { get; set; }
        public ICommand SetDebugMenuOpenOffCommand { get; set; }

        // Choose line mode by default
        public DebugMenuCommandX SelectedMenuItem { get; set; } = DebugMenuCommandX.VisCvg;

        public ObservableCollection<DebugMenuCommandX> MenuItems { get; set; } = new ObservableCollection<DebugMenuCommandX>();

        public ICommand DebugMenuCommand { get; set; }

        public bool CanSetDebugMenuOpenOnCommand
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

        public bool CanSetDebugMenuOpenOffCommand
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

        public bool CanDebugMenuCommand
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

        public DebugTabViewModel(ILogger logger, IDeviceManagerResolver deviceManagerResolver)
            : base(_tabName, logger, deviceManagerResolver)
        {
            SetDebugMenuOpenOnCommand = new CommandHandler(SetDebugMenuOpenOnCommandHandler, () => CanSetDebugMenuOpenOnCommand);
            SetDebugMenuOpenOffCommand = new CommandHandler(SetDebugMenuOpenOffCommandHandler, () => CanSetDebugMenuOpenOffCommand);
            DebugMenuCommand = new CommandHandler(DebugMenuCommandHandler, () => CanDebugMenuCommand);

            DisplayOrder = 25;

            DebugMenuCommandX.ValidCommands.ForEach(x => MenuItems.Add(x));
        }

        public void SetDebugMenuOpenOnCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var value = new U8Parameter(1);

            var msg = new RomDebugMessage(GebugCmdDebug.ShowDebugMenu) { Source = CommunicationSource.Pc };
            msg.Parameters.Add(value);

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }

        public void SetDebugMenuOpenOffCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var value = new U8Parameter(0);

            var msg = new RomDebugMessage(GebugCmdDebug.ShowDebugMenu) { Source = CommunicationSource.Pc };
            msg.Parameters.Add(value);

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }

        public void DebugMenuCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var value = new U8Parameter((int)SelectedMenuItem.Id);

            var msg = new RomDebugMessage(GebugCmdDebug.DebugMenuProcessor) { Source = CommunicationSource.Pc };
            msg.Parameters.Add(value);

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }
    }
}
