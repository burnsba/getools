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
