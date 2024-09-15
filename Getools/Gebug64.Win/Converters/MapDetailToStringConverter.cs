using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;
using Gebug64.Win.Enum;
using Getools.Lib.Compiler.Map;

namespace Gebug64.Win.Converters
{
    /// <summary>
    /// <see cref="MapDetail"/> converter to friendly UI text.
    /// </summary>
    public class MapDetailToStringConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MapDetail)
            {
                var md = (MapDetail)value;

                return md.ToString();
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                var trimmed = ((string)value)?.Trim();

                if (string.IsNullOrEmpty(trimmed))
                {
                    return new MapDetail();
                }

                var splits = trimmed?.Split(' ') ?? new List<string>().ToArray();
                if (splits.Length == 2)
                {
                    var name = splits[0];

                    UInt32 address = 0;

                    try
                    {
                        int intval = (int)new System.ComponentModel.Int32Converter()!.ConvertFromString(splits[1]!)!;
                        address = (UInt32)intval;
                    }
                    catch
                    {
                    }

                    if (address > 0)
                    {
                        return new MapDetail()
                        {
                            Address = address,
                            Name = name,
                        };
                    }
                }
                else if (splits.Length == 1)
                {
                    UInt32 address = 0;

                    try
                    {
                        int intval = (int)new System.ComponentModel.Int32Converter()!.ConvertFromString(splits[0]!)!;
                        address = (UInt32)intval;
                    }
                    catch
                    {
                    }

                    if (address > 0)
                    {
                        return new MapDetail()
                        {
                            Address = address,
                            Name = Getools.Lib.Formatters.IntegralTypes.ToHex8(address),
                        };
                    }
                }

                return new MapDetail()
                {
                    Address = 0,
                    Name = trimmed!,
                };
            }

            return new MapDetail();
        }
    }
}
