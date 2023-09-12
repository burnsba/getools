using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Message;
using Gebug64.Win.Mvvm;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.CategoryTabs
{
    public class MiscTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Misc";

        public ICommand OsTimeCommand { get; set; }

        public bool CanSendOsTimeCommand
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

        public MiscTabViewModel(ILogger logger, IDeviceManagerResolver deviceManagerResolver)
            : base(_tabName, logger, deviceManagerResolver)
        {
            OsTimeCommand = new CommandHandler(OsTimeCommandHandler, () => CanSendOsTimeCommand);

            DisplayOrder = 95;
        }

        public void OsTimeCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var msg = new RomMiscMessage(Unfloader.Message.MessageType.GebugCmdMisc.OsTime) { Source = CommunicationSource.Pc };
            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }
    }
}
