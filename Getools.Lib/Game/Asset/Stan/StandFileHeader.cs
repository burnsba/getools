using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    public class StandFileHeader
    {
        public int? Unknown1 { get; set; }
        public int? FirstTileOffset { get; set; }
        public int? Unknown2 { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public string ToCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{Config.Stan.HeaderCTypeName} {Name} = {{");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown1)},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(FirstTileOffset)},");
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCPointerString(Unknown2)}");
            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        public byte[] ToByteArray()
        {
            var results = new byte[Config.TargetPointerSize * 3];

            int index = 0;

            BitUtility.InsertPointer32Big(results, index, Unknown1);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, FirstTileOffset);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, Unknown2);
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
