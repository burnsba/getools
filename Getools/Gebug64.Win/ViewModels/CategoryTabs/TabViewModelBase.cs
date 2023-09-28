using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Antlr4.Runtime.Atn;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Win.Config;
using Gebug64.Win.Mvvm;
using Gebug64.Win.ViewModels.Config;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.CategoryTabs
{
    public abstract class TabViewModelBase : ViewModelBase
    {
        private readonly string _tabName;

        protected readonly ILogger _logger;
        protected IConnectionServiceProvider? _connectionServiceProvider;
        protected readonly IConnectionServiceProviderResolver _connectionServiceProviderResolver;
        protected readonly Dispatcher _dispatcher;
        protected bool _ignoreAppSettingsChange = false;
        protected AppConfigViewModel _appConfig;

        public string TabName => _tabName;

        public int DisplayOrder { get; set; }

        public TabViewModelBase(string tabName, ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
        {
            _tabName = tabName;

            _logger = logger;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _connectionServiceProviderResolver = connectionServiceProviderResolver;

            _appConfig = (AppConfigViewModel)Workspace.Instance.ServiceProvider.GetService(typeof(AppConfigViewModel));
        }

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
