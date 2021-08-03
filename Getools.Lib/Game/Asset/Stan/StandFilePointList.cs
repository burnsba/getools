using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    public class StandFilePointList
    {
        public List<StandFilePoint> Points { get; set; } = new List<StandFilePoint>();

        public string Name { get; set; }
        public int DeclaredLength { get; set; }

        public int OrderId { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public void AppendToBinaryStream(BinaryWriter stream)
        {
            foreach (var p in Points)
            {
                p.AppendToBinaryStream(stream);
            }
        }

        public string ToCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();
            int i;
            int numberPoints = Points.Count();

            sb.AppendLine($"{prefix}{Config.Stan.PointCTypeName} {Name}[{numberPoints}] = {{");

            for (i = 0; i < numberPoints - 1; i++)
            {
                var p = Points[i];
                sb.AppendLine(p.ToCInlineDeclaration(Config.DefaultIndent) + ",");
            }

            if (Points.Any())
            {
                sb.AppendLine(Points.Last().ToCInlineDeclaration(Config.DefaultIndent));
            }

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }

        public static StandFilePointList ReadFromBinFile(BinaryReader br, int tileIndex, int pointsCount)
        {
            var result = new StandFilePointList();

            for (int i=0; i<pointsCount; i++)
            {
                var point = StandFilePoint.ReadFromBinFile(br);
                result.Points.Add(point);
            }

            result.Name = $"{Config.Stan.DefaultDeclarationName_StandFilePoint}_{tileIndex:X}";
            result.OrderId = tileIndex;
            result.DeclaredLength = pointsCount;

            return result;
        }
    }
}
