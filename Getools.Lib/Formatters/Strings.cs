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

        /// <summary>
        /// Converts the string to a quoted c literal.
        /// If the <see cref="Value"/> is null or empty, an
        /// empty string is returned.
        /// </summary>
        /// <param name="s">String to format.</param>
        /// <param name="prefix">Optional prefix before string.</param>
        /// <returns>Quoted value.</returns>
        public static string ToCValue(string s, string prefix = "")
        {
            if (string.IsNullOrEmpty(s))
            {
                return prefix + Formatters.Strings.ToQuotedString(string.Empty);
            }

            return prefix + Formatters.Strings.ToQuotedString(s);
        }

        /// <summary>
        /// Converts the string to a quoted c literal.
        /// If the <see cref="Value"/> is null or empty, the c macro NULL
        /// is returned (without quotes).
        /// </summary>
        /// <param name="s">String to format.</param>
        /// <param name="prefix">Optional prefix before string.</param>
        /// <returns>Quoted value.</returns>
        public static string ToCValueOrNull(string s, string prefix = "")
        {
            if (string.IsNullOrEmpty(s))
            {
                return $"{prefix}NULL";
            }

            return prefix + Formatters.Strings.ToQuotedString(s);
        }

        /// <summary>
        /// Converts the string to a quoted c literal.
        /// If the <see cref="Value"/> is null, the c macro NULL
        /// is returned (without quotes). Otherwise, a quoted string is returned (this may be <see cref="string.Empty"/>).
        /// </summary>
        /// <param name="s">String to format.</param>
        /// <param name="prefix">Optional prefix before string.</param>
        /// <returns>Quoted value.</returns>
        public static string ToCValueOrNullEmpty(string s, string prefix = "")
        {
            if (object.ReferenceEquals(null, s))
            {
                return $"{prefix}NULL";
            }

            return prefix + Formatters.Strings.ToQuotedString(s);
        }
    }
}
