using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using Getools.Lib.Converters;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.Stan;
using Getools.Options;

namespace Getools.Verbs
{
    /// <summary>
    /// Program command verb convert_stan.
    /// </summary>
    public class ConvertSetup : ConvertBase
    {
        /// <summary>
        /// Validates the command line arguments then runs the program.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public void CheckRun<T>(ParserResult<T> result, ConvertSetupOptions opts)
        {
            TypoCheck(result, opts);

            /* begin input description */

            ValidateSetInputFilename(result, opts);
            ValidateSetInputFileType(result, opts);
            ValidateSetInputTypeFormat(result, opts);
            ValidateSetInputDataFormat(result, opts);

            if (!StandFile.SupportedInputFormats.Contains(opts.InputDataFormat))
            {
                ConsoleColor.ConsoleWriteLineRed($"Input format not supported: file type=\"{opts.InputFileTypeString}\"");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }

            /* done with input description */

            /* begin output description */

            ValidateSetOutputFilename(result, opts);
            ValidateSetOutputFileType(result, opts);
            ValidateSetOutputTypeFormat(result, opts);
            ValidateSetOutputDataFormat(result, opts);

            if (!StandFile.SupportedOutputFormats.Contains(opts.OutputDataFormat))
            {
                ConsoleColor.ConsoleWriteLineRed($"Output format not supported: file type=\"{opts.OutputFileTypeString}\"");

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
            var dataformats = StageSetupFile.SupportedInputFormats;
            var fileTypes = Getools.Lib.Converters.FormatConverter.ToKnownFileTypes(dataformats);
            return string.Join(", ", fileTypes);
        }

        private static string GetOutFormatNames()
        {
            var dataformats = StageSetupFile.SupportedOutputFormats;
            var fileTypes = Getools.Lib.Converters.FormatConverter.ToKnownFileTypes(dataformats);
            return string.Join(", ", fileTypes);
        }

        private void Convert(ConvertOptionsBase opts)
        {
            StageSetupFile setup;

            switch (opts.InputDataFormat)
            {
                case Lib.Game.DataFormats.Bin:
                    setup = SetupConverters.ReadFromBinFile(opts.InputFilename);
                    break;

                case Lib.Game.DataFormats.Json:
                    setup = SetupConverters.ReadFromJson(opts.InputFilename);
                    break;

                default:
                    ConsoleColor.ConsoleWriteLineRed($"Input format not supported: file type=\"{opts.InputFileTypeString}\"");
                    Environment.Exit(1);
                    return;
            }

            if (!string.IsNullOrEmpty(opts.DeclarationName))
            {
                setup.Name = opts.DeclarationName;
            }

            switch (opts.OutputDataFormat)
            {
                case Lib.Game.DataFormats.C:
                    SetupConverters.WriteToC(setup, opts.OutputFilename);
                    break;

                case Lib.Game.DataFormats.Json:
                    SetupConverters.WriteToJson(setup, opts.OutputFilename);
                    break;

                default:
                    ConsoleColor.ConsoleWriteLineRed($"Output format not supported: file type=\"{opts.OutputFileTypeString}\"");
                    Environment.Exit(1);
                    return;
            }
        }
    }
}
