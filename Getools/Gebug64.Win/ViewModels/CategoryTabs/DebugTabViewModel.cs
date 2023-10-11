using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
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
    /// View model for "debug" tab.
    /// </summary>
    public class DebugTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Debug";

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugTabViewModel"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public DebugTabViewModel(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(_tabName, logger, connectionServiceProviderResolver)
        {
            SetDebugMenuOpenOnCommand = new CommandHandler(SetDebugMenuOpenOnCommandHandler, () => CanSetDebugMenuOpenOnCommand);
            SetDebugMenuOpenOffCommand = new CommandHandler(SetDebugMenuOpenOffCommandHandler, () => CanSetDebugMenuOpenOffCommand);
            DebugMenuCommand = new CommandHandler(DebugMenuCommandHandler, () => CanDebugMenuCommand);

            DisplayOrder = 25;

            DebugMenuCommandX.ValidCommands.ForEach(x => MenuItems.Add(x));
        }

        /// <summary>
        /// Command to open debug menu (sets variable on console).
        /// </summary>
        public ICommand SetDebugMenuOpenOnCommand { get; set; }

        /// <summary>
        /// Command to close debug menu (clears variable on console).
        /// </summary>
        public ICommand SetDebugMenuOpenOffCommand { get; set; }

        /// <summary>
        /// Currently selected debug menu item. Choose "line mode" by default.
        /// </summary>
        public DebugMenuCommandX SelectedMenuItem { get; set; } = DebugMenuCommandX.VisCvg;

        /// <summary>
        /// List of available menu items.
        /// </summary>
        public ObservableCollection<DebugMenuCommandX> MenuItems { get; set; } = new ObservableCollection<DebugMenuCommandX>();

        /// <summary>
        /// Sends console message to execute selected debug item.
        /// </summary>
        public ICommand DebugMenuCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="SetDebugMenuOpenOnCommand"/> can execute.
        /// </summary>
        public bool CanSetDebugMenuOpenOnCommand
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
        /// Gets a value indicating whether <see cref="SetDebugMenuOpenOffCommand"/> can execute.
        /// </summary>
        public bool CanSetDebugMenuOpenOffCommand
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
        /// Gets a value indicating whether <see cref="DebugMenuCommand"/> can execute.
        /// </summary>
        public bool CanDebugMenuCommand
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

        private void SetDebugMenuOpenOnCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugDebugMenuOpenMessage()
            {
                Open = 1,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void SetDebugMenuOpenOffCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugDebugMenuOpenMessage()
            {
                Open = 0,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void DebugMenuCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugDebugMenuCommandMessage()
            {
                MenuCommand = (byte)SelectedMenuItem.Id,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }
    }
}
