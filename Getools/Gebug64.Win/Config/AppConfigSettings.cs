using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Gebug64.Win.Config
{
    public class AppConfigSettings
    {
        public const string DefaultFilename = "appsettings.json";

        public DeviceSectionSettings Device { get; set; }

        public ConnectionSectionSettings Connection { get; set; }

        public List<string> RecentSendRom { get; set; } = new List<string>();

        public AppConfigSettings(IConfiguration config)
        {
            config.GetSection(nameof(AppConfigSettings)).Bind(this);
        }

        private AppConfigSettings()
        { }

        public static AppConfigSettings GetDefault()
        {
            return new AppConfigSettings();
        }

        public void Save(string path)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }

    public class DeviceSectionSettings
    {
        public string? Flashcart { get; set; }
    }

    public class ConnectionSectionSettings
    {
        public string? SerialPort { get; set; }
    }
}
