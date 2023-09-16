using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Message;
using Gebug64.Win.Mvvm;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.CategoryTabs
{
    public class MetaTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Meta";

        public ICommand PingCommand { get; set; }

        public ICommand VersionCommand { get; set; }

        public bool CanSendPingCommand
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

        public bool CanSendVersionCommand
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

        public MetaTabViewModel(ILogger logger, IDeviceManagerResolver deviceManagerResolver)
            : base(_tabName, logger, deviceManagerResolver)
        {
            PingCommand = new CommandHandler(PingCommandHandler, () => CanSendPingCommand);
            VersionCommand = new CommandHandler(VersionCommandHandler, () => CanSendVersionCommand);

            DisplayOrder = 90;
        }

        public void PingCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var msg = new RomMetaMessage(Unfloader.Message.MessageType.GebugCmdMeta.Ping) { Source = CommunicationSource.Pc };
            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }

        public void VersionCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var msg = new RomMetaMessage(Unfloader.Message.MessageType.GebugCmdMeta.Version) { Source = CommunicationSource.Pc };
            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }
    }
}
