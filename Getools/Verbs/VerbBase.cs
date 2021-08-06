using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using Getools.Options;

namespace Getools.Verbs
{
    public abstract class VerbBase
    {
        public void TypoCheck<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            if (!object.ReferenceEquals(null, opts.TypoCatch) && opts.TypoCatch.Any())
            {
                if (opts.TypoCatch.Any(x => (x?.Trim()?.ToLower() ?? string.Empty) == "help"))
                {
                    DisplayHelp(result, null);
                    return;
                }

                ConsoleColor.ConsoleWriteLineRed($"The following options are not supported (try --help):");
                foreach (var line in opts.TypoCatch)
                {
                    Console.Error.WriteLine(line);
                }

                Environment.Exit(1);
            }
        }

        public abstract void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs);
    }
}
