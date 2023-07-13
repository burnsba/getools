using Getools.Lib.Game;
using Getools.Lib.Game.Enums;
using Getools.Palantir.Render;

namespace Getools.Palantir
{
    /// <summary>
    /// Helper class to manage data as it's being processed.
    /// </summary>
    internal class ProcessedStageDataContext
    {
        public OutputImasgeFormat OutputFormat { get; set; } = OutputImasgeFormat.DefaultUnknown;
        public SliceMode Mode { get; set; } = SliceMode.Unbound;
        public double? Z { get; set; } = null;
        public double? Zmin { get; set; } = null;
        public double? Zmax { get; set; } = null;
        public List<CollectionHullSvgPoints> RoomPolygons { get; set; } = new List<CollectionHullSvgPoints>();
        public List<CollectionHullSvgPoints> TilePolygons { get; set; } = new List<CollectionHullSvgPoints>();
        public List<RenderPosition> PresetPolygons { get; set; } = new List<RenderPosition>();
        public List<RenderPosition> IntroPolygons { get; set; } = new List<RenderPosition>();
        public Dictionary<PropDef, List<PropPosition>> SetupPolygonsCollection { get; set; } = new Dictionary<PropDef, List<PropPosition>>();
        public Coord3dd ScaledMin { get; set; } = Coord3dd.MaxValue.Clone();
        public Coord3dd ScaledMax { get; set; } = Coord3dd.MinValue.Clone();
        public Coord3dd NativeMin { get; set; } = Coord3dd.MaxValue.Clone();
        public Coord3dd NativeMax { get; set; } = Coord3dd.MinValue.Clone();
    }
}
