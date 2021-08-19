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
    /// <summary>
    /// Program command verb convert_stan.
    /// </summary>
    public class ConvertStan : ConvertBase
    {
        /// <summary>
        /// Validates the command line arguments then runs the program.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public void CheckRun<T>(ParserResult<T> result, ConvertStanOptions opts)
        {
            TypoCheck(result, opts);

            /* begin input description */

            ValidateSetInputFilename(result, opts);
            ValidateSetInputFileType(result, opts);
            ValidateSetInputDataFormatIsBeta(result, opts);
            ValidateSetInputTypeFormat(result, opts);
            ValidateSetInputDataFormat(result, opts);

            if (!StandFile.SupportedInputFormats.Contains(opts.InputDataFormat))
            {
                ConsoleColor.ConsoleWriteLineRed($"Input format not supported: file type=\"{opts.InputFileTypeString}\", beta=\"{opts.InputDataFormatIsBeta.Value}\"");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }

            /* done with input description */

            /* begin output description */

            ValidateSetOutputFilename(result, opts);
            ValidateSetOutputFileType(result, opts);
            ValidateSetOutputDataFormatIsBeta(result, opts);
            ValidateSetOutputTypeFormat(result, opts);
            ValidateSetOutputDataFormat(result, opts);

            if (!StandFile.SupportedOutputFormats.Contains(opts.OutputDataFormat))
            {
                ConsoleColor.ConsoleWriteLineRed($"Output format not supported: file type=\"{opts.OutputFileTypeString}\", beta=\"{opts.OutputDataFormatIsBeta.Value}\"");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }

            /* done with output description */

            if (string.IsNullOrEmpty(opts.DeclarationName))
            {
                opts.DeclarationName = Path.GetFileNameWithoutExtension(opts.InputFilename);
            }

            Convert(opts);
        }

        /// <summary>
        /// Verb specific help.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="errs">Parser errors.</param>
        public override void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var errorLines = new List<string>();

            if (result is NotParsed<T>)
            {
                var np = result as NotParsed<T>;
                if (np.Errors.Any())
                {
                    var missingRequired = np.Errors.Where(x => x is MissingRequiredOptionError).Cast<MissingRequiredOptionError>();

                    foreach (var missing in missingRequired)
                    {
                        errorLines.Add($"Error: missing required option: {missing.NameInfo.LongName}");
                    }
                }
            }

            var unknownOptionErrors = errs.Where(x => x is UnknownOptionError).Cast<UnknownOptionError>();
            foreach (var uoe in unknownOptionErrors)
            {
                errorLines.Add($"Error: unknown option: {uoe.Token}");
            }

            foreach (var error in errorLines)
            {
                ConsoleColor.ConsoleWriteLineRed(error);
            }

            var helpText = new HelpText(HeadingInfo.Default, CopyrightInfo.Default);
            helpText.AddDashesToOption = true;
            helpText.MaximumDisplayWidth = 100;
            helpText.AdditionalNewLineAfterOption = false;
            helpText.AutoVersion = false;

            helpText.AddPreOptionsLine("EXAMPLES: ");
            helpText.AddPreOptionsLine(string.Empty);
            helpText.AddPreOptionsLine(HelpText.RenderUsageText(result));

            helpText.AddPreOptionsLine(string.Empty);
            helpText.AddPreOptionsLine("USAGE: ");
            helpText.AddOptions(result);

            helpText.AddPostOptionsLines(new List<string>()
            {
                "The following values are supported for input \"FTYPE\":",
                GetInFormatNames(),
                "\n",
                "The following values are supported for output \"FTYPE\"",
                GetOutFormatNames(),
            });

            var texty = helpText.ToString();

            Console.WriteLine(texty);
        }

        private static string GetInFormatNames()
        {
            var dataformats = StandFile.SupportedInputFormats;
            var fileTypes = Getools.Lib.Converters.FormatConverter.ToKnownFileTypes(dataformats);
            return string.Join(", ", fileTypes);
        }

        private static string GetOutFormatNames()
        {
            var dataformats = StandFile.SupportedOutputFormats;
            var fileTypes = Getools.Lib.Converters.FormatConverter.ToKnownFileTypes(dataformats);
            return string.Join(", ", fileTypes);
        }

        private void Convert(ConvertOptionsBase opts)
        {
            StandFile stan;

            switch (opts.InputDataFormat)
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
                    ConsoleColor.ConsoleWriteLineRed($"Input format not supported: file type=\"{opts.InputFileTypeString}\", beta=\"{opts.InputDataFormatIsBeta.Value}\"");
                    Environment.Exit(1);
                    return;
            }

            switch (opts.OutputDataFormat)
            {
                //case Lib.Game.DataFormats.Bin:
                //    StanConverters.WriteToBin(stan, opts.OutputFilename);
                //    break;

                //case Lib.Game.DataFormats.BetaBin:
                //    StanConverters.WriteToBetaBin(stan, opts.OutputFilename);
                //    break;

                case Lib.Game.DataFormats.C:
                    stan.SetFormat(Lib.Game.TypeFormat.Normal);
                    StanConverters.WriteToC(stan, opts.OutputFilename);
                    break;

                case Lib.Game.DataFormats.BetaC:
                    stan.SetFormat(Lib.Game.TypeFormat.Beta);
                    StanConverters.WriteToC(stan, opts.OutputFilename);
                    break;

                case Lib.Game.DataFormats.Json:
                    StanConverters.WriteToJson(stan, opts.OutputFilename);
                    break;

                default:
                    ConsoleColor.ConsoleWriteLineRed($"Output format not supported: file type=\"{opts.OutputFileTypeString}\", beta=\"{opts.OutputDataFormatIsBeta.Value}\"");
                    Environment.Exit(1);
                    return;
            }
        }
    }
}
