using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Baml2006;
using System.Windows.Controls;
using AutoMapper.Configuration.Annotations;
using Gebug64.Win.ViewModels.CategoryTabs;

namespace Gebug64.Win.Xaml.Selectors
{
    /// <summary>
    /// Data template selector. Used in tab interface to map a viewmodel type
    /// to a corresponding <see cref="DataTemplate"/>.
    /// The viewmodel must implement <see cref="ICategoryTabViewModel"/>.
    /// The data template filename must befine with "categorytabcontent_" (any case),
    /// and it must have an X:Key that begins with "CategoryTabContent" (case sensitive).
    /// The load code is deigned to set a single fallback template that is type <see cref="TabViewModelBase"/>,
    /// which does not implement <see cref="ICategoryTabViewModel"/>.
    /// </summary>
    public class CategoryTabContentTemplateSelector : DataTemplateSelector
    {
        private static bool _isInit = false;

        private static Dictionary<Type, DataTemplate> _vmToDataTemplateResolver = new();
        private static DataTemplate? _fallbackTemplate = null;

#pragma warning disable CS8603 // Possible null reference return.
        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Null value can be passed by IDE designer
            if (item == null)
            {
                return null;
            }

            ReadResourceDictionaries();

            var itemType = item.GetType();
            if (_vmToDataTemplateResolver.ContainsKey(itemType))
            {
                return _vmToDataTemplateResolver[itemType];
            }

            // might be null.
            return _fallbackTemplate;
        }
#pragma warning restore CS8603 // Possible null reference return.

        private static void ReadResourceDictionaries()
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;

            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string[] resourceDictionaries = currentAssembly.GetManifestResourceNames();
            foreach (string resourceName in resourceDictionaries)
            {
                ManifestResourceInfo info = currentAssembly.GetManifestResourceInfo(resourceName)!;
                if (info.ResourceLocation != ResourceLocation.ContainedInAnotherAssembly)
                {
                    Stream resourceStream = currentAssembly.GetManifestResourceStream(resourceName)!;
                    using (ResourceReader reader = new ResourceReader(resourceStream))
                    {
                        foreach (DictionaryEntry entry in reader)
                        {
                            // There are some other resources like embedded fonts which the baml parser
                            // will fail to parse, so filter to just relevant files based on the filename.
                            if (!entry.Key.ToString()!.ToLower().Contains("/categorytabcontent_"))
                            {
                                continue;
                            }

                            var readStream = entry.Value as Stream;
                            Baml2006Reader bamlReader = new Baml2006Reader(readStream);
                            var loadedObject = System.Windows.Markup.XamlReader.Load(bamlReader);
                            if (loadedObject is ResourceDictionary)
                            {
                                var rd = (ResourceDictionary)loadedObject;

                                foreach (var zz in rd.Keys)
                                {
                                    // Look for datatemplate with X:Key that starts with "CategoryTabContent".
                                    if (zz.ToString()!.StartsWith("CategoryTabContent"))
                                    {
                                        var rdval = rd[zz];

                                        // Make sure this is a datatemplate.
                                        if (rdval is DataTemplate template)
                                        {
                                            // A concrete viewmodel implementation should implement ICategoryTabViewModel.
                                            if (typeof(ICategoryTabViewModel).IsAssignableFrom((Type)template.DataType))
                                            {
                                                 if (!_vmToDataTemplateResolver.ContainsKey((Type)template.DataType))
                                                {
                                                    _vmToDataTemplateResolver[(Type)template.DataType] = template;
                                                }
                                            }/***
                                            * otherwise, check if this is the fallback "Nothing" template, for
                                            * the base type, without the interface.
                                            */
                                            else if (typeof(TabViewModelBase).IsAssignableFrom((Type)template.DataType))
                                            {
                                                _fallbackTemplate = template;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
