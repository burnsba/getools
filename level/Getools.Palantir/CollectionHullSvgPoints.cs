using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Palantir
{
    /// <summary>
    /// Helper class to collect information about a 2d (convex) polygon.
    /// Used to model stan tiles and bg boundary.
    /// </summary>
    internal class CollectionHullSvgPoints
    {
        /// <summary>
        /// Gets or sets the order index of the item.
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Gets or sets the room of the item.
        /// </summary>
        public int Room { get; set; }

        /// <summary>
        /// Scaled, first point has been duplicated at the end to formed a closed svg path.
        /// </summary>
        public List<Coord2dd>? Points { get; set; } = null;

        /// <summary>
        /// Native preset coordinate value.
        /// </summary>
        public Coord3dd NaturalMin { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// Native preset coordinate value.
        /// </summary>
        public Coord3dd NaturalMax { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// Stage scaled coordinate value.
        /// </summary>
        public Coord3dd ScaledMin { get; set; } = Coord3dd.Zero.Clone();

        /// <summary>
        /// Stage scaled coordinate value.
        /// </summary>
        public Coord3dd ScaledMax { get; set; } = Coord3dd.Zero.Clone();
    }
}
