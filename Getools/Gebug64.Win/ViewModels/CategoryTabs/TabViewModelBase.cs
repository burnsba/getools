using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Gebug64.Unfloader;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.CategoryTabs
{
    public abstract class TabViewModelBase
    {
        private readonly string _tabName;

        protected readonly ILogger _logger;
        protected IDeviceManager? _deviceManager;
        protected readonly IDeviceManagerResolver _deviceManagerResolver;
        protected readonly Dispatcher _dispatcher;

        public string TabName => _tabName;

        public int DisplayOrder { get; set; }

        public TabViewModelBase(string tabName, ILogger logger, IDeviceManagerResolver deviceManagerResolver)
        {
            _tabName = tabName;

            _logger = logger;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _deviceManagerResolver = deviceManagerResolver;
        }
    }
}
