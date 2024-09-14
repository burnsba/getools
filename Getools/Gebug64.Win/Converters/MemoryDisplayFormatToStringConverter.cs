using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Gebug64.Win.Enum;

namespace Gebug64.Win.Converters
{
    /// <summary>
    /// Convert <see cref="MemoryDisplayFormat"/> to friendly UI text.
    /// </summary>
    public class MemoryDisplayFormatToStringConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MemoryDisplayFormat)
            {
                var mdf = (MemoryDisplayFormat)value;

                switch (mdf)
                {
                    case MemoryDisplayFormat.Decimal: return "dec";
                    case MemoryDisplayFormat.Hex: return "hex";
                    case MemoryDisplayFormat.Float0_00: return "0.00";
                    case MemoryDisplayFormat.Float0_0000: return "0.0000";
                    case MemoryDisplayFormat.Float0_x: return "0.x";
                }
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                var lower = ((string)value).ToLower();

                switch (lower)
                {
                    case "dec": return MemoryDisplayFormat.Decimal;
                    case "hex": return MemoryDisplayFormat.Hex;
                    case "0.00": return MemoryDisplayFormat.Float0_00;
                    case "0.0000": return MemoryDisplayFormat.Float0_0000;
                    case "0.x": return MemoryDisplayFormat.Float0_x;
                }
            }

            return MemoryDisplayFormat.DefaultUnknown;
        }
    }
}
