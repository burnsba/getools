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
            if (object.ReferenceEquals(null, opts))
            {
                throw new NullReferenceException();
            }

            TypoCheck(result, opts);

            /* begin input description */

            this.ValidateSetInputFilename(result, opts);
            ValidateSetInputFileType(result, opts);
            ValidateSetInputDataFormatIsBeta(result, opts);
            ValidateSetInputTypeFormat(result, opts);
            ValidateSetInputDataFormat(result, opts);

            if (!StandFile.SupportedInputFormats.Contains(opts.InputDataFormat))
            {
                ConsoleColor.ConsoleWriteLineRed($"Input format not supported: file type=\"{opts.InputFileTypeString}\", beta=\"{opts.InputDataFormatIsBeta ?? false}\"");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }

            /* done with input description */

            /* begin output description */

            this.ValidateSetOutputFilename(result, opts);
            ValidateSetOutputFileType(result, opts);
            ValidateSetOutputDataFormatIsBeta(result, opts);
            ValidateSetOutputTypeFormat(result, opts);
            ValidateSetOutputDataFormat(result, opts);

            if (!StandFile.SupportedOutputFormats.Contains(opts.OutputDataFormat))
            {
                ConsoleColor.ConsoleWriteLineRed($"Output format not supported: file type=\"{opts.OutputFileTypeString}\", beta=\"{opts.OutputDataFormatIsBeta ?? false}\"");

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
        public override void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error>? errs)
        {
            HelpBaseError(result, errs);

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

        /// <summary>
        /// Sets <see cref="ConvertOptionsBase.InputTypeFormat"/> based on <see cref="ConvertOptionsBase.InputDataFormatIsBeta"/>.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public override void ValidateSetInputTypeFormat<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            base.ValidateSetInputTypeFormat(result, opts);

            var thisOpts = (ConvertStanOptions)opts;
            thisOpts.InputTypeFormat = (thisOpts.InputDataFormatIsBeta == true) ? Lib.Game.TypeFormat.Beta : Lib.Game.TypeFormat.Normal;
        }

        /// <summary>
        /// Sets <see cref="ConvertOptionsBase.OutputTypeFormat"/> based on <see cref="ConvertOptionsBase.OutputDataFormatIsBeta"/>.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public override void ValidateSetOutputTypeFormat<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            base.ValidateSetInputTypeFormat(result, opts);

            var thisOpts = (ConvertStanOptions)opts;
            thisOpts.OutputTypeFormat = (thisOpts.OutputDataFormatIsBeta == true) ? Lib.Game.TypeFormat.Beta : Lib.Game.TypeFormat.Normal;
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
            var thisOpts = (ConvertStanOptions)opts;

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

                    if (object.ReferenceEquals(null, stan.Header))
                    {
                        ConsoleColor.ConsoleWriteLineRed($"Error parsing file. stan.Header is null.");
                        Environment.Exit(1);
                    }

                    stan.Header.Name = opts.DeclarationName;
                    break;

                case Lib.Game.DataFormats.BetaC:
                    stan = StanConverters.ParseFromBetaC(opts.InputFilename);

                    if (object.ReferenceEquals(null, stan.Header))
                    {
                        ConsoleColor.ConsoleWriteLineRed($"Error parsing file. stan.Header is null.");
                        Environment.Exit(1);
                    }

                    stan.Header.Name = opts.DeclarationName;
                    break;

                case Lib.Game.DataFormats.Json:
                    stan = StanConverters.ReadFromJson(opts.InputFilename);
                    break;

                default:
                    ConsoleColor.ConsoleWriteLineRed($"Input format not supported: file type=\"{thisOpts.InputFileTypeString}\", beta=\"{thisOpts.InputDataFormatIsBeta ?? false}\"");
                    Environment.Exit(1);
                    return;
            }

            switch (opts.OutputDataFormat)
            {
                case Lib.Game.DataFormats.Bin:
                    stan.SetFormat(Lib.Game.TypeFormat.Normal);
                    StanConverters.WriteToBin(stan, opts.OutputFilename);
                    break;

                case Lib.Game.DataFormats.BetaBin:
                    stan.SetFormat(Lib.Game.TypeFormat.Beta);
                    StanConverters.WriteToBin(stan, opts.OutputFilename);
                    break;

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
                    ConsoleColor.ConsoleWriteLineRed($"Output format not supported: file type=\"{thisOpts.OutputFileTypeString}\", beta=\"{thisOpts.OutputDataFormatIsBeta ?? false}\"");
                    Environment.Exit(1);
                    return;
            }
        }

        /// <summary>
        /// Validates/sets the beta flag.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        private void ValidateSetInputDataFormatIsBeta<T>(ParserResult<T> result, ConvertStanOptions opts)
        {
            // promote null to false.
            if (opts.InputDataFormatIsBeta != true)
            {
                opts.InputDataFormatIsBeta = false;
            }
        }

        /// <summary>
        /// Validates/sets the beta flag.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        private void ValidateSetOutputDataFormatIsBeta<T>(ParserResult<T> result, ConvertStanOptions opts)
        {
            // promote null to false.
            if (opts.OutputDataFormatIsBeta != true)
            {
                opts.OutputDataFormatIsBeta = false;
            }
        }
    }
}
