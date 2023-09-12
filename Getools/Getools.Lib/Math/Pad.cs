using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Math
{
    public class Pad
    {
        // sub_GAME_7F001BD4
        // perfect dark padGetCentre (pad.c)
        public static Coord3dd GetCenter(Coord3dd pos, Coord3dd up, Coord3dd look, BoundingBoxd bbox)
        {
            double temp_f12;
            double temp_f14;
            double temp_f16;

            Coord3dd normal = new Coord3dd();
            double scale;

            normal.X = (up.Y * look.Z) - (look.Y * up.Z);
            normal.Y = (up.Z * look.X) - (look.Z * up.X);
            normal.Z = (up.X * look.Y) - (look.X * up.Y);

            scale = 1.0f / System.Math.Sqrt((normal.X * normal.X) + ((normal.Y * normal.Y) + (normal.Z * normal.Z)));

            normal.X *= scale;
            normal.Y *= scale;
            normal.Z *= scale;

            temp_f16 = bbox.MinX + bbox.MaxX;
            temp_f14 = bbox.MinY + bbox.MaxY;
            temp_f12 = bbox.MinZ + bbox.MaxZ;

            var result = new Coord3dd();

            result.X = pos.X + ((
                (temp_f16 * normal.X) +
                (temp_f14 * up.X) +
                (temp_f12 * look.X)) * 0.5f);

            result.Y = pos.Y + ((
                (temp_f16 * normal.Y) +
                (temp_f14 * up.Y) +
                (temp_f12 * look.Y)) * 0.5f);

            result.Z = pos.Z + ((
                (temp_f16 * normal.Z) +
                (temp_f14 * up.Z) +
                (temp_f12 * look.Z)) * 0.5f);

            return result;
        }
    }
}
