using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Palantir
{
    /// <summary>
    /// Formatting helper methods.
    /// </summary>
    public static class Format
    {
        /// <summary>
        /// Convert double to string in the standard way.
        /// </summary>
        /// <param name="d">Double.</param>
        /// <param name="format">Format specifier.</param>
        /// <returns>String.</returns>
        public static string DoubleToStringFormat(double d, string format)
        {
            return d.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
