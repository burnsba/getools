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
    /// <see cref="MemoryDataType"/> converter to friendly UI text.
    /// </summary>
    public class MemoryDataTypeToStringConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MemoryDataType)
            {
                var mdt = (MemoryDataType)value;

                switch (mdt)
                {
                    case MemoryDataType.S8:
                    case MemoryDataType.U8:
                    case MemoryDataType.S16:
                    case MemoryDataType.U16:
                    case MemoryDataType.S32:
                    case MemoryDataType.U32:
                    case MemoryDataType.F32:
                        return mdt.ToString().ToLower();

                    case MemoryDataType.Array:
                    case MemoryDataType.Pointer:
                        return mdt.ToString();
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
                    case "u8": return MemoryDataType.U8;
                    case "s8": return MemoryDataType.S8;
                    case "s16": return MemoryDataType.S16;
                    case "u16": return MemoryDataType.U16;
                    case "s32": return MemoryDataType.S32;
                    case "u32": return MemoryDataType.U32;
                    case "f32": return MemoryDataType.F32;
                    case "array": return MemoryDataType.Array;
                    case "pointer": return MemoryDataType.Pointer;
                }
            }

            return MemoryDataType.DefaultUnknown;
        }
    }
}
