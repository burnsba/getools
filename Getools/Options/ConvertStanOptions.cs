using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
    /// <summary>
    /// Verb to convert stan file from one format to another.
    /// </summary>
    [Verb("convert_stan", HelpText = "Convert stan file/object from one format to another.")]
    public class ConvertStanOptions : ConvertOptionsBase
    {
        /// <summary>
        /// Gets example verb usage.
        /// </summary>
        [Usage(ApplicationAlias = "getools")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Convert .bin format to .c file", new ConvertStanOptions
                {
                    InputFilename = "Tbg_jun_all_p_stanZ.bin",
                    OutputFilename = "Tbg_jun_all_p_stanZ.c",
                });

                yield return new Example("Convert beta .bin format to (non-beta) .c file", new ConvertStanOptions
                {
                    InputFilename = "Tbg_cat_all_p_stanZ.bin",
                    InputDataFormatIsBeta = true,
                    OutputFilename = "Tbg_cat_all_p_stanZ.c",
                });

                yield return new Example("Parse .c file with beta types, convert to json", new ConvertStanOptions
                {
                    InputFilename = "Tbg_cat_all_p_stanZ.c",
                    InputDataFormatIsBeta = true,
                    OutputFilename = "out",
                    OutputFileTypeString = "json",
                });
            }
        }
    }
}
