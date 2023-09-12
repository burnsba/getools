using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Math
{
    public class Compare
    {
        public static void SetMinMaxCompareX(Coord3dd setmin, Coord3dd setmax, double comparePoint, double limitMin, double limitMax)
        {
            if (comparePoint < setmin.X && comparePoint >= limitMin)
            {
                setmin.X = comparePoint;
            }

            if (comparePoint > setmax.X && comparePoint <= limitMax)
            {
                setmax.X = comparePoint;
            }
        }

        public static void SetMinMaxCompareY(Coord3dd setmin, Coord3dd setmax, double comparePoint, double limitMin, double limitMax)
        {
            if (comparePoint < setmin.Y && comparePoint >= limitMin)
            {
                setmin.Y = comparePoint;
            }

            if (comparePoint > setmax.Y && comparePoint <= limitMax)
            {
                setmax.Y = comparePoint;
            }
        }

        public static void SetMinMaxCompareZ(Coord3dd setmin, Coord3dd setmax, double comparePoint, double limitMin, double limitMax)
        {
            if (comparePoint < setmin.Z && comparePoint >= limitMin)
            {
                setmin.Z = comparePoint;
            }

            if (comparePoint > setmax.Z && comparePoint <= limitMax)
            {
                setmax.Z = comparePoint;
            }
        }

        public static void SetUnboundCompareX(Coord3dd setmin, Coord3dd setmax, double comparePoint)
        {
            if (comparePoint < setmin.X)
            {
                setmin.X = comparePoint;
            }

            if (comparePoint > setmax.X)
            {
                setmax.X = comparePoint;
            }
        }

        public static void SetUnboundCompareY(Coord3dd setmin, Coord3dd setmax, double comparePoint)
        {
            if (comparePoint < setmin.Y)
            {
                setmin.Y = comparePoint;
            }

            if (comparePoint > setmax.Y)
            {
                setmax.Y = comparePoint;
            }
        }

        public static void SetUnboundCompareZ(Coord3dd setmin, Coord3dd setmax, double comparePoint)
        {
            if (comparePoint < setmin.Z)
            {
                setmin.Z = comparePoint;
            }

            if (comparePoint > setmax.Z)
            {
                setmax.Z = comparePoint;
            }
        }

        public static void SetUnboundCompare(ref double setmin, ref double setmax, double comparePoint)
        {
            if (comparePoint < setmin)
            {
                setmin = comparePoint;
            }

            if (comparePoint > setmax)
            {
                setmax = comparePoint;
            }
        }
    }
}
