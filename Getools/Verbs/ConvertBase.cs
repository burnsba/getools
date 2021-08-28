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
    /// <summary>
    /// Common base class for convert verb.
    /// </summary>
    public abstract class ConvertBase : VerbBase
    {
        /// <summary>
        /// Validates the input filename, and ensures file exists.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
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

        /// <summary>
        /// Validates/sets input file type. If not set, attempts to resolve
        /// based on file extension.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
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

        /// <summary>
        /// Sets <see cref="ConvertOptionsBase.InputTypeFormat"/> based on <see cref="ConvertOptionsBase.InputDataFormatIsBeta"/>.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public virtual void ValidateSetInputTypeFormat<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            opts.InputTypeFormat = Lib.Game.TypeFormat.Normal;
        }

        /// <summary>
        /// Sets <see cref="ConvertOptionsBase.InputDataFormat"/> based on <see cref="ConvertOptionsBase.InputTypeFormat"/> and <see cref="ConvertOptionsBase.InputFileType"/>.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public void ValidateSetInputDataFormat<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            opts.InputDataFormat = Getools.Lib.Converters.FormatConverter.GetDataFormat(opts.InputTypeFormat, opts.InputFileType);
        }

        /// <summary>
        /// Validates the output filename.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public void ValidateSetOutputFilename<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            if (string.IsNullOrEmpty(opts.OutputFilename))
            {
                ConsoleColor.ConsoleWriteLineRed($"No output file specified.");

                DisplayHelp(result, null);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Validates/sets output file type. If not set, attempts to resolve
        /// based on file extension.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
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

        /// <summary>
        /// Sets <see cref="ConvertOptionsBase.OutputTypeFormat"/> based on <see cref="ConvertOptionsBase.OutputDataFormatIsBeta"/>.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public virtual void ValidateSetOutputTypeFormat<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            opts.OutputTypeFormat = Lib.Game.TypeFormat.Normal;
        }

        /// <summary>
        /// Sets <see cref="ConvertOptionsBase.OutputDataFormat"/> based on <see cref="ConvertOptionsBase.OutputTypeFormat"/> and <see cref="ConvertOptionsBase.OutputFileType"/>.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="opts">Options verb.</param>
        public void ValidateSetOutputDataFormat<T>(ParserResult<T> result, ConvertOptionsBase opts)
        {
            opts.OutputDataFormat = Getools.Lib.Converters.FormatConverter.GetDataFormat(opts.OutputTypeFormat, opts.OutputFileType);
        }
    }
}
