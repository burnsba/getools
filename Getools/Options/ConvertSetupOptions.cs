using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
    [Verb("convert_setup", HelpText = "Convert setup file/object from one format to another.")]
    public class ConvertSetupOptions : ConvertOptionsBase
    {
        /// <summary>
        /// Capture any remaining command line arguments here.
        /// </summary>
        [Value(0, Hidden = true)]
        public IEnumerable<string> TypoCatch { get; set; }
    }
}
