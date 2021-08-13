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

        public const string CMacro_WordFromShorts = "_mkword";
        public const string CMacro_WordFromShorts_Format = CMacro_WordFromShorts + "({0}, {1})";

        public const string CMacro_ShortFromBytes = "_mkshort";
        public const string CMacro_ShortFromBytes_Format = CMacro_ShortFromBytes + "({0}, {1})";

        public static string CMacro_WordFromShortShort(UInt16 s1, UInt16 s2)
        {
            return string.Format(CMacro_WordFromShorts_Format, s1, s2);
        }

        public static string CMacro_WordFromShortShort(string s1, string s2)
        {
            return string.Format(CMacro_WordFromShorts_Format, s1, s2);
        }

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
