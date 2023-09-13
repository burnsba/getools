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
        public DataTemplate NothingTemplate { get; set; }
        public DataTemplate MetaTemplate { get; set; }
        public DataTemplate MiscTemplate { get; set; }
        public DataTemplate StageTemplate { get; set; }
        public DataTemplate DebugTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Null value can be passed by IDE designer
            if (item == null)
            {
                return null;
            }

            switch (item)
            {
                case MetaTabViewModel metaTabViewModel: return MetaTemplate;
                case MiscTabViewModel miscTabViewModel: return MiscTemplate;
                case StageTabViewModel stageTabViewModel: return StageTemplate;
                case DebugTabViewModel debugTabViewModel: return DebugTemplate;
                default: return NothingTemplate;
            }
        }
    }
}
