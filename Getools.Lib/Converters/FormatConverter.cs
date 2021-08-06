using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Getools.Lib.Game;

namespace Getools.Lib.Converters
{
    /// <summary>
    /// Helper methods to convert format enums.
    /// </summary>
    public static class FormatConverter
    {
        /// <summary>
        /// Gets <see cref="FileType"/> from combined data format.
        /// </summary>
        /// <param name="df">Format source.</param>
        /// <returns>File format.</returns>
        public static FileType FileTypeFromDataFormat(DataFormats df)
        {
            switch (df)
            {
                case DataFormats.Bin:
                case DataFormats.BetaBin:
                    return FileType.Bin;

                case DataFormats.C:
                case DataFormats.BetaC:
                    return FileType.C;

                case DataFormats.Json:
                    return FileType.Json;

                default:
                    return FileType.DefaultUnknown;
            }
        }

        /// <summary>
        /// Gets <see cref="TypeFormat"/> from combined data format.
        /// </summary>
        /// <param name="df">Format source.</param>
        /// <returns>Type format.</returns>
        public static TypeFormat TypeFormatFromDataFormat(DataFormats df)
        {
            switch (df)
            {
                case DataFormats.Bin:
                case DataFormats.C:
                    return TypeFormat.Normal;

                case DataFormats.BetaBin:
                case DataFormats.BetaC:
                    return TypeFormat.Beta;

                case DataFormats.Json:
                    // not enough information for json
                    // fallthrough
                default:
                    return TypeFormat.DefaultUnknown;
            }
        }

        /// <summary>
        /// Builds combined data format from descriptors.
        /// </summary>
        /// <param name="type">Struct type.</param>
        /// <param name="file">File container type.</param>
        /// <returns>Combined data format.</returns>
        public static DataFormats GetDataFormat(TypeFormat type, FileType file)
        {
            if (file == FileType.C)
            {
                if (type == TypeFormat.Normal)
                {
                    return DataFormats.C;
                }
                else if (type == TypeFormat.Beta)
                {
                    return DataFormats.BetaC;
                }
            }
            else if (file == FileType.Bin)
            {
                if (type == TypeFormat.Normal)
                {
                    return DataFormats.Bin;
                }
                else if (type == TypeFormat.Beta)
                {
                    return DataFormats.BetaBin;
                }
            }
            else if (file == FileType.Json)
            {
                return DataFormats.Json;
            }

            return DataFormats.DefaultUnknown;
        }

        /// <summary>
        /// Filters data formats down to a unique list of file types.
        /// </summary>
        /// <param name="formats">Formats to filter.</param>
        /// <returns>File types.</returns>
        public static List<FileType> ToKnownFileTypes(List<DataFormats> formats)
        {
            return formats
                .Select(x => FileTypeFromDataFormat(x))
                .Where(x => x != FileType.DefaultUnknown)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Filters data formats down to a unique list of struct types.
        /// </summary>
        /// <param name="formats">Formats to filter.</param>
        /// <returns>Struct types.</returns>
        public static List<TypeFormat> ToKnownTypeFormat(List<DataFormats> formats)
        {
            return formats
                .Select(x => TypeFormatFromDataFormat(x))
                .Where(x => x != TypeFormat.DefaultUnknown)
                .Distinct()
                .ToList();
        }
    }
}
