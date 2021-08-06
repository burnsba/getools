using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Getools.Lib.Game;

namespace Getools.Lib.Converters
{
    public static class FormatConverter
    {
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

                default:
                    return TypeFormat.DefaultUnknown;
            }
        }

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

        public static List<FileType> ToKnownFileTypes(List<DataFormats> formats)
        {
            return formats
                .Select(x => FileTypeFromDataFormat(x))
                .Where(x => x != FileType.DefaultUnknown)
                .Distinct()
                .ToList();
        }

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
