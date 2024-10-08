﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Gebug64.Win.Config;
using Gebug64.Win.ViewModels.Config;

namespace Gebug64.Win.Session
{
    /// <summary>
    /// Helper class to define automapper configuration.
    /// </summary>
    public class MapperConfig
    {
        /// <summary>
        /// Startup method to configure AutoMapper class mappings.
        /// </summary>
        /// <param name="mce">Configurator.</param>
        public static void SetConfiguration(IMapperConfigurationExpression mce)
        {
            mce.CreateMap<RecentPathSection, RecentPathSectionViewModel>();
            mce.CreateMap<DeviceSectionSettings, DeviceSectionViewModel>();
            mce.CreateMap<ConnectionSectionSettings, ConnectionSectionViewModel>();
            mce.CreateMap<AppConfigSettings, AppConfigViewModel>();
            mce.CreateMap<MapSettings, MapSettingsViewModel>();
            mce.CreateMap<MemorySettings, MemorySettingsViewModel>();
            mce.CreateMap<UiLayoutState, UiLayoutStateViewModel>();

            mce.CreateMap<RecentPathSectionViewModel, RecentPathSection>();
            mce.CreateMap<DeviceSectionViewModel, DeviceSectionSettings>();
            mce.CreateMap<ConnectionSectionViewModel, ConnectionSectionSettings>();
            mce.CreateMap<AppConfigViewModel, AppConfigSettings>();
            mce.CreateMap<MapSettingsViewModel, MapSettings>();
            mce.CreateMap<MemorySettingsViewModel, MemorySettings>();
            mce.CreateMap<UiLayoutStateViewModel, UiLayoutState>();
        }
    }
}
