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
    /// <summary>
    /// Abstract base class for view model tabs.
    /// </summary>
    public abstract class TabViewModelBase : ViewModelBase
    {
        private readonly string _tabName;

        /// <summary>
        /// Logger.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// Connection service provider.
        /// </summary>
        protected readonly IConnectionServiceProviderResolver _connectionServiceProviderResolver;

        /// <summary>
        /// Current dispatcher, as set when instantiated.
        /// </summary>
        protected readonly Dispatcher _dispatcher;

        /// <summary>
        /// Protected flag to disable saving app settings.
        /// </summary>
        protected bool _ignoreAppSettingsChange = false;

        /// <summary>
        /// Current app settings.
        /// </summary>
        protected AppConfigViewModel _appConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabViewModelBase"/> class.
        /// </summary>
        /// <param name="tabName">Name of tab.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public TabViewModelBase(
            string tabName,
            ILogger logger,
            IConnectionServiceProviderResolver connectionServiceProviderResolver)
        {
            _tabName = tabName;

            _logger = logger;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _connectionServiceProviderResolver = connectionServiceProviderResolver;

            _appConfig = (AppConfigViewModel)Workspace.Instance.ServiceProvider.GetService(typeof(AppConfigViewModel))!;
        }

        /// <summary>
        /// Gets the name of tab.
        /// </summary>
        public string TabName => _tabName;

        /// <summary>
        /// Tab sort order.
        /// </summary>
        public int DisplayOrder { get; set; }

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
