using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Win.Mvvm;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.CategoryTabs
{
    /// <summary>
    /// View model for "misc" tab.
    /// </summary>
    public class MiscTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Misc";

        /// <summary>
        /// Initializes a new instance of the <see cref="MiscTabViewModel"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public MiscTabViewModel(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(_tabName, logger, connectionServiceProviderResolver)
        {
            OsTimeCommand = new CommandHandler(OsTimeCommandHandler, () => CanSendOsTimeCommand);
            OsMemSizeCommand = new CommandHandler(OsMemSizeCommandHandler, () => CanSendOsMemSizeCommand);
            GrenadeChance0Command = new CommandHandler(GrenadeChance0CommandHandler, () => CanSendGrenadeChance0Command);
            GrenadeChance100Command = new CommandHandler(GrenadeChance100CommandHandler, () => CanSendGrenadeChance100Command);
            GrenadeChanceDefaultCommand = new CommandHandler(GrenadeChanceDefaultCommandHandler, () => CanSendGrenadeChanceDefaultCommand);

            DisplayOrder = 95;
        }

        /// <summary>
        /// Command to send os time message.
        /// </summary>
        public ICommand OsTimeCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="OsTimeCommand"/> can execute.
        /// </summary>
        public bool CanSendOsTimeCommand
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
        /// Command to send <see cref="GebugMiscOsGetMemSizeMessage"/>.
        /// </summary>
        public ICommand OsMemSizeCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="OsMemSizeCommand"/> can execute.
        /// </summary>
        public bool CanSendOsMemSizeCommand
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
        /// Command to send <see cref="GebugMiscGrenadeChanceMessage"/>.
        /// </summary>
        public ICommand GrenadeChance0Command { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="GrenadeChance0Command"/> can execute.
        /// </summary>
        public bool CanSendGrenadeChance0Command
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
        /// Command to send <see cref="GebugMiscGrenadeChanceMessage"/>.
        /// </summary>
        public ICommand GrenadeChance100Command { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="GrenadeChance100Command"/> can execute.
        /// </summary>
        public bool CanSendGrenadeChance100Command
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
        /// Command to send <see cref="GebugMiscGrenadeChanceMessage"/>.
        /// </summary>
        public ICommand GrenadeChanceDefaultCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="GrenadeChanceDefaultCommand"/> can execute.
        /// </summary>
        public bool CanSendGrenadeChanceDefaultCommand
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

        private void OsTimeCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugMiscOsTimeMessage();

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void OsMemSizeCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugMiscOsGetMemSizeMessage();

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void GrenadeChance0CommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugMiscGrenadeChanceMessage()
            {
                Option = 1,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void GrenadeChance100CommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugMiscGrenadeChanceMessage()
            {
                Option = 2,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void GrenadeChanceDefaultCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugMiscGrenadeChanceMessage()
            {
                Option = 0,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }
    }
}
