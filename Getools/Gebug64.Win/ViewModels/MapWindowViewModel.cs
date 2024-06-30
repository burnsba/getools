using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Gebug64.Unfloader.Manage;
using Gebug64.Win.Controls;
using Gebug64.Win.Mvvm;
using Gebug64.Win.ViewModels.Config;
using Gebug64.Win.Windows.Mdi;
using Getools.Utility.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Gebug64.Win.ViewModels
{
    public class MapWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IConnectionServiceProviderResolver _connectionServiceResolver;
        private readonly Dispatcher _dispatcher;

        private string? _setupBinFolder;
        private string? _stanBinFolder;
        private string? _bgBinFolder;

        /// <summary>
        /// Flag to disable saving app settings. Used during startup.
        /// </summary>
        private bool _ignoreAppSettingsChange = false;

        /// <summary>
        /// Current app settings.
        /// </summary>
        protected AppConfigViewModel _appConfig;

        /// <summary>
        /// Folder containing setup files.
        /// </summary>
        public string SetupBinFolder
        {
            get => _setupBinFolder;
            set
            {
                if (_setupBinFolder == value)
                {
                    return;
                }

                _setupBinFolder = value;
                OnPropertyChanged(nameof(SetupBinFolder));

                _appConfig.Map.SetupBinFolder = value;

                SaveAppSettings();
            }
        }

        /// <summary>
        /// Folder containing stan files.
        /// </summary>
        public string StanBinFolder
        {
            get => _stanBinFolder;
            set
            {
                if (_stanBinFolder == value)
                {
                    return;
                }

                _stanBinFolder = value;
                OnPropertyChanged(nameof(StanBinFolder));

                _appConfig.Map.StanBinFolder = value;

                SaveAppSettings();
            }
        }

        /// <summary>
        /// Folder containing stan files.
        /// </summary>
        public string BgBinFolder
        {
            get => _bgBinFolder;
            set
            {
                if (_bgBinFolder == value)
                {
                    return;
                }

                _bgBinFolder = value;
                OnPropertyChanged(nameof(BgBinFolder));

                _appConfig.Map.BgBinFolder = value;

                SaveAppSettings();
            }
        }

        /// <summary>
        /// Command to set the <see cref="SetupBinFolder"/>.
        /// </summary>
        public ICommand SetSetupBinFolderCommand { get; set; }

        /// <summary>
        /// Command to set the <see cref="StanBinFolder"/>.
        /// </summary>
        public ICommand SetStanBinFolderCommand { get; set; }

        /// <summary>
        /// Command to set the <see cref="BgBinFolder"/>.
        /// </summary>
        public ICommand SetBgBinFolderCommand { get; set; }

        public MapWindowViewModel(ILogger logger, IConnectionServiceProviderResolver deviceManagerResolver, AppConfigViewModel appConfig)
        {
            _ignoreAppSettingsChange = true;

            _logger = logger;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _connectionServiceResolver = deviceManagerResolver;
            _appConfig = appConfig;

            SetSetupBinFolderCommand = new CommandHandler(
                () => Workspace.Instance.SetDirectoryCommandHandler(
                    this,
                    nameof(SetupBinFolder),
                    () => System.IO.Path.GetDirectoryName(System.AppContext.BaseDirectory)!),
                () => true);

            SetStanBinFolderCommand = new CommandHandler(
                () => Workspace.Instance.SetDirectoryCommandHandler(
                    this,
                    nameof(StanBinFolder),
                    () => System.IO.Path.GetDirectoryName(System.AppContext.BaseDirectory)!),
                () => true);

            SetBgBinFolderCommand = new CommandHandler(
                () => Workspace.Instance.SetDirectoryCommandHandler(
                    this,
                    nameof(BgBinFolder),
                    () => System.IO.Path.GetDirectoryName(System.AppContext.BaseDirectory)!),
                () => true);

            _setupBinFolder = _appConfig.Map.SetupBinFolder;
            _stanBinFolder = _appConfig.Map.StanBinFolder;
            _bgBinFolder = _appConfig.Map.BgBinFolder;

            _ignoreAppSettingsChange = false;
        }

        /// <summary>
        /// Pass through to <see cref="Workspace.Instance.SaveAppSettings"/>.
        /// </summary>
        protected void SaveAppSettings()
        {
            if (_ignoreAppSettingsChange)
            {
                return;
            }

            Workspace.Instance.SaveAppSettings();

            _appConfig.ClearIsDirty();
        }
    }
}
