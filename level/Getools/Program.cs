using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using Getools.Lib.Converters;
using Getools.Options;
using Getools.Verbs;

namespace Getools
{
    /// <summary>
    /// Main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Program entry.
        /// </summary>
        /// <param name="args">Command line args.</param>
        private static void Main(string[] args)
        {
            var parser = new CommandLine.Parser(with =>
            {
                with.HelpWriter = null;
            });

            var types = LoadVerbs();
            var parserResult = parser.ParseArguments(args, types);

            parserResult
                .WithParsed(options => CheckRun(parserResult, options))
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));
        }

        /// <summary>
        /// Uses reflection to find options verbs available to the program.
        /// </summary>
        /// <returns>List of types.</returns>
        private static Type[] LoadVerbs()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
        }

        /// <summary>
        /// Resolves to the appropriate verb, then validates according to verb and runs program.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        private static void CheckRun<T>(ParserResult<T> result, object opts)
        {
            VerbBase verb = null;

            switch (opts)
            {
                case ConvertStanOptions cstan:
                    verb = new Verbs.ConvertStan();
                    ((ConvertStan)verb).CheckRun(result, cstan);
                    break;

                case ConvertSetupOptions csetup:
                    verb = new Verbs.ConvertSetup();
                    ((ConvertSetup)verb).CheckRun(result, csetup);
                    break;

                case UnzipOptions unzip:
                    verb = new Verbs.Unzip();
                    ((Unzip)verb).CheckRun(result, unzip);
                    break;

                case MakeMapOptions mmap:
                    ////PreOptionCheck_MakeMap(result, mmap);
                    break;

                default:
                    throw new NotSupportedException($"Could not resolve type in {nameof(CheckRun)}");
            }
        }

        /// <summary>
        /// Top level help information. This gets called if no verb is found.
        /// Verb specific help should be handled in its own class.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="errs">Parser errors.</param>
        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            if (errs.IsVersion())
            {
                Console.Write(HeadingInfo.Default);
                return;
            }

            if (result.TypeInfo.Current == typeof(ConvertStanOptions))
            {
                var verb = new Verbs.ConvertStan();
                verb.DisplayHelp(result, errs);
                return;
            }
            else if (result.TypeInfo.Current == typeof(ConvertSetupOptions))
            {
                var verb = new Verbs.ConvertSetup();
                verb.DisplayHelp(result, errs);
                return;
            }
            else if (result.TypeInfo.Current == typeof(UnzipOptions))
            {
                var verb = new Verbs.Unzip();
                verb.DisplayHelp(result, errs);
                return;
            }
            else if (result.TypeInfo.Current == typeof(MakeMapOptions))
            {
                throw new NotImplementedException();
                ////return;
            }

            var helpText = new HelpText(HeadingInfo.Default, CopyrightInfo.Default);
            helpText.MaximumDisplayWidth = 100;
            helpText.AdditionalNewLineAfterOption = false;
            helpText.AddOptions(result);
            helpText.AddVerbs(LoadVerbs());

            var texty = helpText.ToString();

            Console.WriteLine(texty);
        }
    }
}
