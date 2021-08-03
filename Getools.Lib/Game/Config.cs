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
                "baselib.h",
                "stan.h",
            };

            public const string HeaderCTypeName = "StandFileHeader";
            public const string TileCTypeName = "StandFileTile";
            public const string PointCTypeName = "StandFilePoint";
            public const string FooterCTypeName = "StandFileFooter";
        }
    }
}
