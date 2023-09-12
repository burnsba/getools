using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib
{
    /// <summary>
    /// Parsing helper methods.
    /// </summary>
    public static class ParseHelpers
    {
        /// <summary>
        /// Helper function to support parsing a string that has a "0x" prefix as hex.
        /// </summary>
        /// <param name="s">String to parse.</param>
        /// <param name="i">Result of parse.</param>
        /// <returns>True if value could be converted to int, false otherwise.</returns>
        public static bool TryParseInt(string s, out int i)
        {
            i = 0;
            int result;
            var trimmed = s?.Trim()?.ToLower() ?? string.Empty;

            if (trimmed.StartsWith("0x"))
            {
                try
                {
                    result = Convert.ToInt32(trimmed, 16);
                    i = result;
                    return true;
                }
                catch
                {
                }
            }
            else
            {
                try
                {
                    result = Convert.ToInt32(trimmed, 10);
                    i = result;
                    return true;
                }
                catch
                {
                }
            }

            return false;
        }

        /// <summary>
        /// Helper method, to treat a value as a pointer. If the value is zero,
        /// it is interpreted as a null pointer.
        /// </summary>
        /// <param name="val">Val to convert.</param>
        /// <returns>Value, if greater than zero, otherwise null.</returns>
        public static int? ToCPointer(uint val)
        {
            if (val > 0)
            {
                return (int)val;
            }

            return null;
        }

        /// <summary>
        /// Helper method, to treat a value as a pointer. If the value is zero,
        /// it is interpreted as a null pointer.
        /// </summary>
        /// <param name="val">Val to convert.</param>
        /// <returns>Value, if greater than zero, otherwise null.</returns>
        public static int? ToCPointer(int val)
        {
            if (val > 0)
            {
                return (int)val;
            }

            return null;
        }

        /// <summary>
        /// Helper method, to treat a value as a pointer. If the value is zero,
        /// it is interpreted as a null pointer.
        /// </summary>
        /// <param name="val">Val to convert.</param>
        /// <returns>Value, if greater than zero, otherwise null.</returns>
        public static int? ToCPointer(int? val)
        {
            if (val > 0)
            {
                return (int)val;
            }

            return null;
        }
    }
}
