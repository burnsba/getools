using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Gebug64.Win.Config
{
    public class AppConfigStartupResolver : IAppConfigSettingsFactory
    {
        private readonly IConfiguration _configuration;

        public AppConfigStartupResolver(IConfiguration config)
        {
            _configuration = config;
        }

        public AppConfigSettings GetAppConfigSettings()
        {
            return new AppConfigSettings(_configuration);
        }
    }
}
