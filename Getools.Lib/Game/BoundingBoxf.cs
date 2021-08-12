using Getools.Lib.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Getools.Lib.Game
{
    public class BoundingBoxf
    {
        /// <summary>
        /// C file type name.
        /// </summary>
        public const string CTypeName = "bbox";

        /// <summary>
        /// Size of the <see cref="BoundingBoxf"/> struct in bytes.
        /// </summary>
        public const int SizeOf = 24;

        public Single MinX { get; set; }

        public Single MaxX { get; set; }

        public Single MinY { get; set; }

        public Single MaxY { get; set; }

        public Single MinZ { get; set; }

        public Single MaxZ { get; set; }

        public BoundingBoxf()
        { }

        public BoundingBoxf(Single minx, Single maxx, Single miny, Single maxy, Single minz, Single maxz)
        {
            MinX = minx;
            MaxX = maxx;
            MinY = miny;
            MaxY = maxy;
            MinZ = minz;
            MaxZ = maxz;
        }

        public static BoundingBoxf ReadFromBinFile(BinaryReader br)
        {
            var result = new BoundingBoxf();

            result.MinX = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.MaxX = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.MinY = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.MaxY = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.MinZ = BitUtility.CastToFloat(BitUtility.Read32Big(br));
            result.MaxZ = BitUtility.CastToFloat(BitUtility.Read32Big(br));

            return result;
        }

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs.
        /// Does not include type, variable name, or trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToCInlineDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            sb.Append("{");
            sb.Append(FloatingPoint.ToFloatCLiteral(MinX));
            sb.Append(", ");
            sb.Append(FloatingPoint.ToFloatCLiteral(MaxX));
            sb.Append(", ");
            sb.Append(FloatingPoint.ToFloatCLiteral(MinY));
            sb.Append(", ");
            sb.Append(FloatingPoint.ToFloatCLiteral(MaxY));
            sb.Append(", ");
            sb.Append(FloatingPoint.ToFloatCLiteral(MinZ));
            sb.Append(", ");
            sb.Append(FloatingPoint.ToFloatCLiteral(MaxZ));
            sb.Append("}");

            return sb.ToString();
        }
    }
}
