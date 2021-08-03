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

            sb.AppendLine(Points.Last().ToCInlineDeclaration(Config.DefaultIndent));

            sb.AppendLine($"{prefix}}};");

            return sb.ToString();
        }
    }
}
