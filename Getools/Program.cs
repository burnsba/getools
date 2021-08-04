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
            //var path = "../../../test.c";

            //var stan = StanConverters.ParseFromC(path);

            //StanConverters.WriteToC(stan, "test_write.c");
            //StanConverters.WriteToBin(stan, "test_write.bin");

            //using (var br = new BinaryReader(new FileStream("Tbg_depo_all_p_stanZ.bin", FileMode.Open)))
            //{
            //    var stan_round2 = Getools.Lib.Game.Asset.Stan.StandFile.ReadFromBinFile(br, "Tbg_depo_all_p_stanZ");
            //    StanConverters.WriteToC(stan_round2, "Tbg_depo_all_p_stanZ.c");
            //    StanConverters.WriteToBin(stan_round2, "Tbg_depo_all_p_stanZ_2.bin");
            //}

            //using (var br = new BinaryReader(new FileStream("Tbg_depo_all_p_stanZ_2.bin", FileMode.Open)))
            //{
            //    var stan = Getools.Lib.Game.Asset.Stan.StandFile.ReadFromBinFile(br, "Tbg_depo_all_p_stanZ");
            //    StanConverters.WriteToC(stan, "Tbg_depo_all_p_stanZ_3.c");
            //    StanConverters.WriteToBin(stan, "Tbg_depo_all_p_stanZ_3.bin");
            //}

            //var files = new List<string>()
            //{
            //    "Tbg_azt_all_p_stanZ",
            //    //"Tbg_cat_all_p_stanZ",
            //    "Tbg_crad_all_p_stanZ",
            //    "Tbg_dam_all_p_stanZ",
            //    "Tbg_dest_all_p_stanZ",
            //    "Tbg_pete_all_p_stanZ",
            //    "Tbg_run_all_p_stanZ",
            //    "Tbg_silo_all_p_stanZ",
            //    "Tbg_stat_all_p_stanZ",
            //    "Tbg_tra_all_p_stanZ",
            //};

            //foreach (var file in files)
            //{
            //    using (var br = new BinaryReader(new FileStream(file + ".bin", FileMode.Open)))
            //    {
            //        var stan = Getools.Lib.Game.Asset.Stan.StandFile.ReadFromBinFile(br, file);
            //        StanConverters.WriteToC(stan, file + ".c");
            //        StanConverters.WriteToBin(stan, file + "_check.bin");
            //    }
            //}

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
