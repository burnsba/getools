using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game
{
    /// <summary>
    /// Configuration options.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Justification")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1203:Constants should appear before fields", Justification = "Justification")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "<Justification>")]
    public static class Config
    {
        /// <summary>
        /// Default indent when building source text files.
        /// </summary>
        public const string DefaultIndent = "    ";

        /// <summary>
        /// Goldeneye target platform (N64) pointer size in bytes.
        /// </summary>
        public const int TargetPointerSize = 4;

        /// <summary>
        /// Size of word in bytes on N64 MIPS hardware.
        /// </summary>
        public const int TargetWordSize = 4;

        /// <summary>
        /// Size of short/ushort in bytes on N64 MIPS hardware.
        /// </summary>
        public const int TargetShortSize = 2;

        /// <summary>
        /// Default alignment for pointers on MIPS.
        /// </summary>
        public const int TargetPointerAlignment = 4;

        /// <summary>
        /// C macro name, used to build a 32-bit word from two 16-bit values.
        /// </summary>
        public const string CMacro_WordFromShorts = "_mkword";

        /// <summary>
        /// Format string. Appended to <see cref="CMacro_WordFromShorts"/> to generate
        /// a string to be used in .c file.
        /// </summary>
        public const string CMacro_WordFromShorts_Format = CMacro_WordFromShorts + "({0}, {1})";

        /// <summary>
        /// C macro name, used to build a 16-bit value from two 8-bit values.
        /// </summary>
        public const string CMacro_ShortFromBytes = "_mkshort";

        /// <summary>
        /// Format string. Appended to <see cref="CMacro_ShortFromBytes"/> to generate
        /// a string to be used in .c file.
        /// </summary>
        public const string CMacro_ShortFromBytes_Format = CMacro_ShortFromBytes + "({0}, {1})";

        /// <summary>
        /// Builds c macro call to turn two 16-bit values into a 32-bit word.
        /// </summary>
        /// <param name="s1">First value.</param>
        /// <param name="s2">Second value.</param>
        /// <returns>C macro call string.</returns>
        public static string CMacro_WordFromShortShort(UInt16 s1, UInt16 s2)
        {
            return string.Format(CMacro_WordFromShorts_Format, s1, s2);
        }

        /// <summary>
        /// Builds c macro call to turn two values into a 32-bit word.
        /// </summary>
        /// <param name="s1">First value.</param>
        /// <param name="s2">Second value.</param>
        /// <returns>C macro call string.</returns>
        public static string CMacro_WordFromShortShort(string s1, string s2)
        {
            return string.Format(CMacro_WordFromShorts_Format, s1, s2);
        }

        /// <summary>
        /// Builds c macro call to turn a 16-bit value, 8-bit value, and 8-bit value into a 32-bit word.
        /// </summary>
        /// <param name="s1">First value.</param>
        /// <param name="b1">Second value.</param>
        /// <param name="b2">Third value.</param>
        /// <returns>C macro call string.</returns>
        public static string CMacro_WordFromShortByteByte(UInt16 s1, Byte b1, Byte b2)
        {
            return string.Format(
                CMacro_WordFromShorts_Format,
                s1,
                string.Format(
                    CMacro_ShortFromBytes_Format,
                    b1,
                    b2));
        }

        /// <summary>
        /// Builds c macro call to turn a 16-bit value string, 8-bit value string, and 8-bit value string into a 32-bit word.
        /// </summary>
        /// <param name="s1">First value.</param>
        /// <param name="b1">Second value.</param>
        /// <param name="b2">Third value.</param>
        /// <returns>C macro call string.</returns>
        public static string CMacro_WordFromShortByteByte(string s1, string b1, string b2)
        {
            return string.Format(
                CMacro_WordFromShorts_Format,
                s1,
                string.Format(
                    CMacro_ShortFromBytes_Format,
                    b1,
                    b2));
        }

        /// <summary>
        /// Builds c macro call to turn a 8-bit value, 8-bit value, and 16-bit value into a 32-bit word.
        /// </summary>
        /// <param name="b1">First value.</param>
        /// <param name="b2">Second value.</param>
        /// <param name="s1">Third value.</param>
        /// <returns>C macro call string.</returns>
        public static string CMacro_WordFromByteByteShort(Byte b1, Byte b2, UInt16 s1)
        {
            return string.Format(
                CMacro_WordFromShorts_Format,
                string.Format(
                    CMacro_ShortFromBytes_Format,
                    b1,
                    b2),
                s1);
        }

        /// <summary>
        /// Builds c macro call to turn a 8-bit value string, 8-bit value string, and 16-bit value string into a 32-bit word.
        /// </summary>
        /// <param name="b1">First value.</param>
        /// <param name="b2">Second value.</param>
        /// <param name="s1">Third value.</param>
        /// <returns>C macro call string.</returns>
        public static string CMacro_WordFromByteByteShort(string b1, string b2, string s1)
        {
            return string.Format(
                CMacro_WordFromShorts_Format,
                string.Format(
                    CMacro_ShortFromBytes_Format,
                    b1,
                    b2),
                s1);
        }

        /// <summary>
        /// Builds c macro call to turn four 8-bit values into a 32-bit word.
        /// </summary>
        /// <param name="b1">First value.</param>
        /// <param name="b2">Second value.</param>
        /// <param name="b3">Third value.</param>
        /// <param name="b4">Fourth value.</param>
        /// <returns>C macro call string.</returns>
        public static string CMacro_WordFromByteByteByteByte(Byte b1, Byte b2, Byte b3, Byte b4)
        {
            return string.Format(
                string.Format(
                    CMacro_ShortFromBytes_Format,
                    b1,
                    b2),
                string.Format(
                    CMacro_ShortFromBytes_Format,
                    b3,
                    b4));
        }

        /// <summary>
        /// Builds c macro call to turn four string values into a 32-bit word.
        /// </summary>
        /// <param name="b1">First value.</param>
        /// <param name="b2">Second value.</param>
        /// <param name="b3">Third value.</param>
        /// <param name="b4">Fourth value.</param>
        /// <returns>C macro call string.</returns>
        public static string CMacro_WordFromByteByteByteByte(string b1, string b2, string b3, string b4)
        {
            return string.Format(
                string.Format(
                    CMacro_ShortFromBytes_Format,
                    b1,
                    b2),
                string.Format(
                    CMacro_ShortFromBytes_Format,
                    b3,
                    b4));
        }

        /// <summary>
        /// Header text to prepend to automatically built source text files.
        /// Will be surrounded with /* */.
        /// </summary>
        public static List<String> COutputPrefix = new List<string>()
        {
            "This file was automatically generated",
            string.Empty,
        };
    }
}
