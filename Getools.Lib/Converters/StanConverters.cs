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
using Getools.Lib.Game.Asset.Stan;
using Newtonsoft.Json;

namespace Getools.Lib.Converters
{
    /// <summary>
    /// Preferred interface to convert betweeen stan and various files types/formats.
    /// </summary>
    public static class StanConverters
    {
        /// <summary>
        /// Loads file content and parses as c text source file.
        /// </summary>
        /// <param name="path">Path of file to read.</param>
        /// <returns>Parsed stan.</returns>
        /// <remarks>
        /// This uses <see cref="CStanListener"/>, built on the Antlr generated code using the C11 parser.
        /// </remarks>
        public static StandFile ParseFromC(string path)
        {
            var tree = C11Parser.ParseC(path);

            CStanListener listener = new CStanListener();
            ParseTreeWalker.Default.Walk(listener, tree);

            listener.Result.SetFormat(TypeFormat.Normal);
            listener.Result.DeserializeFix();

            return listener.Result;
        }

        /// <summary>
        /// Loads file content and parses as c text source file, using beta structs.
        /// </summary>
        /// <param name="path">Path of file to read.</param>
        /// <returns>Parsed stan.</returns>
        /// <remarks>
        /// This uses <see cref="CStanListener"/>, built on the Antlr generated code using the C11 parser.
        /// </remarks>
        public static StandFile ParseFromBetaC(string path)
        {
            var tree = C11Parser.ParseC(path);

            BetaCStanListener listener = new BetaCStanListener();
            ParseTreeWalker.Default.Walk(listener, tree);

            listener.Result.SetFormat(TypeFormat.Beta);
            listener.Result.DeserializeFix();

            return listener.Result;
        }

        /// <summary>
        /// Loads file content and parses as binary file.
        /// </summary>
        /// <param name="path">Path of file to read.</param>
        /// <param name="name">Header variable name.</param>
        /// <returns>Parsed setup.</returns>
        public static StandFile ReadFromBinFile(string path, string name)
        {
            var stan = Kaitai.StanParser.ParseBin(path);
            stan.Header.Name = name;

            return stan;
        }

        /// <summary>
        /// Loads file content and parses as binary file.
        /// </summary>
        /// <param name="path">Path of file to read.</param>
        /// <param name="name">Header variable name.</param>
        /// <returns>Parsed setup.</returns>
        public static StandFile ReadFromBetaBinFile(string path, string name)
        {
            var stan = Kaitai.BetaStanParser.ParseBin(path);
            stan.Header.Name = name;

            return stan;
        }

        /// <summary>
        /// Loads file content and parses as JSON text source file.
        /// The <see cref="StandFile.Format"/> needs to be set in the provided object.
        /// </summary>
        /// <param name="path">Path of file to read.</param>
        /// <returns>Parsed stan.</returns>
        public static StandFile ReadFromJson(string path)
        {
            var json = File.ReadAllText(path);
            var stan = JsonConvert.DeserializeObject<StandFile>(json);

            if (stan.Format == TypeFormat.DefaultUnknown)
            {
                throw new BadFileFormatException("Type format not set in json");
            }

            if (object.ReferenceEquals(null, stan.Header))
            {
                throw new BadFileFormatException("Missing header in json");
            }

            if (object.ReferenceEquals(null, stan.Footer))
            {
                throw new BadFileFormatException("Missing footer in json");
            }

            // hmmm, does it matter where this happens? Want to check for null header and footer
            // at least, but should this be moved after the tile checks?
            stan.SetFormat(stan.Format);
            stan.DeserializeFix();

            if (object.ReferenceEquals(null, stan.Tiles) || !stan.Tiles.Any())
            {
                throw new BadFileFormatException("No tiles found in json");
            }

            foreach (var tile in stan.Tiles)
            {
                if (tile.PointCount > 0 && (tile.Points == null || tile.Points.Count != tile.PointCount))
                {
                    if (tile.Points == null)
                    {
                        throw new BadFileFormatException($"Invalid tile. Declared point count={tile.PointCount}, but tile.{nameof(tile.Points)} is null");
                    }
                    else
                    {
                        throw new BadFileFormatException($"Invalid tile. Declared point count={tile.PointCount}, there are {tile.Points.Count} point(s)");
                    }
                }
            }

            return stan;
        }

        /// <summary>
        /// Converts stan to complete .c text source file.
        /// This uses the regular data structs.
        /// </summary>
        /// <param name="source">Object to convert.</param>
        /// <param name="path">Path of file to write to.</param>
        public static void WriteToC(StandFile source, string path)
        {
            using (var sw = new StreamWriter(path, false))
            {
                source.WriteToCFile(sw);
            }
        }

        /// <summary>
        /// Converts stan to JSON text source file.
        /// Requires <see cref="StandFile.Format"/> to be set to know
        /// which data structs to use..
        /// </summary>
        /// <param name="source">Object to convert.</param>
        /// <param name="path">Path of file to write to.</param>
        /// <remarks>
        /// See <see cref="StanShouldSerializeContractResolver"/>.
        /// </remarks>
        public static void WriteToJson(StandFile source, string path)
        {
            if (source.Format == TypeFormat.DefaultUnknown)
            {
                throw new BadFileFormatException("Type format not set in source object");
            }

            // sync all child formats to base object format.
            source.SetFormat(source.Format);

            var json = JsonConvert.SerializeObject(
                source,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = new StanShouldSerializeContractResolver(),
                });

            File.WriteAllText(path, json);
        }
    }
}
