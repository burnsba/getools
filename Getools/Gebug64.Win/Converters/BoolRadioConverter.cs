using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gebug64.Win.Converters
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/a/34474668/1462295
    /// </remarks>
    public class BoolRadioConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            return this.Inverse ? !boolValue : boolValue;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            return this.Inverse ? !boolValue : boolValue;
        }
    }
}
