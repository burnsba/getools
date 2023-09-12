using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Getools.Options;

namespace Getools.Verbs
{
    internal static class VerbExtensions
    {
        /// <summary>
        /// Validates the input filename, and ensures file exists.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="vb">Base.</param>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public static void ValidateSetInputFilename<T>(this VerbBase vb, ParserResult<T> result, IOptionsInputFile opts)
        {
            if (string.IsNullOrEmpty(opts.InputFilename))
            {
                ConsoleColor.ConsoleWriteLineRed($"No input file specified.");

                vb.DisplayHelp(result, null);
                Environment.Exit(1);
            }

            // permute input to reflect run environment
            opts.InputFilename = Path.Combine(System.IO.Directory.GetCurrentDirectory(), opts.InputFilename);

            if (!File.Exists(opts.InputFilename))
            {
                ConsoleColor.ConsoleWriteLineRed($"File not found: {opts.InputFilename}");

                vb.DisplayHelp(result, null);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Validates the output filename.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="vb">Base.</param>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public static void ValidateSetOutputFilename<T>(this VerbBase vb, ParserResult<T> result, IOptionsOutputFile opts)
        {
            if (string.IsNullOrEmpty(opts.OutputFilename))
            {
                ConsoleColor.ConsoleWriteLineRed($"No output file specified.");

                vb.DisplayHelp(result, null);
                Environment.Exit(1);
            }

            // permute output to reflect run environment
            opts.OutputFilename = Path.Combine(System.IO.Directory.GetCurrentDirectory(), opts.OutputFilename);
        }
    }
}
