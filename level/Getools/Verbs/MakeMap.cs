using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Getools.Lib.Converters;
using Getools.Lib.Game.Asset.Bg;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.Stan;
using Getools.Options;
using SvgLib;
using static System.Net.Mime.MediaTypeNames;
using static SvgLib.SvgDefaults.Attributes;

namespace Getools.Verbs
{
    public class MakeMap : VerbBase
    {
        /// <summary>
        /// Validates the command line arguments then runs the program.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public void CheckRun<T>(ParserResult<T> result, MakeMapOptions opts)
        {
            TypoCheck(result, opts);

            /* begin input description */

            if (string.IsNullOrEmpty(opts.StanFilename)
                && string.IsNullOrEmpty(opts.SetupFilename)
                && string.IsNullOrEmpty(opts.BgFilename))
            {
                ConsoleColor.ConsoleWriteLineRed($"At least one of stan/setup/bg need to be specified");

                this.DisplayHelp(result, null);
                Environment.Exit(1);
            }

            // stan
            {
                var check = InputComboCheck(opts.StanFilename, result);
                opts.StanFilename = check.Item1;
                opts.StanFileType = check.Item2;
                opts.StanTypeFormat = check.Item3;
                opts.StanDataFormat = check.Item4;

                if (opts.StanDataFormatIsBeta)
                {
                    opts.StanDataFormat = Lib.Game.DataFormats.BetaBin;
                }
            }

            // setup
            {
                var check = InputComboCheck(opts.SetupFilename, result);
                opts.SetupFilename = check.Item1;
                opts.SetupFileType = check.Item2;
                opts.SetupTypeFormat = check.Item3;
                opts.SetupDataFormat = check.Item4;
            }

            // bg
            {
                var check = InputComboCheck(opts.BgFilename, result);
                opts.BgFilename = check.Item1;
                opts.BgFileType = check.Item2;
                opts.BgTypeFormat = check.Item3;
                opts.BgDataFormat = check.Item4;
            }

            if (opts.SlizeZ.HasValue)
            {
                opts.ZMin = null;
                opts.ZMax = null;
            }

            if (opts.ZMax.HasValue && !opts.ZMin.HasValue)
            {
                opts.ZMin = -1e20;
            }

            if (opts.ZMin.HasValue && !opts.ZMax.HasValue)
            {
                opts.ZMax = 1e20;
            }

            /* done with input description */

            /* begin output description */

            this.ValidateSetOutputFilename(result, opts);

            /* done with output description */

            Go(opts);
        }

        private (string, Getools.Lib.Game.FileType, Getools.Lib.Game.TypeFormat, Getools.Lib.Game.DataFormats) InputComboCheck<T>(string filename, ParserResult<T> result)
        {
            string r1 = filename;
            Getools.Lib.Game.FileType r2 = Lib.Game.FileType.DefaultUnknown;
            Getools.Lib.Game.TypeFormat r3 = Lib.Game.TypeFormat.DefaultUnknown;
            Getools.Lib.Game.DataFormats r4 = Lib.Game.DataFormats.DefaultUnknown;

            if (!string.IsNullOrEmpty(filename))
            {
                // permute input to reflect run environment
                r1 = Path.Combine(System.IO.Directory.GetCurrentDirectory(), filename);

                if (!File.Exists(r1))
                {
                    ConsoleColor.ConsoleWriteLineRed($"File not found: {r1}");

                    this.DisplayHelp(result, null);
                    Environment.Exit(1);
                }

                var ext = Path.GetExtension(r1);
                if (ext.Length > 1)
                {
                    ext = ext.Substring(1);
                }

                Getools.Lib.Game.FileType ft;
                if (Enum.TryParse<Getools.Lib.Game.FileType>(ext, ignoreCase: true, out ft))
                {
                    r2 = ft;
                }

                r3 = Lib.Game.TypeFormat.Normal;
                r4 = Getools.Lib.Converters.FormatConverter.GetDataFormat(r3, r2);

                if (!StandFile.SupportedInputFormats.Contains(r4))
                {
                    ConsoleColor.ConsoleWriteLineRed($"Input format not supported: file type=\"{ext}\"");

                    DisplayHelp(result, null);
                    Environment.Exit(1);
                }
            }

            return (r1, r2, r3, r4);
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

            var texty = helpText.ToString();

            Console.WriteLine(texty);
        }

