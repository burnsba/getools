using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Setup.Ai;
using Getools.Lib.Game.Enums;
using Getools.Palantir.Render;

namespace Getools.Palantir
{
    /// <summary>
    /// Helper class to manage data as it's being processed.
    /// </summary>
    public class ProcessedStageData
    {
        /// <summary>
        /// Gets or sets final output image format.
        /// </summary>
        public OutputImageFormat OutputFormat { get; set; } = OutputImageFormat.DefaultUnknown;

        /// <summary>
        /// Gets or sets the slice mode of the data set being processed.
        /// </summary>
        public SliceMode Mode { get; set; } = SliceMode.Unbound;

        /// <summary>
        /// Gets or sets the single vertical value being sliced. Used with <see cref="SliceMode.Slice"/>.
        /// </summary>
        public double? Z { get; set; } = null;

        /// <summary>
        /// Gets or sets the min vertical value to consider. Used with <see cref="SliceMode.BoundingBox"/>.
        /// When <see cref="SliceMode.Unbound"/>, this is <see cref="double.MinValue"/>.
        /// </summary>
        public double? Zmin { get; set; } = null;

        /// <summary>
        /// Gets or sets the max vertical value to consider. Used with <see cref="SliceMode.BoundingBox"/>.
        /// When <see cref="SliceMode.Unbound"/>, this is <see cref="double.MaxValue"/>.
        /// </summary>
        public double? Zmax { get; set; } = null;

        /// <summary>
        /// Gets or sets the collection of room polygons.
        /// </summary>
        public List<HullPoints> RoomPolygons { get; set; } = new List<HullPoints>();

        /// <summary>
        /// Gets or sets the collection of stan polygons.
        /// </summary>
        public List<HullPoints> TilePolygons { get; set; } = new List<HullPoints>();

        /// <summary>
        /// Gets or sets the collection of pad items to be rendered.
        /// </summary>
        public List<RenderPosition> PresetPolygons { get; set; } = new List<RenderPosition>();

        /// <summary>
        /// Gets or sets the collection of setup intro items to be rendered.
        /// </summary>
        public List<RenderPosition> IntroPolygons { get; set; } = new List<RenderPosition>();

        /// <summary>
        /// Gets or sets the collection of setup path waypoints to be rendered.
        /// </summary>
        public List<RenderLine> PathWaypointLines { get; set; } = new List<RenderLine>();

        /// <summary>
        /// Gets or sets the collection of setup path patrols to be rendered.
        /// </summary>
        public List<RenderPolyline> PatrolPathLines { get; set; } = new List<RenderPolyline>();

        /// <summary>
        /// Gets or sets the collection of "everything else" setup data to be rendered (props, guards, doors, etc).
        /// </summary>
        public Dictionary<PropDef, List<PropPosition>> SetupPolygonsCollection { get; set; } = new Dictionary<PropDef, List<PropPosition>>();

        /// <summary>
        /// Gets or sets the overall min scaled value seen so far.
        /// </summary>
        public Coord3dd ScaledMin { get; set; } = Coord3dd.MaxValue.Clone();

        /// <summary>
        /// Gets or sets the overall max scaled value seen so far.
        /// </summary>
        public Coord3dd ScaledMax { get; set; } = Coord3dd.MinValue.Clone();

        /// <summary>
        /// Gets or sets the native min value seen so far.
        /// </summary>
        public Coord3dd NativeMin { get; set; } = Coord3dd.MaxValue.Clone();

        /// <summary>
        /// Gets or sets the native max value seen so far.
        /// </summary>
        public Coord3dd NativeMax { get; set; } = Coord3dd.MinValue.Clone();

        /// <summary>
        /// Gets or sets the collection of AI scripts associated with the stage data.
        /// </summary>
        public List<AiCommandBlock> AiScripts { get; set; } = new List<AiCommandBlock>();

        /// <summary>
        /// Gets or sets lookup table to map guard ids to any associated AI scripts.
        /// </summary>
        public Dictionary<int, HashSet<int>> ChrIdToAiCommandBlock { get; set; } = new Dictionary<int, HashSet<int>>();

        /// <summary>
        /// Gets or sets lookup table to pad ids to any associated AI scripts.
        /// </summary>
        public Dictionary<int, HashSet<int>> PadIdToAiCommandBlock { get; set; } = new Dictionary<int, HashSet<int>>();

        /// <summary>
        /// Gets or sets lookup table to patrols to any associated AI scripts.
        /// </summary>
        public Dictionary<int, HashSet<int>> PathIdToAiCommandBlock { get; set; } = new Dictionary<int, HashSet<int>>();

        /// <summary>
        /// Gets or sets list of unique AI script ids processed so far.
        /// </summary>
        /// <remarks>
        /// This is used to include global AI scripts referenced by guards.
        /// </remarks>
        public HashSet<int> RefAiListId { get; set; } = new HashSet<int>();

        /// <summary>
        /// Gets or sets lookup table to map AI scripts to any associated guard ids.
        /// </summary>
        public Dictionary<int, HashSet<int>> AiCommandBlockToChrId { get; set; } = new Dictionary<int, HashSet<int>>();

        /// <summary>
        /// Gets or sets lookup table to map AI scripts to any associated pad ids.
        /// </summary>
        public Dictionary<int, HashSet<int>> AiCommandBlockToPadId { get; set; } = new Dictionary<int, HashSet<int>>();

        /// <summary>
        /// Gets or sets lookup table to map AI scripts to any associated patrols.
        /// </summary>
        public Dictionary<int, HashSet<int>> AiCommandBlockToPathId { get; set; } = new Dictionary<int, HashSet<int>>();
    }
}
