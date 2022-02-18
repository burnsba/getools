using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Formatters
{
    /// <summary>
    /// Formats floating point type objects to string.
    /// </summary>
    public static class FloatingPoint
    {
        /// <summary>
        /// Converts Single/float value to string. If the value
        /// is not in scientific notation, ensures it has a decimal
        /// and (if integer value) trailing zero.
        /// </summary>
        /// <param name="val">Value to format.</param>
        /// <returns>NaN string, or value.</returns>
        public static string ToFloatString(Single val)
        {
            if (Single.IsNaN(val))
            {
                return "NaN";
            }

            var s = val.ToString().ToLower();

            if (s.IndexOf('e') >= 0)
            {
                return s;
            }

            if (s.IndexOf('.') < 0)
            {
                return s + ".0";
            }

            return s;
        }

        /// <summary>
        /// Converts to single/float, then formats as floating point string.
        /// </summary>
        /// <param name="val">Value to convert.</param>
        /// <returns>NaN string, or value.</returns>
        public static string ToFloatString(int val)
        {
            return ToFloatString((Single)val);
        }

        /// <summary>
        /// Converts to single/float, then formats as floating point string.
        /// </summary>
        /// <param name="val">Value to convert.</param>
        /// <returns>NaN string, or value.</returns>
        public static string ToFloatString(short val)
        {
            return ToFloatString((Single)val);
        }

        /// <summary>
        /// Converts to single/float, then formats as floating point string.
        /// </summary>
        /// <param name="val">Value to convert.</param>
        /// <returns>NaN string, or value.</returns>
        public static string ToFloatString(double val)
        {
            return ToFloatString((Single)val);
        }

        /// <summary>
        /// Converts Single/float value to string. If the value
        /// is not in scientific notation, ensures it has a decimal
        /// and (if integer value) trailing zero and appends "f".
        /// </summary>
        /// <param name="val">Value to format.</param>
        /// <returns>NaN string, or value in scientific notation, or value followed by "f".</returns>
        public static string ToFloatCLiteral(Single val)
        {
            if (Single.IsNaN(val))
            {
                return "NAN";
            }

            var s = val.ToString().ToLower();

            if (s.IndexOf('e') >= 0)
            {
                return s;
            }

            if (s.IndexOf('.') < 0)
            {
                return s + ".0f";
            }

            return s + "f";
        }

        /// <summary>
        /// Converts to single/float, then formats as floating point string.
        /// </summary>
        /// <param name="val">Value to convert.</param>
        /// <returns>NaN string, or value in scientific notation, or value followed by "f".</returns>
        public static string ToFloatCLiteral(int val)
        {
            return ToFloatCLiteral((Single)val);
        }

        /// <summary>
        /// Converts to single/float, then formats as floating point string.
        /// </summary>
        /// <param name="val">Value to convert.</param>
        /// <returns>NaN string, or value in scientific notation, or value followed by "f".</returns>
        public static string ToFloatCLiteral(short val)
        {
            return ToFloatCLiteral((Single)val);
        }

        /// <summary>
        /// Converts to single/float, then formats as floating point string.
        /// </summary>
        /// <param name="val">Value to convert.</param>
        /// <returns>NaN string, or value in scientific notation, or value followed by "f".</returns>
        public static string ToFloatCLiteral(double val)
        {
            return ToFloatCLiteral((Single)val);
        }
    }
}
