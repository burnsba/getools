using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Palantir
{
    public static class Format
    {
        public static string DoubleToStringFormat(double d, string format)
        {
            return d.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
