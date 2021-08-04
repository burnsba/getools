using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Getools.Lib.Antlr;
using Getools.Lib.Antlr.Gen;
using Getools.Lib.Game.Asset.Stan;
using Newtonsoft.Json;

namespace Getools.Lib.Converters
{
    public static class StanConverters
    {
        public static StandFile ParseFromC(string path)
        {
            throw new NotImplementedException();
            //var text = System.IO.File.ReadAllText(path);
            //ICharStream stream = CharStreams.fromString(text);
            //ITokenSource lexer = new CLexer(stream);
            //ITokenStream tokens = new CommonTokenStream(lexer);
            //CParser parser = new CParser(tokens);
            //parser.BuildParseTree = true;
            //IParseTree tree = parser.compilationUnit();

            //CStanListener listener = new CStanListener();
            //ParseTreeWalker.Default.Walk(listener, tree);

            //return listener.Result;
        }

        public static StandFile ReadFromBinFile(string path, string name)
        {
            var result = new StandFile();

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

            return result;
        }

        public static StandFile ReadFromBetaBinFile(string path, string name)
        {
            var result = new StandFile();

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

            return result;
        }

        public static StandFile ReadFromJson(string path)
        {
            var json = File.ReadAllText(path);
            var stan = JsonConvert.DeserializeObject<StandFile>(json);
            return stan;
        }

        public static void WriteToC(StandFile source, string path)
        {
            using (var sw = new StreamWriter(path, false))
            {
                source.WriteToCFile(sw);
            }
        }

        public static void WriteToBetaC(StandFile source, string path)
        {
            using (var sw = new StreamWriter(path, false))
            {
                source.WriteToBetaCFile(sw);
            }
        }

        public static void WriteToBin(StandFile source, string path)
        {
            using (var bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                source.WriteToBinFile(bw);
            }
        }

        public static void WriteToBetaBin(StandFile source, string path)
        {
            using (var bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                source.WriteToBetaBinFile(bw);
            }
        }

        public static void WriteToJson(StandFile source, string path)
        {
            var json = JsonConvert.SerializeObject(source, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
