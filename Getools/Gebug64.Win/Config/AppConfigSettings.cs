using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Gebug64.Win.Config
{
    /// <summary>
    /// Runtime app settings.
    /// </summary>
    public class AppConfigSettings
    {
        private readonly string _currentLocation;

        /// <summary>
        /// Default filename of appsettings.
        /// </summary>
        public const string DefaultFilename = "appsettings.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfigSettings"/> class.
        /// </summary>
        /// <param name="config">Base app configuration to load.</param>
        public AppConfigSettings(IConfiguration config)
        {
            _currentLocation = Assembly.GetExecutingAssembly().Location;

            config.GetSection(nameof(AppConfigSettings)).Bind(this);

            if (Device == null)
            {
                Device = new();
            }

            if (Connection == null)
            {
                Connection = new();
            }

            if (RecentPath == null)
            {
                RecentPath = new();
            }

            if (string.IsNullOrEmpty(FramebufferGrabSavePath))
            {
                FramebufferGrabSavePath = _currentLocation;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfigSettings"/> class.
        /// </summary>
        private AppConfigSettings()
        {
            _currentLocation = Assembly.GetExecutingAssembly().Location;

            if (Device == null)
            {
                Device = new();
            }

            if (Connection == null)
            {
                Connection = new();
            }

            if (RecentPath == null)
            {
                RecentPath = new();
            }

            FramebufferGrabSavePath = _currentLocation;
        }

        /// <summary>
        /// Device section settings.
        /// </summary>
        public DeviceSectionSettings Device { get; set; }

        /// <summary>
        /// Connection section settings.
        /// </summary>
        public ConnectionSectionSettings Connection { get; set; }

        /// <summary>
        /// Recent paths settings.
        /// </summary>
        public RecentPathSection RecentPath { get; set; }

        /// <summary>
        /// Path to save framebuffer grabs to.
        /// </summary>
        public string FramebufferGrabSavePath { get; set; }

        /// <summary>
        /// Gets the default (empty) settings.
        /// </summary>
        /// <returns>Settings.</returns>
        public static AppConfigSettings GetDefault()
        {
            return new AppConfigSettings();
        }

        /// <summary>
        /// Serializes the current settings to JSON and saves to the specified file.
        /// </summary>
        /// <param name="path">Path of file to save to.</param>
        public void Save(string path)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