        private void Go(MakeMapOptions opts)
        {
            StageSetupFile setup = null;
            StandFile stan = null;
            BgFile bg = null;

            if (!string.IsNullOrEmpty(opts.SetupFilename))
            {
                Console.WriteLine($"load setup: {opts.SetupFilename}");

                switch (opts.SetupDataFormat)
                {
                    case Lib.Game.DataFormats.Bin:
                        setup = SetupConverters.ReadFromBinFile(opts.SetupFilename);
                        break;

                    case Lib.Game.DataFormats.Json:
                        setup = SetupConverters.ReadFromJson(opts.SetupFilename);
                        break;

                    default:
                        ConsoleColor.ConsoleWriteLineRed($"Input format not supported: file type=\"{opts.SetupFilename}\"");
                        Environment.Exit(1);
                        return;
                }
            }

            if (!string.IsNullOrEmpty(opts.StanFilename))
            {
                Console.WriteLine($"load stan: {opts.StanFilename}");

                switch (opts.StanDataFormat)
                {
                    case Lib.Game.DataFormats.Bin:
                        stan = StanConverters.ReadFromBinFile(opts.StanFilename, "ignore");
                        break;

                    case Lib.Game.DataFormats.BetaBin:
                        stan = StanConverters.ReadFromBetaBinFile(opts.StanFilename, "ignore");
                        break;

                    case Lib.Game.DataFormats.C:
                        stan = StanConverters.ParseFromC(opts.StanFilename);
                        stan.Header.Name = "ignore";
                        break;

                    case Lib.Game.DataFormats.BetaC:
                        stan = StanConverters.ParseFromBetaC(opts.StanFilename);
                        stan.Header.Name = "ignore";
                        break;

                    case Lib.Game.DataFormats.Json:
                        stan = StanConverters.ReadFromJson(opts.StanFilename);
                        break;

                    default:
                        ConsoleColor.ConsoleWriteLineRed($"Input format not supported: file type=\"{opts.StanFilename}\"");
                        Environment.Exit(1);
                        return;
                }
            }

            if (!string.IsNullOrEmpty(opts.BgFilename))
            {
                Console.WriteLine($"load bg: {opts.BgFilename}");

                switch (opts.BgDataFormat)
                {
                    case Lib.Game.DataFormats.Bin:
                        bg = BgConverters.ReadFromBinFile(opts.BgFilename);
                        break;

                    default:
                        ConsoleColor.ConsoleWriteLineRed($"Input format not supported: file type=\"{opts.SetupFilename}\"");
                        Environment.Exit(1);
                        return;
                }
            }

            var stage = new Palantir.Stage()
            {
                Bg = bg,
                Setup = setup,
                Stan = stan,
                LevelScale = opts.LevelScale,
            };

            var mim = new Palantir.MapImageMaker(stage);

            SvgDocument svg;

            if (opts.SlizeZ.HasValue)
            {
                svg = mim.SliceZToImageSvg(opts.SlizeZ.Value);
            }
            else if (opts.ZMin.HasValue)
            {
                svg = mim.BoundingZToImageSvg(opts.ZMin.Value, opts.ZMax!.Value);
            }
            else
            {
                svg = mim.FullImageSvg();
            }

            using (var fs = new FileStream(opts.OutputFilename, FileMode.Create))
            {
                Console.WriteLine($"write output file: {opts.OutputFilename}");
                svg.Save(fs);
            }
        }
    }
}
