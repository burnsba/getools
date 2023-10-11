using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Win.Mvvm;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.CategoryTabs
{
    /// <summary>
    /// View model for "meta" tab.
    /// </summary>
    public class MetaTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Meta";

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaTabViewModel"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public MetaTabViewModel(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(_tabName, logger, connectionServiceProviderResolver)
        {
            PingCommand = new CommandHandler(PingCommandHandler, () => CanSendPingCommand);
            VersionCommand = new CommandHandler(VersionCommandHandler, () => CanSendVersionCommand);

            DisplayOrder = 90;
        }

        /// <summary>
        /// Command to send "ping" message.
        /// </summary>
        public ICommand PingCommand { get; set; }

        /// <summary>
        /// Command to send "version" message.
        /// </summary>
        public ICommand VersionCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="PingCommand"/> can execute.
        /// </summary>
        public bool CanSendPingCommand
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
        /// Gets a value indicating whether <see cref="VersionCommand"/> can execute.
        /// </summary>
        public bool CanSendVersionCommand
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

        private void PingCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugMetaPingMessage();

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void VersionCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugMetaVersionMessage();

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }
    }
}
