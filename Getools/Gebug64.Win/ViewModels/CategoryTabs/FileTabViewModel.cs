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
    /// View model for "cheat" tab.
    /// </summary>
    public class FileTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "File";

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTabViewModel"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public FileTabViewModel(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(_tabName, logger, connectionServiceProviderResolver)
        {
            UnlockStageDifficultyCommand = new CommandHandler(UnlockStageDifficultyCommandHandler, () => CanUnlockStageDifficulty);

            DisplayOrder = 75;

            LevelSoloSequenceX.SinglePlayerStages.ForEach(x => AvailableStages.Add(x));
            DifficultyX.UnlockFileDifficulty.ForEach(x => AvailableDifficulties.Add(x));
        }

        public LevelSoloSequenceX SelectedStage { get; set; } = LevelSoloSequenceX.Dam;

        public ObservableCollection<LevelSoloSequenceX> AvailableStages { get; set; } = new ObservableCollection<LevelSoloSequenceX>();

        public DifficultyX SelectedDifficulty { get; set; } = DifficultyX.Agent;

        public ObservableCollection<DifficultyX> AvailableDifficulties { get; set; } = new ObservableCollection<DifficultyX>();

        /// <summary>
        /// Command to turn a "runtime" cheat on.
        /// </summary>
        public ICommand UnlockStageDifficultyCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="UnlockStageDifficultyCommand"/> can execute.
        /// </summary>
        public bool CanUnlockStageDifficulty
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

        private void UnlockStageDifficultyCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugFileUnlockStageDifficultyMessage()
            {
                Stage = (byte)SelectedStage.Id,
                Difficulty = (byte)SelectedDifficulty.Id,
            };

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }
    }
}
