using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using Getools.Options;

namespace Getools.Verbs
{
    public abstract class ConvertBase : VerbBase
    {
        public void ValidateSetInputFilename<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            if (string.IsNullOrEmpty(opts.InputFilename))
            {
                ConsoleColor.ConsoleWriteLineRed($"No input file specified.");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }

            if (!File.Exists(opts.InputFilename))
            {
                ConsoleColor.ConsoleWriteLineRed($"File not found: {opts.InputFilename}");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }
        }

        public void ValidateSetInputFileType<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            // attempt to fall back to file extension
            if (string.IsNullOrEmpty(opts.InputFileTypeString))
            {
                var ext = Path.GetExtension(opts.InputFilename);
                if (ext.Length > 1)
                {
                    ext = ext.Substring(1);
                }

                opts.InputFileTypeString = ext;
            }

            Getools.Lib.Game.FileType ft;
            if (Enum.TryParse<Getools.Lib.Game.FileType>(opts.InputFileTypeString, ignoreCase: true, out ft))
            {
                opts.InputFileType = ft;
            }
        }

        public void ValidateSetInputDataFormatIsBeta<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            // promote null to false.
            if (opts.InputDataFormatIsBeta != true)
            {
                opts.InputDataFormatIsBeta = false;
            }
        }

        public void ValidateSetInputTypeFormat<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            opts.InputTypeFormat = (opts.InputDataFormatIsBeta == true) ? Lib.Game.TypeFormat.Beta : Lib.Game.TypeFormat.Normal;
        }

        public void ValidateSetInputDataFormat<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            opts.InputDataFormat = Getools.Lib.Converters.FormatConverter.GetDataFormat(opts.InputTypeFormat, opts.InputFileType);
        }

        public void ValidateSetOutputFilename<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            if (string.IsNullOrEmpty(opts.OutputFilename))
            {
                ConsoleColor.ConsoleWriteLineRed($"No output file specified.");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }
        }

        public void ValidateSetOutputFileType<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            // attempt to fall back to file extension
            if (string.IsNullOrEmpty(opts.OutputFileTypeString))
            {
                var ext = Path.GetExtension(opts.OutputFilename);
                if (ext.Length > 1)
                {
                    ext = ext.Substring(1);
                }

                opts.OutputFileTypeString = ext;
            }

            Getools.Lib.Game.FileType ft;
            if (Enum.TryParse<Getools.Lib.Game.FileType>(opts.OutputFileTypeString, ignoreCase: true, out ft))
            {
                opts.OutputFileType = ft;
            }
        }

        public void ValidateSetOutputDataFormatIsBeta<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            // promote null to false.
            if (opts.OutputDataFormatIsBeta != true)
            {
                opts.OutputDataFormatIsBeta = false;
            }
        }

        public void ValidateSetOutputTypeFormat<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            opts.OutputTypeFormat = (opts.OutputDataFormatIsBeta == true) ? Lib.Game.TypeFormat.Beta : Lib.Game.TypeFormat.Normal;
        }

        public void ValidateSetOutputDataFormat<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            opts.OutputDataFormat = Getools.Lib.Converters.FormatConverter.GetDataFormat(opts.OutputTypeFormat, opts.OutputFileType);
        }
    }
}
