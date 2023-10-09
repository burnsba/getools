using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Converters
{
    /// <summary>
    /// Helper class to try to make things truthy.
    /// </summary>
    public static class ToBoolConverter
    {
        /// <summary>
        /// Tries to enterpret value as bool.
        /// Text is trimmed and converted to lowercase for comparison.
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <returns>
        /// False, if string is null or empty.
        /// True if text is exactly "true", "1", or "t".
        /// False otherwise.
        /// </returns>
        public static bool ToBool(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            var normed = s.ToLower().Trim();

            if (normed.Length == 1)
            {
                if (normed[0] == '0'
                    || normed[0] == 'f')
                {
                    return false;
                }
                else if (normed[0] == '1'
                    || normed[0] == 't')
                {
                    return true;
                }
            }

            if (normed == "true")
            {
                return true;
            }

            return false;
        }
    }
}
