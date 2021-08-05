using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    public class BetaFooter
    {
        // includes terminating zero
        public const int PointStringLength = 8;

        public BetaFooter()
        {
        }

        /// <summary>
        /// Beta stan files include ASCII names of points after the above footer.
        /// </summary>
        public List<String> BetaPointList { get; set; } = new List<string>();

        public string ToBetaCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            int count = BetaPointList.Count;

            if (count < 1)
            {
                return string.Empty;
            }

            // only have one example, so not sure if there is supposed to be a null entry at
            // the end, or if it's supposed to pad to a multiple of 16.

            sb.AppendLine($"{prefix}{Config.Stan.BetaFooterCTypeName} {Config.Stan.DefaultDeclarationName_BetaFooter}[{count + 1}][{PointStringLength}] = {{");

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

        internal void BetaAppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToBetaByteArray();
            stream.Write(bytes);
        }

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
                    throw new Exception($"Error reading stan, beta point name exceeded buffer length. Stream positiion: {position}");
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
    }
}
