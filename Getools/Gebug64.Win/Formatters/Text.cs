using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Formatters
{
    /// <summary>
    /// Text formatting helper methods.
    /// </summary>
    public class Text
    {
        /// <summary>
        /// Removes trailing newlines, whitespace, and null characters.
        /// </summary>
        /// <param name="text">Input text.</param>
        /// <returns>Text.</returns>
        public static string RemoveTrailingNonVisible(string text)
        {
            // reverse returns new collection.
            var chars = text.ToCharArray().Reverse().ToList();

            while (chars[0] == '\0'
                || chars[0] == '\t'
                || chars[0] == '\r'
                || chars[0] == '\n'
                || chars[0] == ' ')
            {
                chars.RemoveAt(0);
            }

            // reverse reverses collection in place.
            chars.Reverse();
            var result = string.Join(string.Empty, chars);

            return result;
        }
    }
}
