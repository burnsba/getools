using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using Getools.Lib.Converters;
using Getools.Lib.Game.Asset.Stan;
using Getools.Options;
using GzipSharpLib;

namespace Getools.Verbs
{
    /// <summary>
    /// Program command verb unzip.
    /// Used to inflate regular gzip file, or Rare 1172 compressed binary file.
    /// </summary>
    public class Unzip : VerbBase
    {
        /// <summary>
        /// Validates the command line arguments then runs the program.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public void CheckRun<T>(ParserResult<T> result, UnzipOptions opts)
        {
            TypoCheck(result, opts);

            /* begin input description */

            this.ValidateSetInputFilename(result, opts);

            /* done with input description */

            /* begin output description */

            this.ValidateSetOutputFilename(result, opts);

            /* done with output description */

            DoInflate(opts);
        }

        /// <summary>
        /// Verb specific help.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="errs">Parser errors.</param>
        public override void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error>? errs)
        {
            HelpBaseError(result, errs);

            var helpText = new HelpText(HeadingInfo.Default, CopyrightInfo.Default);
            helpText.AddDashesToOption = true;
            helpText.MaximumDisplayWidth = 100;
            helpText.AdditionalNewLineAfterOption = false;
            helpText.AutoVersion = false;

            helpText.AddPreOptionsLine(HelpText.RenderUsageText(result));

            helpText.AddPreOptionsLine(string.Empty);
            helpText.AddPreOptionsLine("USAGE: ");
            helpText.AddOptions(result);

            var texty = helpText.ToString();

            Console.WriteLine(texty);
        }

        private void DoInflate(UnzipOptions opts)
        {
            var logger = new Getools.Utility.Logging.Logger();

            var gzipContext = new Context(logger);

            gzipContext.Trace = opts.Trace;

            var sourcePath = opts.InputFilename;
            var destPath = opts.OutputFilename;

            ReturnCode result;

            using (var ms = new MemoryStream(System.IO.File.ReadAllBytes(sourcePath)))
            {
                gzipContext.Source = ms;
                result = gzipContext.Execute();
            }

            if (result == ReturnCode.Ok)
            {
                if (object.ReferenceEquals(null, gzipContext.Destination))
                {
                    throw new NullReferenceException("Destination not set.");
                }

                var contentResult = gzipContext.Destination.ToArray();

                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                }

                System.IO.File.WriteAllBytes(destPath, contentResult);
            }

            Environment.Exit((int)result);
        }
    }
}
