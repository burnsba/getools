using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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

        /// <summary>
        /// Gets or sets a value indicating whether to use beta structs or not for input.
        /// </summary>
        [Option("input-data-is-beta", Required = false, Default = false, HelpText = "Flag for input to use beta data structures/formats.")]
        public bool? InputDataFormatIsBeta { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use beta structs or not for output.
        /// </summary>
        [Option("output-data-is-beta", Required = false, Default = false, HelpText = "Flag for output to use beta data structures/formats.")]
        public bool? OutputDataFormatIsBeta { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
