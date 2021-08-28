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
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.Stan;
using Newtonsoft.Json;

namespace Getools.Lib.Converters
{
    /// <summary>
    /// Preferred interface to convert betweeen setup and various files types/formats.
    /// </summary>
    public static class SetupConverters
    {
        /// <summary>
        /// Loads file content and parses as binary file.
        /// </summary>
        /// <param name="path">Path of file to read.</param>
        /// <returns>Parsed setup.</returns>
        public static StageSetupFile ReadFromBinFile(string path)
        {
            return Kaitai.SetupParser.ParseBin(path);
        }

        /// <summary>
        /// Loads file content and parses as JSON text source file.
        /// </summary>
        /// <param name="path">Path of file to read.</param>
        /// <returns>Parsed setup.</returns>
        public static StageSetupFile ReadFromJson(string path)
        {
            var json = File.ReadAllText(path);
            var setup = JsonConvert.DeserializeObject<StageSetupFile>(
                json,
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                });

            return setup;
        }

        /// <summary>
        /// Converts setup to complete .c text source file.
        /// </summary>
        /// <param name="source">Object to convert.</param>
        /// <param name="path">Path of file to write to.</param>
        public static void WriteToC(StageSetupFile source, string path)
        {
            using (var sw = new StreamWriter(path, false))
            {
                source.WriteToCFile(sw);
            }
        }

        /// <summary>
        /// Converts setup to JSON text source file.
        /// </summary>
        /// <param name="source">Object to convert.</param>
        /// <param name="path">Path of file to write to.</param>
        public static void WriteToJson(StageSetupFile source, string path)
        {
            var json = JsonConvert.SerializeObject(
                source,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                });

            File.WriteAllText(path, json);
        }
    }
}
