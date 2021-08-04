using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    public class StandFileFooter
    {
        public StandFileFooter()
        {
        }

        public int? Unknown1 { get; set; }
        public int? Unknown2 { get; set; }
        public string C { get; set; } = "unstric";
        public int? Unknown3 { get; set; }
        public int? Unknown4 { get; set; }
        public int? Unknown5 { get; set; }
        public int? Unknown6 { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public string ToCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{Config.Stan.FooterCTypeName} {Name} = {{");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown1)},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown2)},");

            if (!string.IsNullOrEmpty(C))
            {
                sb.AppendLine($"{prefix}{Config.DefaultIndent}\"{C}\",");
            }
            else
            {
                sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(null)},");
            }

            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown3)},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown4)},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown5)},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown6)}");

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        public string ToBetaCDeclaration(string prefix = "")
        {
            // no change for beta
            return ToCDeclaration(prefix);
        }

        public byte[] ToByteArray(int currentStreamPosition)
        {
            int stringStart = currentStreamPosition + (Config.TargetPointerSize * 2);
            int stringEnd = stringStart + C.Length - 1; // adjust for zero
            int next16 = 0;
            if ((stringEnd % 16) != 0)
            {
                next16 = ((int)(stringEnd / 16) + 1) * 16;
            }
            else
            {
                next16 = (int)(stringEnd / 16) * 16;
            }
            
            var stringLength = next16 - stringStart;

            var results = new byte[stringLength + (Config.TargetPointerSize * 6)];

            int index = 0;

            BitUtility.InsertPointer32Big(results, index, Unknown1);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, Unknown2);
            index += Config.TargetPointerSize;

            BitUtility.InsertString(results, index, C, stringLength);
            index += stringLength;

            BitUtility.InsertPointer32Big(results, index, Unknown3);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, Unknown4);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, Unknown5);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, Unknown6);
            index += Config.TargetPointerSize;

            return results;
        }

        internal void AppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToByteArray((int)stream.BaseStream.Position);
            stream.Write(bytes);
        }

        internal void BetaAppendToBinaryStream(BinaryWriter stream)
        {
            // no changes for beta
            AppendToBinaryStream(stream);
        }

        internal static StandFileFooter ReadFromBinFile(BinaryReader br)
        {
            var result = new StandFileFooter();

            result.Unknown1 = br.ReadInt32();
            if (result.Unknown1 == 0)
            {
                result.Unknown1 = null;
            }

            result.Unknown2 = br.ReadInt32();
            if (result.Unknown2 == 0)
            {
                result.Unknown2 = null;
            }

            int stringStart = (int)br.BaseStream.Position;

            var strBytes = new List<byte>();
            int stringLength = 0;
            int safety = 32;
            while (true)
            {
                Byte b = br.ReadByte();

                if (b == 0)
                {
                    br.BaseStream.Seek(-1, SeekOrigin.Current);
                    break;
                }

                strBytes.Add(b);

                stringLength++;

                if (stringLength >= safety)
                {
                    throw new Exception("Could not find terminating character when reading stan footer string");
                }
            }

            var unstricString = System.Text.Encoding.ASCII.GetString(strBytes.ToArray());
            result.C = unstricString;

            int stringEnd = stringStart + stringLength;
            if ((stringEnd % 16) != 0)
            {
                int next16 = ((int)(stringEnd / 16) + 1) * 16;
                var seek = next16 - stringEnd;
                br.BaseStream.Seek(seek, SeekOrigin.Current);
            }

            result.Unknown3 = br.ReadInt32();
            if (result.Unknown3 == 0)
            {
                result.Unknown3 = null;
            }

            result.Unknown4 = br.ReadInt32();
            if (result.Unknown4 == 0)
            {
                result.Unknown4 = null;
            }

            result.Unknown5 = br.ReadInt32();
            if (result.Unknown5 == 0)
            {
                result.Unknown5 = null;
            }

            result.Unknown6 = br.ReadInt32();
            if (result.Unknown6 == 0)
            {
                result.Unknown6 = null;
            }

            result.Name = $"{Config.Stan.DefaultDeclarationName_StandFileFooter}";

            return result;
        }

        internal static StandFileFooter ReadFromBetaBinFile(BinaryReader br)
        {
            // footer is the same here.
            return StandFileFooter.ReadFromBinFile(br);
        }
    }
}
