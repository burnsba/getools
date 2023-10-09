using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Gebug64.Win.ViewModels;

namespace Gebug64.Win.Converters
{
    /// <summary>
    /// Gets the <see cref="Type"/> of an object.
    /// </summary>
    public class DataTypeConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType() ?? Binding.DoNothing;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
