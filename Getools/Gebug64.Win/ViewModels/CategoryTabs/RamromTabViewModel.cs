﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree.Xpath;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Win.Mvvm;
using Gebug64.Win.QueryTask;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Gebug64.Win.ViewModels.CategoryTabs
{
    /// <summary>
    /// Viewmodel for main window tab for ramrom commands.
    /// </summary>
    public class RamromTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Ramrom";

        private readonly MainWindowViewModel _mainWindowViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="RamromTabViewModel"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Service provider.</param>
        /// <param name="mainWindowViewModel">Parent viewmodel.</param>
        public RamromTabViewModel(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(_tabName, logger, connectionServiceProviderResolver)
        {
            DisplayOrder = 10;

            StartDemoReplayFromPcCommand = new CommandHandler(StartDemoReplayFromPcHandler, () => CanStartDemoReplayFromPc);
            ReplayNativeCommand = new CommandHandler(ReplayNativeHandler, () => CanReplayNative);
            ReplayFromExpansionPakCommand = new CommandHandler(ReplayFromExpansionPakHandler, () => CanReplayFromExpansionPak);

            _mainWindowViewModel = (MainWindowViewModel)Workspace.Instance.ServiceProvider.GetService(typeof(MainWindowViewModel))!;
        }

        /// <summary>
        /// Command to start demo playback by sending demo from PC.
        /// </summary>
        public ICommand StartDemoReplayFromPcCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="StartDemoReplayFromPcCommand"/> command can execute.
        /// </summary>
        public bool CanStartDemoReplayFromPc
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown && _mainWindowViewModel.ConnectionLevel == Enum.ConnectionLevel.Rom;
            }
        }

        /// <summary>
        /// Which replay demo to start.
        /// </summary>
        public byte ReplayNativeIndex { get; set; }

        /// <summary>
        /// Command to send <see cref="GebugRamromReplayNativeMessage"/>.
        /// </summary>
        public ICommand ReplayNativeCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="ReplayNativeCommand"/> can execute.
        /// </summary>
        public bool CanReplayNative
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown && _mainWindowViewModel.ConnectionLevel == Enum.ConnectionLevel.Rom;
            }
        }

        /// <summary>
        /// Command to send <see cref="GebugRamromReplayFromXpakMessage"/>.
        /// </summary>
        public ICommand ReplayFromExpansionPakCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="ReplayFromExpansionPakCommand"/> can execute.
        /// </summary>
        public bool CanReplayFromExpansionPak
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown
                    && _mainWindowViewModel.ConnectionLevel == Enum.ConnectionLevel.Rom
                    && _mainWindowViewModel.HasExpansionBack;
            }
        }

        private void StartDemoReplayFromPcHandler()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".bin", // Default file extension
                Filter = "BIN format | *.bin", // Filter files by extension
            };

            if (System.IO.Directory.Exists(_mainWindowViewModel.AppConfig.RecentPath.RamromPcReplayFolder))
            {
                dialog.InitialDirectory = _mainWindowViewModel.AppConfig.RecentPath.RamromPcReplayFolder;
            }

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true && !string.IsNullOrEmpty(dialog.FileName))
            {
                string filename = dialog.FileName;

                if (dialog.FileName != _mainWindowViewModel.AppConfig.RecentPath.RamromPcReplayFolder)
                {
                    _mainWindowViewModel.AppConfig.RecentPath.RamromPcReplayFolder = System.IO.Path.GetDirectoryName(dialog.FileName);
                    SaveAppSettings();
                }

                var context = new DemoReplayFromPcTask(_logger, _connectionServiceProviderResolver, filename);

                _mainWindowViewModel.RegisterBegin(context);
            }
        }

        private void ReplayNativeHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var msg = new GebugRamromReplayNativeMessage();

            msg.Index = ReplayNativeIndex;

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void ReplayFromExpansionPakHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".bin", // Default file extension
                Filter = "BIN format | *.bin", // Filter files by extension
            };

            if (System.IO.Directory.Exists(_mainWindowViewModel.AppConfig.RecentPath.RamromPcReplayFolder))
            {
                dialog.InitialDirectory = _mainWindowViewModel.AppConfig.RecentPath.RamromPcReplayFolder;
            }

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true && !string.IsNullOrEmpty(dialog.FileName))
            {
                string filename = dialog.FileName;

                if (dialog.FileName != _mainWindowViewModel.AppConfig.RecentPath.RamromPcReplayFolder)
                {
                    _mainWindowViewModel.AppConfig.RecentPath.RamromPcReplayFolder = System.IO.Path.GetDirectoryName(dialog.FileName);
                    SaveAppSettings();
                }

                var msg = new GebugRamromReplayFromXpakMessage();

                msg.Data = System.IO.File.ReadAllBytes(filename);

                _logger.Log(LogLevel.Information, $"Read ramrom file: {filename}");
                _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

                connectionServiceProvider.SendMessage(msg);
            }
        }
    }
}
