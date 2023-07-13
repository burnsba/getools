using Getools.Lib.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Palantir
{
    /// <summary>
    /// Helper class to collect information about a 2d polygon.
    /// Used to model stan tiles and bg boundary.
    /// </summary>
    internal class CollectionHullSvgPoints
    {
        public int OrderIndex { get; set; }

        public int Room { get; set; }

        // scaled, first point has been duplicated at the end to formed a closed svg path
        public List<Coord2dd> Points { get; set; }

        // native preset coordinate value
        public Coord3dd NaturalMin { get; set; } = Coord3dd.Zero.Clone();

        // native preset coordinate value
        public Coord3dd NaturalMax { get; set; } = Coord3dd.Zero.Clone();

        //  stage scaled coordinate value
        public Coord3dd ScaledMin { get; set; } = Coord3dd.Zero.Clone();

        // stage scaled coordinate value
        public Coord3dd ScaledMax { get; set; } = Coord3dd.Zero.Clone();
    }
}
