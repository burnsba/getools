using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Gebug64.Win.Config;
using Gebug64.Win.ViewModels.Config;

namespace Gebug64.Win.Session
{
    public class MapperConfig
    {
        public static void SetConfiguration(IMapperConfigurationExpression mce)
        {
            mce.CreateMap<RecentPathSection, RecentPathSectionViewModel>();
            mce.CreateMap<DeviceSectionSettings, DeviceSectionViewModel>();
            mce.CreateMap<ConnectionSectionSettings, ConnectionSectionViewModel>();
            mce.CreateMap<AppConfigSettings, AppConfigViewModel>();

            mce.CreateMap<RecentPathSectionViewModel, RecentPathSection>();
            mce.CreateMap<DeviceSectionViewModel, DeviceSectionSettings>();
            mce.CreateMap<ConnectionSectionViewModel, ConnectionSectionSettings>();
            mce.CreateMap<AppConfigViewModel, AppConfigSettings>();
        }
    }
}
