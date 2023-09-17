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
    public class ViTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Vi";

        public ICommand GetFrameBufferCommand { get; set; }

        public bool CanSendGetFrameBufferCommand
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

        public ViTabViewModel(ILogger logger, IDeviceManagerResolver deviceManagerResolver)
            : base(_tabName, logger, deviceManagerResolver)
        {
            GetFrameBufferCommand = new CommandHandler(GetFrameBufferCommandHandler, () => CanSendGetFrameBufferCommand);

            DisplayOrder = 80;
        }

        public void GetFrameBufferCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var msg = new RomViMessage(Unfloader.Message.MessageType.GebugCmdVi.GrabFramebuffer) { Source = CommunicationSource.Pc };
            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }
    }
}
