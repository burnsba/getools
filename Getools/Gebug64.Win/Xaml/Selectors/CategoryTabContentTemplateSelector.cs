using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Gebug64.Win.ViewModels.CategoryTabs;

namespace Gebug64.Win.Xaml.Selectors
{
    public class CategoryTabContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CheatTemplate { get; set; }
        public DataTemplate DebugTemplate { get; set; }
        public DataTemplate MetaTemplate { get; set; }
        public DataTemplate MiscTemplate { get; set; }
        public DataTemplate NothingTemplate { get; set; }
        public DataTemplate StageTemplate { get; set; }
        public DataTemplate ViTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Null value can be passed by IDE designer
            if (item == null)
            {
                return null;
            }

            switch (item)
            {
                case CheatTabViewModel cheatTabViewModel: return CheatTemplate;
                case DebugTabViewModel debugTabViewModel: return DebugTemplate;
                case MetaTabViewModel metaTabViewModel: return MetaTemplate;
                case MiscTabViewModel miscTabViewModel: return MiscTemplate;
                case StageTabViewModel stageTabViewModel: return StageTemplate;
                case ViTabViewModel viTabViewModel: return ViTemplate;
                default: return NothingTemplate;
            }
        }
    }
}
