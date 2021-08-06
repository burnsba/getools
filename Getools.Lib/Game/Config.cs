using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game
{
    public static class Config
    {
        public const string DefaultIndent = "    ";

        public const int TargetPointerSize = 4;

        /// <summary>
        /// Will be surrounded with /* */.
        /// </summary>
        public static List<String> COutputPrefix = new List<string>()
        {
            "This file was automatically generated",
            "",
        };

        public static class Stan
        {
            public static List<string> IncludeHeaders = new List<string>()
            {
                "ultra64.h",
                "stan.h",
            };

            public static List<DataFormats> SupportedInputFormats = new List<DataFormats>()
            {
                DataFormats.C,
                DataFormats.BetaC,
                DataFormats.Json,
                DataFormats.Bin,
                DataFormats.BetaBin,
            };

            public static List<DataFormats> SupportedOutputFormats = new List<DataFormats>()
            {
                DataFormats.C,
                DataFormats.BetaC,
                DataFormats.Json,
                DataFormats.Bin,
                DataFormats.BetaBin,
            };

            public const string HeaderCTypeName = "StandFileHeader";
            public const string TileCTypeName = "StandTile";
            public const string FooterCTypeName = "StandFileFooter";

            public const string TileBetaCTypeName = "BetaStandTile";
            // header and footer are the same for beta
            public const string BetaFooterCTypeName = "char";

            public const string DefaultDeclarationName_StandFileFooter = "footer";
            public const string DefaultDeclarationName_BetaFooter = "beta_footer";
        }
    }
}
