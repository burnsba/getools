using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using Getools.Lib.Converters;
using Getools.Options;

namespace Getools
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new CommandLine.Parser(with =>
            {
                with.HelpWriter = null;
            });

            var types = LoadVerbs();
            var parserResult = parser.ParseArguments(args, types);

            parserResult
                .WithParsed(options => PreOptionCheck(parserResult, options))
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));
        }

        private static Type[] LoadVerbs()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
        }

        private static void PreOptionCheck<T>(ParserResult<T> result, object opts)
        {
            switch (opts)
            {
                case ConvertStanOptions cstan:
                    Verbs.ConvertStan.PreOptionCheck(result, cstan);
                    break;

                case ConvertSetupOptions csetup:
                    PreOptionCheck_ConvertSetup(result, csetup);
                    break;

                case MakeMapOptions mmap:
                    PreOptionCheck_MakeMap(result, mmap);
                    break;

                default:
                    throw new NotSupportedException($"Could not resolve type in {nameof(PreOptionCheck)}");
            }
        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            if (result.TypeInfo.Current == typeof(ConvertStanOptions))
            {
                Verbs.ConvertStan.DisplayHelp(result, errs);
                return;
            }
            else if (result.TypeInfo.Current == typeof(ConvertSetupOptions))
            {
                throw new NotImplementedException();
                return;
            }
            else if (result.TypeInfo.Current == typeof(MakeMapOptions))
            {
                throw new NotImplementedException();
                return;
            }

            var helpText = new HelpText(HeadingInfo.Default, CopyrightInfo.Default);
            helpText.AddDashesToOption = true;
            helpText.MaximumDisplayWidth = 100;
            helpText.AdditionalNewLineAfterOption = false;
            helpText.AddOptions(result);
            helpText.AddVerbs(LoadVerbs());

            var texty = helpText.ToString();

            Console.WriteLine(texty);
        }

        

        private static void PreOptionCheck_ConvertSetup<T>(ParserResult<T> result, ConvertSetupOptions opts)
        {
        }

        private static void DisplayHelp_ConvertSetup<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
        }

        private static void PreOptionCheck_MakeMap<T>(ParserResult<T> result, MakeMapOptions opts)
        {
        }

        private static void DisplayHelp_MakeMap<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
        }
    }
}
