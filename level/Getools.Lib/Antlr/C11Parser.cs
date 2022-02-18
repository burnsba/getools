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

namespace Getools.Lib.Antlr
{
    /// <summary>
    /// C parser helper class.
    /// </summary>
    public static class C11Parser
    {
        /// <summary>
        /// Loads a text file then parsers according to C11 generated parser.
        /// </summary>
        /// <param name="path">File to parse.</param>
        /// <returns>Parse tree.</returns>
        public static IParseTree ParseC(string path)
        {
            var text = System.IO.File.ReadAllText(path);
            ICharStream stream = CharStreams.fromString(text);
            ITokenSource lexer = new CLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            CParser parser = new CParser(tokens);
            parser.BuildParseTree = true;
            IParseTree tree = parser.compilationUnit();

            return tree;
        }
    }
}
