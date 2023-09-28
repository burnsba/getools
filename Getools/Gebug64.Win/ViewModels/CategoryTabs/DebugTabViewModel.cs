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
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown;
            }
        }

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

        public DebugTabViewModel(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(_tabName, logger, connectionServiceProviderResolver)
        {
            SetDebugMenuOpenOnCommand = new CommandHandler(SetDebugMenuOpenOnCommandHandler, () => CanSetDebugMenuOpenOnCommand);
            SetDebugMenuOpenOffCommand = new CommandHandler(SetDebugMenuOpenOffCommandHandler, () => CanSetDebugMenuOpenOffCommand);
            DebugMenuCommand = new CommandHandler(DebugMenuCommandHandler, () => CanDebugMenuCommand);

            DisplayOrder = 25;

            DebugMenuCommandX.ValidCommands.ForEach(x => MenuItems.Add(x));
        }

        public void SetDebugMenuOpenOnCommandHandler()
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

        public void SetDebugMenuOpenOffCommandHandler()
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

        public void DebugMenuCommandHandler()
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
