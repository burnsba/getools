using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Formatters
{
    /// <summary>
    /// String format helpers.
    /// </summary>
    public static class Strings
    {
        /// <summary>
        /// Returns the string surrounded with double quotes.
        /// </summary>
        /// <param name="s">String to quote.</param>
        /// <param name="maxLen">
        /// Optional max length. If value is provided, will truncate
        /// input string to this length if exceeded.</param>
        /// <returns>Quoted string.</returns>
        public static string ToQuotedString(string s, int? maxLen = null)
        {
            if (maxLen.HasValue && maxLen >= 0)
            {
                if (s.Length > maxLen)
                {
                    s = s.Substring(0, maxLen.Value);
                }
            }

            return $"\"{s}\"";
        }

        /// <summary>
        /// Accepts a string as if it were a variable to be used
        /// in a c declaration. If the string has a value, it is
        /// returned with an "&" prefix. Otherwise the c macro NULL
        /// is returned.
        /// </summary>
        /// <param name="s">Variable name.</param>
        /// <returns>Address of variable, or c macro NULL.</returns>
        public static string ToCPointerOrNull(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "NULL";
            }

            return $"&{s}";
        }
    }
}
