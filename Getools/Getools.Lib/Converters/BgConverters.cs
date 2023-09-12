using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Getools.Lib.Antlr;
using Getools.Lib.Antlr.Gen;
using Getools.Lib.Error;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Bg;
using Getools.Lib.Game.Asset.Stan;
using Newtonsoft.Json;

namespace Getools.Lib.Converters
{
    /// <summary>
    /// Preferred interface to convert betweeen bg and various files types/formats.
    /// </summary>
    public static class BgConverters
    {
        /// <summary>
        /// Loads file content and parses as binary file.
        /// </summary>
        /// <param name="path">Path of file to read.</param>
        /// <returns>Parsed bg.</returns>
        public static BgFile ReadFromBinFile(string path)
        {
            var stan = Kaitai.BgParse.ParseBin(path);

            return stan;
        }
    }
}
