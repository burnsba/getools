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
        /// This uses the regular struct definitions.
        /// </summary>
        /// <param name="path">Path of file to read.</param>
        /// <param name="name">Name of header variable.</param>
        /// <returns>Parsed stan.</returns>
        public static StandFile ReadFromBinFile(string path, string name)
        {
            var result = new StandFile(TypeFormat.Normal);

            using (var br = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                result.Header = StandFileHeader.ReadFromBinFile(br, name);

                int tileIndex = 0;
                int safety = UInt16.MaxValue + 1;

                try
                {
                    while (tileIndex < safety)
                    {
                        var tile = StandTile.ReadFromBinFile(br, tileIndex);

                        result.Tiles.Add(tile);

                        tileIndex++;
                    }
                }
                catch (Error.ExpectedStreamEndException)
                {
                }

                result.Footer = StandFileFooter.ReadFromBinFile(br);
            }

            result.DeserializeFix();

            return result;
        }

        /// <summary>
        /// Loads file content and parses as binary file.
        /// This uses the beta struct definitions.
        /// </summary>
        /// <param name="path">Path of file to read.</param>
        /// <param name="name">Name of header variable.</param>
        /// <returns>Parsed stan.</returns>
        public static StandFile ReadFromBetaBinFile(string path, string name)
        {
            var result = new StandFile(TypeFormat.Beta);

            using (var br = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                result.Header = StandFileHeader.ReadFromBetaBinFile(br, name);

                int tileIndex = 0;
                int safety = UInt16.MaxValue + 1;

                try
                {
                    while (tileIndex < safety)
                    {
                        var tile = StandTile.ReadFromBetaBinFile(br, tileIndex);

                        result.Tiles.Add(tile);

                        tileIndex++;
                    }
                }
                catch (Error.ExpectedStreamEndException)
                {
                }

                result.Footer = StandFileFooter.ReadFromBetaBinFile(br);
                result.BetaFooter = BetaFooter.ReadFromBetaBinFile(br);
            }

            result.DeserializeFix();

            return result;
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

            stan.SetFormat(stan.Format);
            stan.DeserializeFix();

            if (object.ReferenceEquals(null, stan.Header))
            {
                throw new BadFileFormatException("Missing header in json");
            }

            if (object.ReferenceEquals(null, stan.Footer))
            {
                throw new BadFileFormatException("Missing footer in json");
            }

            if (object.ReferenceEquals(null, stan.Tiles) || !stan.Tiles.Any())
            {
                throw new BadFileFormatException("No tiles found in json");
            }

            if (stan.Tiles.Any(x => x.Points == null || !x.Points.Any()))
            {
                throw new BadFileFormatException("Invalid json, found tile without any points");
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
        /// Converts stan to complete .c text source file.
        /// This uses the beta data structs.
        /// </summary>
        /// <param name="source">Object to convert.</param>
        /// <param name="path">Path of file to write to.</param>
        public static void WriteToBetaC(StandFile source, string path)
        {
            using (var sw = new StreamWriter(path, false))
            {
                source.WriteToBetaCFile(sw);
            }
        }

        /// <summary>
        /// Converts stan to binary format.
        /// This uses the regular data structs.
        /// </summary>
        /// <param name="source">Object to convert.</param>
        /// <param name="path">Path of file to write to.</param>
        public static void WriteToBin(StandFile source, string path)
        {
            using (var bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                source.WriteToBinFile(bw);
            }
        }

        /// <summary>
        /// Converts stan to binary format.
        /// This uses the beta data structs.
        /// </summary>
        /// <param name="source">Object to convert.</param>
        /// <param name="path">Path of file to write to.</param>
        public static void WriteToBetaBin(StandFile source, string path)
        {
            using (var bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                source.WriteToBetaBinFile(bw);
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
