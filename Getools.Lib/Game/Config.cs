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

        /// <summary>
        /// Configuration options for <see cref="Stan.StandFile"/>.
        /// </summary>
        public static class Stan
        {
            /// <summary>
            /// C headers to #include when building .c files.
            /// </summary>
            public static List<string> IncludeHeaders = new List<string>()
            {
                "ultra64.h",
                "stan.h",
            };

            /// <summary>
            /// Formats available for reading in a <see cref="Stan.StandFile"/>.
            /// </summary>
            public static List<DataFormats> SupportedInputFormats = new List<DataFormats>()
            {
                DataFormats.C,
                DataFormats.BetaC,
                DataFormats.Json,
                DataFormats.Bin,
                DataFormats.BetaBin,
            };

            /// <summary>
            /// Formats available to output a <see cref="Stan.StandFile"/>.
            /// </summary>
            public static List<DataFormats> SupportedOutputFormats = new List<DataFormats>()
            {
                DataFormats.C,
                DataFormats.BetaC,
                DataFormats.Json,
                DataFormats.Bin,
                DataFormats.BetaBin,
            };

            /// <summary>
            /// C file, header section type name, non-beta. Should match known struct type.
            /// </summary>
            public const string HeaderCTypeName = "StandFileHeader";

            /// <summary>
            /// C file, tile type name, non-beta. Should match known struct type.
            /// </summary>
            public const string TileCTypeName = "StandTile";

            /// <summary>
            /// C file, footer section type name, non-beta. Should match known struct type.
            /// </summary>
            public const string FooterCTypeName = "StandFileFooter";

            /// <summary>
            /// C file, beta tile type name. Should match known struct type.
            /// </summary>
            public const string TileBetaCTypeName = "BetaStandTile";

            ////// header and footer are the same for beta

            /// <summary>
            /// C file, beta footer points list type name. Should match known struct type.
            /// </summary>
            public const string BetaFooterCTypeName = "char";

            /// <summary>
            /// C file, default variable declaration prefix for tiles.
            /// </summary>
            public const string DefaultDeclarationName_StandTile = "tile_";

            /// <summary>
            /// C file, footer variable declaration name.
            /// </summary>
            public const string DefaultDeclarationName_StandFileFooter = "footer";
        }
    }
}
