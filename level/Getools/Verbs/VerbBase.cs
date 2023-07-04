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
    /// <summary>
    /// Common command line option verb base class.
    /// </summary>
    public abstract class VerbBase
    {
        /// <summary>
        /// Check if any unknown options were collected. Print an error message then exit the application.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public void TypoCheck<T>(ParserResult<T> result, IOptionsBase opts)
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

        /// <summary>
        /// VVerb specific help text.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="errs">Parser errors.</param>
        public abstract void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error>? errs);

        protected void HelpBaseError<T>(ParserResult<T> result, IEnumerable<Error>? errs)
        {
            var errorLines = new List<string>();

            if (result is NotParsed<T>)
            {
                var np = result as NotParsed<T>;
                if (np!.Errors.Any())
                {
                    var missingRequired = np.Errors.Where(x => x is MissingRequiredOptionError).Cast<MissingRequiredOptionError>();

                    foreach (var missing in missingRequired)
                    {
                        errorLines.Add($"Error: missing required option: {missing.NameInfo.LongName}");
                    }
                }
            }

            if (!object.ReferenceEquals(null, errs))
            {
                var unknownOptionErrors = errs.Where(x => x is UnknownOptionError).Cast<UnknownOptionError>();
                foreach (var uoe in unknownOptionErrors)
                {
                    errorLines.Add($"Error: unknown option: {uoe.Token}");
                }
            }

            foreach (var error in errorLines)
            {
                ConsoleColor.ConsoleWriteLineRed(error);
            }
        }
    }
}
