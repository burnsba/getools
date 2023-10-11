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
    /// <summary>
    /// View model for "stage" tab.
    /// </summary>
    public class StageTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Stage";

        /// <summary>
        /// Initializes a new instance of the <see cref="StageTabViewModel"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public StageTabViewModel(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(_tabName, logger, connectionServiceProviderResolver)
        {
            SetStageCommand = new CommandHandler(SetStageCommandHandler, () => CanSetStage);

            DisplayOrder = 55;

            LevelIdX.SinglePlayerStages.ForEach(x => Stages.Add(x));
        }

        /// <summary>
        /// Send "set stage" command.
        /// </summary>
        public ICommand SetStageCommand { get; set; }

        /// <summary>
        /// Currently selected stage.
        /// </summary>
        public LevelIdX SelectedStage { get; set; } = LevelIdX.Dam;

        /// <summary>
        /// List of available stages.
        /// </summary>
        public ObservableCollection<LevelIdX> Stages { get; set; } = new ObservableCollection<LevelIdX>();

        private bool CanSetStage
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

        private void SetStageCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugStageSetStageMessage()
            {
                LevelId = (byte)SelectedStage.Id,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }
    }
}
