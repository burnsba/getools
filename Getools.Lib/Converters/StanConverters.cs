using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Getools.Lib.Antlr;
using Getools.Lib.Antlr.Gen;
using Getools.Lib.Game.Asset.Stan;

namespace Getools.Lib.Converters
{
    public static class StanConverters
    {
        public static StandFile ParseFromC(string path)
        {
            var text = System.IO.File.ReadAllText(path);
            ICharStream stream = CharStreams.fromString(text);
            ITokenSource lexer = new CLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            CParser parser = new CParser(tokens);
            parser.BuildParseTree = true;
            IParseTree tree = parser.compilationUnit();

            CStanListener listener = new CStanListener();
            ParseTreeWalker.Default.Walk(listener, tree);

            return listener.Result;
        }

        public static void WriteToC(StandFile source, string path)
        {
            using (var sw = new StreamWriter(path, false))
            {
                source.WriteToCFile(sw);
            }
        }

        public static void WriteToBin(StandFile source, string path)
        {
            using (var bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                source.WriteToBinFile(bw);
            }
        }
    }
}
