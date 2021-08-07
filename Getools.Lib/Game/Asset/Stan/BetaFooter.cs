using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.Error;

namespace Getools.Lib.Game.Asset.Stan
{
    /// <summary>
    /// Beta footer object declare at the end of stan; appears after regular <see cref="StandFileFooter"/>.
    /// Subset of <see cref="StandFile"/>.
    /// </summary>
    public class BetaFooter
    {
        /// <summary>
        /// String length of point name listed at the end of the beta stan.
        /// This is the exact string length including zeroes.
        /// </summary>
        public const int PointStringLength = 8;

        /// <summary>
        /// Initializes a new instance of the <see cref="BetaFooter"/> class.
        /// </summary>
        public BetaFooter()
        {
        }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Gets or sets the number of entries declared in the points array.
        /// </summary>
        public int DeclaredLength { get; set; }

        /// <summary>
        /// Beta stan files include ASCII names of points after the above footer.
        /// A fake empty string is inlcuded at the beginning of the list when outputing to .c.
        /// This is actually the .rodata section.
        /// </summary>
        public List<String> BetaPointList { get; set; } = new List<string>();

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using beta structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToBetaCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            int count = BetaPointList.Count;

            if (count < 1)
            {
                return string.Empty;
            }

            ///// only have one example, so not sure if there is supposed to be a null entry at
            ///// the end, or if it's supposed to pad to a multiple of 16.

            sb.AppendLine($"{prefix}{Config.Stan.BetaFooterCTypeName} {Config.Stan.DefaultDeclarationName_BetaFooter}[{count + 1}][{PointStringLength}] = {{");

            // Not actually a real entry, but required to get the .c file
            // to compile to a matching binary.
            sb.AppendLine(Config.DefaultIndent + "\"\"" + ",");

            for (int i = 0; i < count - 1; i++)
            {
                var p = BetaPointList[i];
                sb.AppendLine(Config.DefaultIndent + Formatters.Strings.ToQuotedString(p, PointStringLength) + ",");
            }

            if (BetaPointList.Any())
            {
                var p = BetaPointList.Last();
                sb.AppendLine(Config.DefaultIndent + Formatters.Strings.ToQuotedString(p, PointStringLength));
            }

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        /// <summary>
        /// Converts the current object to a byte array, as it would
        /// exist in a beta binary format.
        /// </summary>
        /// <returns>Byte array of object.</returns>
        public byte[] ToBetaByteArray()
        {
            bool appendEmpty = false;
            var pointsCount = BetaPointList.Count();

            // only have one example, so not sure if there is supposed to be a null entry at
            // the end, or if it's supposed to pad to a multiple of 16.
            if ((pointsCount & 0x1) > 0)
            {
                appendEmpty = true;
            }

            var allocCount = pointsCount + (appendEmpty ? 1 : 0);

            var results = new byte[allocCount * PointStringLength];

            int index = 0;
            foreach (var p in BetaPointList)
            {
                string s = (p.Length >= PointStringLength) ? p.Substring(0, PointStringLength - 1) : p;
                Array.Copy(System.Text.Encoding.ASCII.GetBytes(s), 0, results, index, s.Length);

                index += PointStringLength;
            }

            return results;
        }

        /// <summary>
        /// Should be called after deserializing. Cleans up values/properties
        /// based on the known format.
        /// </summary>
        public void DeserializeFix()
        {
            if (DeclaredLength < 1 && BetaPointList.Count > 0)
            {
                DeclaredLength = BetaPointList.Count;
            }

            if (string.IsNullOrEmpty(VariableName))
            {
                VariableName = Config.Stan.DefaultDeclarationName_BetaFooter;
            }
        }

        /// <summary>
        /// Reads from current position in stream. Loads object from
        /// stream as it would be read from a binary file using beta structs.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        /// <returns>New object.</returns>
        internal static BetaFooter ReadFromBetaBinFile(BinaryReader br)
        {
            var result = new BetaFooter();

            var buffer = new Byte[16];
            long position = br.BaseStream.Position;
            Byte b;
            int bufferPosition = 0;

            // read points until end of file
            while (position < br.BaseStream.Length - 1)
            {
                b = br.ReadByte();
                if (b > 0)
                {
                    buffer[bufferPosition] = b;
                    bufferPosition++;
                }
                else if (b == 0)
                {
                    if (buffer[0] > 0)
                    {
                        var pointName = System.Text.Encoding.ASCII.GetString(buffer, 0, bufferPosition);
                        result.BetaPointList.Add(pointName);

                        Array.Clear(buffer, 0, 16);
                        bufferPosition = 0;
                    }
                }

                if (bufferPosition >= 16)
                {
                    throw new BadFileFormatException($"Error reading stan, beta point name exceeded buffer length. Stream positiion: {position}");
                }

                position++;
            }

            if (buffer[0] > 0)
            {
                var pointName = System.Text.Encoding.ASCII.GetString(buffer, 0, bufferPosition);
                result.BetaPointList.Add(pointName);
            }

            return result;
        }

        /// <summary>
        /// Converts this object to a byte array using beta structs and writes
        /// it to the current stream position.
        /// </summary>
        /// <param name="stream">Stream to write to.</param>
        internal void BetaAppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToBetaByteArray();
            stream.Write(bytes);
        }
    }
}
