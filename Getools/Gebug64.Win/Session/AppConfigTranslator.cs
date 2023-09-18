using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Win.Config;
using Gebug64.Win.ViewModels.Config;

namespace Gebug64.Win.Session
{
    public class AppConfigTranslator : ISessionTranslator<AppConfigViewModel, AppConfigSettings>
    {
        private TranslateService _translatorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfigTranslator"/> class.
        /// </summary>
        /// <param name="translatorService">Parent translation service.</param>
        public AppConfigTranslator(TranslateService translatorService)
        {
            _translatorService = translatorService;
        }

        /// <inheritdoc />
        public AppConfigViewModel ConvertBack(AppConfigSettings source)
        {
            var dest = new AppConfigViewModel();

            dest.Device = new();

            _translatorService.ReflectionTranslate(source.Device, dest.Device);

            dest.Connection = new();

            _translatorService.ReflectionTranslate(source.Connection, dest.Connection);

            if (!object.ReferenceEquals(null, source.RecentSendRom))
            {
                if (object.ReferenceEquals(null, dest.RecentSendRom))
                {
                    dest.RecentSendRom = new System.Collections.ObjectModel.ObservableCollection<string>();
                }

                source.RecentSendRom.ForEach(x => dest.RecentSendRom.Add(x));
            }

            dest.FramebufferGrabSavePath = source.FramebufferGrabSavePath;

            dest.ClearIsDirty();

            return dest;
        }

        /// <inheritdoc />
        public AppConfigSettings ConvertFrom(AppConfigViewModel source)
        {
            var dest = AppConfigSettings.GetDefault();

            dest.Device = new();

            _translatorService.ReflectionTranslate(source.Device, dest.Device);

            dest.Connection = new();

            _translatorService.ReflectionTranslate(source.Connection, dest.Connection);

            if (!object.ReferenceEquals(null, source.RecentSendRom))
            {
                if (object.ReferenceEquals(null, dest.RecentSendRom))
                {
                    dest.RecentSendRom = new List<string>();
                }

                source.RecentSendRom.ToList().ForEach(x => dest.RecentSendRom.Add(x));
            }

            dest.FramebufferGrabSavePath = source.FramebufferGrabSavePath;

            return dest;
        }
    }
}
