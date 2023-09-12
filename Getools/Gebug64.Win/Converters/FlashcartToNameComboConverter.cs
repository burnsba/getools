using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Flashcart;

namespace Gebug64.Win.Converters
{
    [ValueConversion(typeof(IFlashcart), typeof(string))]
    public class FlashcartToNameComboConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flashcart = (IFlashcart)value;
            switch (flashcart)
            {
                case Everdrive ed: return nameof(Everdrive);
                default: throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
