using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    public class StandFileHeader
    {
        public StandFileHeader()
        {
        }

        public int? Unknown1 { get; set; }
        public int FirstTileOffset { get; set; }
        public List<Byte> UnknownHeaderData { get; set; } = new List<byte>();

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
            sb.AppendLine($"{prefix}{Config.DefaultIndent}{Formatters.IntegralTypes.ToCInlineByteArray(UnknownHeaderData)}");
            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        public byte[] ToByteArray()
        {
            var results = new byte[4 + 4 + UnknownHeaderData.Count];

            int index = 0;

            BitUtility.InsertPointer32Big(results, index, Unknown1);
            index += Config.TargetPointerSize;

            BitUtility.InsertPointer32Big(results, index, FirstTileOffset);
            index += Config.TargetPointerSize;

            Array.Copy(UnknownHeaderData.ToArray(), 0, results, index, UnknownHeaderData.Count);
            index += UnknownHeaderData.Count;

            return results;
        }

        internal void AppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToByteArray();
            stream.Write(bytes);
        }

        internal static StandFileHeader ReadFromBinFile(BinaryReader br, string name)
        {
            var result = new StandFileHeader();

            result.Unknown1 = br.ReadInt32();
            if (result.Unknown1 == 0)
            {
                result.Unknown1 = null;
            }

            result.FirstTileOffset = (int)(BitUtility.Swap((uint)br.ReadInt32()));

            var remaining = result.FirstTileOffset - br.BaseStream.Position;
            if (remaining < 0)
            {
                throw new Exception($"Error reading stan header, invalid first tile offset: \"{result.FirstTileOffset}\"");
            }

            for (int i=0; i<remaining; i++)
            {
                result.UnknownHeaderData.Add(br.ReadByte());
            }

            result.Name = name;

            return result;
        }

        internal static StandFileHeader ReadFromBetaBinFile(BinaryReader br, string name)
        {
            var result = new StandFileHeader();

            result.Unknown1 = br.ReadInt32();
            if (result.Unknown1 == 0)
            {
                result.Unknown1 = null;
            }

            result.FirstTileOffset = (int)(BitUtility.Swap((uint)br.ReadInt32()));

            var remaining = result.FirstTileOffset - br.BaseStream.Position;
            if (remaining < 0)
            {
                throw new Exception($"Error reading stan header, invalid first tile offset: \"{result.FirstTileOffset}\"");
            }

            for (int i=0; i<remaining; i++)
            {
                result.UnknownHeaderData.Add(br.ReadByte());
            }

            result.Name = name;

            return result;
        }
    }
}
