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


namespace Getools.Verbs
{
    public static class ConvertStan
    {
        public static void PreOptionCheck<T>(ParserResult<T> result, ConvertStanOptions opts)
        {
            if (!object.ReferenceEquals(null, opts.TypoCatch) && opts.TypoCatch.Any())
            {
                if (opts.TypoCatch.Any(x => (x?.Trim()?.ToLower() ?? string.Empty) == "help"))
                {
                    DisplayHelp(result, null);
                    return;
                }

                Console.Error.WriteLine($"The following options are not supported (try --help):");
                foreach (var line in opts.TypoCatch)
                {
                    Console.Error.WriteLine(line);
                }

                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(opts.InputFilename))
            {
                Console.Error.WriteLine($"No input file specified.");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }

            if (!File.Exists(opts.InputFilename))
            {
                Console.Error.WriteLine($"File not found: {opts.InputFilename}");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(opts.InputFormatString))
            {
                Console.Error.WriteLine($"No input format specified.");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }

            Getools.Lib.Game.DataFormats df;
            if (Enum.TryParse<Getools.Lib.Game.DataFormats>(opts.InputFormatString, ignoreCase:true, out df))
            {
                opts.InputFormat = df;
            }

            if (!Getools.Lib.Game.Config.Stan.SupportedInputFormats.Contains(opts.InputFormat))
            {
                Console.Error.WriteLine($"Input format not supported: {opts.InputFormatString}");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }

            if (Enum.TryParse<Getools.Lib.Game.DataFormats>(opts.OutputFormatString, ignoreCase: true, out df))
            {
                opts.OutputFormat = df;
            }

            if (!Getools.Lib.Game.Config.Stan.SupportedOutputFormats.Contains(opts.OutputFormat))
            {
                Console.Error.WriteLine($"Output format not supported: {opts.OutputFormatString}");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(opts.DeclarationName))
            {
                opts.DeclarationName = Path.GetFileNameWithoutExtension(opts.InputFilename);
            }

            Convert(opts);
        }

        public static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = new HelpText(HeadingInfo.Default, CopyrightInfo.Default);
            helpText.AddDashesToOption = true;
            helpText.MaximumDisplayWidth = 100;
            helpText.AdditionalNewLineAfterOption = false;
            helpText.AutoVersion = false;
            helpText.AddOptions(result);
            helpText.AddPostOptionsLines(new List<string>()
            {
                "The following values are supported for \"IN_FORMAT\":",
                GetInFormatNames(),
                "\n",
                "The following values are supported for \"OUT_FORMAT\"",
                GetOutFormatNames(),
            });

            var texty = helpText.ToString();

            Console.WriteLine(texty);
        }

        private static string GetInFormatNames()
        {
            return string.Join(", ", Getools.Lib.Game.Config.Stan.SupportedInputFormats);
        }

        private static string GetOutFormatNames()
        {
            return string.Join(", ", Getools.Lib.Game.Config.Stan.SupportedInputFormats);
        }

        private static void Convert(ConvertStanOptions opts)
        {
            StandFile stan;

            switch (opts.InputFormat)
            {
                case Lib.Game.DataFormats.Bin:
                    stan = StanConverters.ReadFromBinFile(opts.InputFilename, opts.DeclarationName);
                    break;

                case Lib.Game.DataFormats.BetaBin:
                    stan = StanConverters.ReadFromBetaBinFile(opts.InputFilename, opts.DeclarationName);
                    break;

                case Lib.Game.DataFormats.C:
                    stan = StanConverters.ParseFromC(opts.InputFilename);
                    stan.Header.Name = opts.DeclarationName;
                    break;

                case Lib.Game.DataFormats.BetaC:
                    stan = StanConverters.ParseFromBetaC(opts.InputFilename);
                    stan.Header.Name = opts.DeclarationName;
                    break;

                case Lib.Game.DataFormats.Json:
                    stan = StanConverters.ReadFromJson(opts.InputFilename);
                    break;

                default:
                    throw new Exception($"Input format not supported: {opts.InputFormat}");
            }

            switch (opts.OutputFormat)
            {
                case Lib.Game.DataFormats.Bin:
                    StanConverters.WriteToBin(stan, opts.OutputFilename);
                    break;

                case Lib.Game.DataFormats.BetaBin:
                    StanConverters.WriteToBetaBin(stan, opts.OutputFilename);
                    break;

                case Lib.Game.DataFormats.C:
                    StanConverters.WriteToC(stan, opts.OutputFilename);
                    break;

                case Lib.Game.DataFormats.BetaC:
                    StanConverters.WriteToBetaC(stan, opts.OutputFilename);
                    break;

                case Lib.Game.DataFormats.Json:
                    StanConverters.WriteToJson(stan, opts.OutputFilename);
                    break;

                default:
                    throw new Exception($"Output format not supported: {opts.OutputFormat}");
            }
        }
    }
}
