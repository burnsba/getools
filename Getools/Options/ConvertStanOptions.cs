using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
    [Verb("convert_stan", HelpText = "Convert stan file/object from one format to another.")]
    public class ConvertStanOptions : ConvertOptionsBase
    {
        /// <summary>
        /// Capture any remaining command line arguments here.
        /// </summary>
        [Value(0, Hidden = true)]
        public IEnumerable<string> TypoCatch { get; set; }

        public Getools.Lib.Game.DataFormats InputFormat { get; set; }
        public Getools.Lib.Game.DataFormats OutputFormat { get; set; }
    }
}
