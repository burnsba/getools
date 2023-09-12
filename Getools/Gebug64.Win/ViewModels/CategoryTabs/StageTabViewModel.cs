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
    public class StageTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Stage";

        public ICommand SetStageCommand { get; set; }

        public LevelIdX SelectedStage { get; set; } = LevelIdX.Dam;

        public ObservableCollection<LevelIdX> Stages { get; set; } = new ObservableCollection<LevelIdX>();

        public bool CanSetStage
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

        public StageTabViewModel(ILogger logger, IDeviceManagerResolver deviceManagerResolver)
            : base(_tabName, logger, deviceManagerResolver)
        {
            SetStageCommand = new CommandHandler(SetStageCommandHandler, () => CanSetStage);

            DisplayOrder = 55;

            LevelIdX.SinglePlayerStages.ForEach(x => Stages.Add(x));
        }

        public void SetStageCommandHandler()
        {
            IDeviceManager? deviceManager = _deviceManagerResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, deviceManager))
            {
                return;
            }

            var value = new U8Parameter(SelectedStage.Id);

            var msg = new RomStageMessage(Unfloader.Message.MessageType.GebugCmdStage.SetStage) { Source = CommunicationSource.Pc };
            msg.Parameters.Add(value);

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            deviceManager.EnqueueMessage(msg);
        }
    }
}
