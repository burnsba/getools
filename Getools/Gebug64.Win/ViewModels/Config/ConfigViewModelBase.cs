using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    /// <summary>
    /// Base class for app settings viewmodel sections.
    /// </summary>
    public abstract class ConfigViewModelBase : ISettingsViewModel
    {
        /// <inheritdoc />
        public bool IsDirty { get; set; }

        /// <inheritdoc />
        public void ClearIsDirty()
        {
            IsDirty = false;

            var props = GetType().GetProperties();
            foreach (var prop in props)
            {
                if (typeof(ISettingsViewModel).IsAssignableFrom(prop.PropertyType))
                {
                    var mi = prop.PropertyType.GetMethod(nameof(ClearIsDirty));

                    var propInstance = prop.GetValue(this, null);

                    mi!.Invoke(propInstance, null);
                }
            }
        }
    }
}
