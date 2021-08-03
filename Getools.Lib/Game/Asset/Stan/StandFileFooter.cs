using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    public class StandFileFooter
    {
        public int? Unknown1 { get; set; }
        public int? Unknown2 { get; set; }
        public string C { get; set; }
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

        public byte[] ToByteArray()
        {
            int strAllocationSize = C?.Length ?? 0;

            if ((strAllocationSize % Config.TargetPointerSize) != 0)
            {
                strAllocationSize +=
                    Config.TargetPointerSize -
                    (strAllocationSize % Config.TargetPointerSize);
            }

            var results = new byte[strAllocationSize + (Config.TargetPointerSize * 6)];

            int index = 0;

            BitUtility.InsertPointer32Big(results, index, Unknown1);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, Unknown2);
            index += Config.TargetPointerSize;

            BitUtility.InsertString(results, index, C, strAllocationSize);
            index += strAllocationSize;

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

        public void AppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToByteArray();
            stream.Write(bytes);
        }
    }
}
