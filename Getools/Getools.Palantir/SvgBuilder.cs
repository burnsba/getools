using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Getools.Lib.Extensions;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Bg;
using Getools.Lib.Game.Asset.Intro;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.Setup.Ai;
using Getools.Lib.Game.Asset.SetupObject;
using Getools.Lib.Game.Asset.Stan;
using Getools.Lib.Game.Engine;
using Getools.Lib.Game.Enums;
using Getools.Palantir.Render;
using SvgLib;

namespace Getools.Palantir
{
    /// <summary>
    /// Internal helper class. Used to translate the processed stage data to an output SVG document.
    /// </summary>
    internal class SvgBuilder
    {
        // three decimal places
        private const string StandardDoubleToStringFormat = "0.###";

        private const string SvgBgLayerId = "svg-bg-room-layer";
        private const string SvgStanLayerId = "svg-stan-tile-layer";
        private const string SvgPadLayerId = "svg-pad-layer";
        private const string SvgWaypointLayerId = "svg-waypoint-layer";
        private const string SvgPatrolPathLayerId = "svg-patrol-layer";

        private const string SvgSetupAlarmLayerId = "svg-setup-alarm-layer";
        private const string SvgSetupAmmoLayerId = "svg-setup-ammo-layer";
        private const string SvgSetupAircraftLayerId = "svg-setup-aircraft-layer";
        private const string SvgSetupBodyArmorLayerId = "svg-setup-bodyarmor-layer";
        private const string SvgSetupChrLayerId = "svg-setup-chr-layer";
        private const string SvgSetupCctvLayerId = "svg-setup-cctv-layer";
        private const string SvgSetupCollectableLayerId = "svg-setup-collectable-layer";
        private const string SvgSetupDoorLayerId = "svg-setup-door-layer";
        private const string SvgSetupDroneLayerId = "svg-setup-drone-layer";
        private const string SvgSetupKeyLayerId = "svg-setup-key-layer";
        private const string SvgSetupSafeLayerId = "svg-setup-safe-layer";
        private const string SvgSetupSingleMonitorLayerId = "svg-setup-singlemonitor-layer";
        private const string SvgSetupStandardPropLayerId = "svg-setup-prop-layer";
        private const string SvgSetupTankLayerId = "svg-setup-tank-layer";

        private const string SvgSetupIntroLayerId = "svg-setup-intro-layer";

        // waypoint, path table index
        private const string SvgItemIdPathWaypointFormat = "svg-waypoint-{0}-t-{1}";

        private const string SvgItemIdPatrolPathFormat = "svg-patrol-{0}";

        private const string SvgItemIdRoomFormat = "svg-room-{0}";
        private const string SvgItemIdTileFormat = "svg-room-{0}-tile-{1}";
        private const string SvgItemIdPadFormat = "svg-pad-{0}";

        private const string SvgItemIdSetupAlarmFormat = "svg-setup-alarm-{0}";
        private const string SvgItemIdSetupAmmoFormat = "svg-setup-ammo-{0}";
        private const string SvgItemIdSetupAircraftFormat = "svg-setup-aircraft-{0}";
        private const string SvgItemIdSetupBodyArmorFormat = "svg-setup-bodyarmor-{0}";
        private const string SvgItemIdSetupChrFormat = "svg-setup-chr-{0}";
        private const string SvgItemIdSetupCctvFormat = "svg-setup-cctv-{0}";
        private const string SvgItemIdSetupCollectableFormat = "svg-setup-collectable-{0}";
        private const string SvgItemIdSetupDoorFormat = "svg-setup-door-{0}";
        private const string SvgItemIdSetupDroneFormat = "svg-setup-drone-{0}";
        private const string SvgItemIdSetupKeyFormat = "svg-setup-key-{0}";
        private const string SvgItemIdSetupSafeFormat = "svg-setup-safe-{0}";
        private const string SvgItemIdSetupSingleMonitorFormat = "svg-setup-singlemonitor-{0}";
        private const string SvgItemIdSetupStandardPropFormat = "svg-setup-prop-{0}";
        private const string SvgItemIdSetupTankFormat = "svg-setup-tank-{0}";

        private const string SvgItemIdSetupIntroFormat = "svg-setup-intro-{0}";

        private const string SvgAiListsId = "svg-ailists";
        private const string SvgAiScriptFormat = "svg-ai-{0}";

        private const double MaxPixelSizeError = 10000000;

        private readonly ProcessedStageData _context;
        private readonly Stage _stage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgBuilder"/> class.
        /// </summary>
        /// <param name="context">Current stage data context.</param>
        /// <param name="stage">Stage data.</param>
        public SvgBuilder(ProcessedStageData context, Stage stage)
        {
            _context = context;
            _stage = stage;
        }

        /// <summary>
        /// Gets or sets default output width.
        /// Height will be automatically set according to required aspect ratio.
        /// </summary>
        public int OutputWidth { get; set; } = 2048;

        /// <summary>
        /// Final processing step, should be called after the stage has been "sliced".
        /// </summary>
        /// <returns>SVG.</returns>
        public SvgDocument BuildSvg()
        {
            // The resulting output should cover the range of the level.
            // This will be the "view box". It might have to scale to accomodate
            // the desired output width.
            var outputVeiwboxWidth = Math.Abs(_context.ScaledMax.X - _context.ScaledMin.X);
            var outputViewboxHeight = Math.Abs(_context.ScaledMax.Z - _context.ScaledMin.Z);

            if (!double.IsFinite(outputVeiwboxWidth) || outputVeiwboxWidth > MaxPixelSizeError)
            {
                throw new Exception("Invalid image width");
            }

            if (!double.IsFinite(outputViewboxHeight) || outputViewboxHeight > MaxPixelSizeError || outputViewboxHeight == 0)
            {
                throw new Exception("Invalid image height");
            }

            // Find offset to zero axis.
            var adjustx = 0 - _context.ScaledMin.X;
            var adjusty = 0 - _context.ScaledMin.Z;

            double naturalRatioWh = outputVeiwboxWidth / outputViewboxHeight;
            double naturalRatioHw = outputViewboxHeight / outputVeiwboxWidth;

            var svg = SvgDocument.Create();

            svg.Width = OutputWidth;
            svg.Height = svg.Width * naturalRatioHw;

            svg.ViewBox = new SvgViewBox()
            {
                Width = outputVeiwboxWidth,
                Height = outputViewboxHeight,
                Left = _context.ScaledMin.X,
                Top = _context.ScaledMin.Z,
            };

            svg.SetDataAttribute("created-with-tool", "getool");
            svg.SetDataAttribute("created-home", "https://github.com/burnsba/getools");
            svg.SetDataAttribute("created-on", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            //// svg.SetDataAttribute("created-by", "Ben Burns");
            //// svg.SetDataAttribute("author-home", "https://tolos.me/");

            // Log useful attributes to output image.
            svg.SetDataAttribute("adjustx", adjustx.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("adjusty", adjusty.ToString(StandardDoubleToStringFormat));

            svg.SetDataAttribute("level-scale", _stage.LevelScale.ToString(StandardDoubleToStringFormat));

            // "n" is short for "natural"
            svg.SetDataAttribute("n-min-x", _context.NativeMin.X.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("n-max-x", _context.NativeMax.X.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("n-min-y", _context.NativeMin.Y.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("n-max-y", _context.NativeMax.Y.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("n-min-z", _context.NativeMin.Z.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("n-max-z", _context.NativeMax.Z.ToString(StandardDoubleToStringFormat));

            // "s" is short for "scaled"
            svg.SetDataAttribute("s-min-x", _context.ScaledMin.X.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("s-max-x", _context.ScaledMax.X.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("s-min-y", _context.ScaledMin.Y.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("s-max-y", _context.ScaledMax.Y.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("s-min-z", _context.ScaledMin.Z.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("s-max-z", _context.ScaledMax.Z.ToString(StandardDoubleToStringFormat));

            // Add default CSS styling
            var cssText = new StringBuilder();
            cssText.AppendLine(".gelib-stan { stroke: #9e87a3; stroke-width: 2; fill: #fdf5ff; }");
            cssText.AppendLine(".gelib-room { stroke: blue; stroke-width: 4; fill: #ffffff; fill-opacity: 0; }");
            cssText.AppendLine(".gelib-pad { stroke: #9c009e; stroke-width: 4; fill: #ff00ff; }");
            cssText.AppendLine(".gelib-wpline { stroke: #ff80ff; stroke-width: 12; }");
            cssText.AppendLine(".gelib-patrol { stroke: #3fb049; stroke-width: 18; }");
            cssText.AppendLine("#" + SvgAiListsId + " { display: none; }");

            svg.SetStylesheet(cssText.ToString());

            AddStanGroupToSvgDoc(svg, _context.TilePolygons);

            // room boundaries need to be above the tiles.
            AddBgGroupToSvgDoc(svg, _context.RoomPolygons);

            AddPadGroupToSvgDoc(svg, _context.PresetPolygons);

            // draw path waypoints on top of pads
            AddPathWaypointGroupToSvgDoc(svg, _context.PathWaypointLines);

            // draw patrol paths on top of pads and waypoints
            AddPatrolPathGroupToSvgDoc(svg, _context.PatrolPathLines);

            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.AmmoBox, svg, SvgSetupAmmoLayerId, SvgItemIdSetupAmmoFormat);

            // safe should be under door z layer.
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.Safe, svg, SvgSetupSafeLayerId, SvgItemIdSetupSafeFormat);
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.Door, svg, SvgSetupDoorLayerId, SvgItemIdSetupDoorFormat);
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.Alarm, svg, SvgSetupAlarmLayerId, SvgItemIdSetupAlarmFormat);
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.Cctv, svg, SvgSetupCctvLayerId, SvgItemIdSetupCctvFormat);
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.Drone, svg, SvgSetupDroneLayerId, SvgItemIdSetupDroneFormat);
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.Aircraft, svg, SvgSetupAircraftLayerId, SvgItemIdSetupAircraftFormat);
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.Tank, svg, SvgSetupTankLayerId, SvgItemIdSetupTankFormat);
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.StandardProp, svg, SvgSetupStandardPropLayerId, SvgItemIdSetupStandardPropFormat);
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.SingleMonitor, svg, SvgSetupSingleMonitorLayerId, SvgItemIdSetupSingleMonitorFormat);
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.Collectable, svg, SvgSetupCollectableLayerId, SvgItemIdSetupCollectableFormat);
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.Armour, svg, SvgSetupBodyArmorLayerId, SvgItemIdSetupBodyArmorFormat);

            // keys should be on top of tables (StandardProp)
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.Key, svg, SvgSetupKeyLayerId, SvgItemIdSetupKeyFormat);

            // guards should be one of the highest z layers
            AddSetupGroupToSvgDoc(_context.SetupPolygonsCollection, PropDef.Guard, svg, SvgSetupChrLayerId, SvgItemIdSetupChrFormat);

            // Intro star should be above other layers
            AddIntroGroupToSvgDoc(svg, _context.IntroPolygons);

            AddAiListsToSvgDoc(svg, _context.AiScripts);

            return svg;
        }

        private void AddSetupGroupToSvgDoc(Dictionary<PropDef, List<PropPointPosition>> collection, PropDef key, SvgDocument svg, string groupId, string itemFormatString)
        {
            if (collection.ContainsKey(key))
            {
                var group = svg.AddGroup();
                group.Id = groupId;
                foreach (var poly in collection[key])
                {
                    var svgprop = SvgAppend.PropToSvg.SetupObjectToSvgAppend(group, poly, _stage.LevelScale);
                    if (!object.ReferenceEquals(null, svgprop))
                    {
                        SetupObjectGuard? guard = null;

                        // check for linked ai script
                        if (key == PropDef.Guard)
                        {
                            guard = (SetupObjectGuard)poly.SetupObject!;
                            svgprop.Id = string.Format(itemFormatString, (int)guard.ObjectId);
                        }
                        else
                        {
                            svgprop.Id = string.Format(itemFormatString, poly.OrderIndex);
                        }

                        AddPropAttributes(svgprop, poly);

                        svgprop.SetDataAttribute("nc-x", poly.Origin.X.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("nc-y", poly.Origin.Y.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("nc-z", poly.Origin.Z.ToString(StandardDoubleToStringFormat));

                        var scaledPos = poly.Origin.Clone().Scale(1 / _stage.LevelScale);

                        svgprop.SetDataAttribute("sc-x", scaledPos.X.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("sc-y", scaledPos.Y.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("sc-z", scaledPos.Z.ToString(StandardDoubleToStringFormat));

                        svgprop.SetDataAttribute("room-id", poly.Room.ToString());

                        // check for linked ai script
                        if (key == PropDef.Guard)
                        {
                            svgprop.SetDataAttribute("chr-hdist", guard!.HearingDistance.ToString());
                            svgprop.SetDataAttribute("chr-vdist", guard.VisibileDistance.ToString());

                            if ((guard.Flags & Getools.Lib.Game.Flags.SetupChrFlags.GUARD_SETUP_FLAG_CHR_CLONE) > 0)
                            {
                                svgprop.SetDataAttribute("chr-clone", string.Empty);
                            }

                            if ((guard.Flags & Getools.Lib.Game.Flags.SetupChrFlags.GUARD_SETUP_FLAG_CHR_INVINCIBLE) > 0)
                            {
                                svgprop.SetDataAttribute("chr-invincible", string.Empty);
                            }

                            svgprop.SetDataAttribute("x-ai-init", guard!.ActionPathAssignment.ToString());

                            var chrId = (int)guard!.ObjectId;
                            if (_context.ChrIdToAiCommandBlock.ContainsKey(chrId))
                            {
                                var aiIdJson = string.Join(", ", _context.ChrIdToAiCommandBlock[chrId].OrderBy(x => x).Select(x => x.ToString()));
                                svgprop.SetDataAttribute("x-ai-id", $"[{aiIdJson}]");
                            }
                        }
                    }
                }
            }
        }

        private void AddBgGroupToSvgDoc(SvgDocument svg, List<HullPoints> roomPolygons)
        {
            var group2 = svg.AddGroup();
            group2.Id = SvgBgLayerId;
            foreach (var poly in roomPolygons)
            {
                if (object.ReferenceEquals(null, poly.Points))
                {
                    throw new NullReferenceException();
                }

                var polyline = group2.AddPolyLine();

                polyline.AddClass("svg-logical-item");
                polyline.AddClass("gelib-room");

                polyline.Id = string.Format(SvgItemIdRoomFormat, poly.OrderIndex);
                polyline.SetPoints(poly.Points.To1dArray(), StandardDoubleToStringFormat);

                polyline.SetDataAttribute("n-min-x", poly.NaturalMin.X.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("n-max-x", poly.NaturalMax.X.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("n-min-y", poly.NaturalMin.Y.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("n-max-y", poly.NaturalMax.Y.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("n-min-z", poly.NaturalMin.Z.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("n-max-z", poly.NaturalMax.Z.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-min-x", poly.ScaledMin.X.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-max-x", poly.ScaledMax.X.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-min-y", poly.ScaledMin.Y.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-max-y", poly.ScaledMax.Y.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-min-z", poly.ScaledMin.Z.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-max-z", poly.ScaledMax.Z.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("room-id", poly.Room.ToString());
            }
        }

        private void AddPadGroupToSvgDoc(SvgDocument svg, List<PointPosition> presetPolygons)
        {
            if (presetPolygons.Any())
            {
                var group = svg.AddGroup();
                group.Id = SvgPadLayerId;
                foreach (var poly in presetPolygons)
                {
                    var svgcontainer = SvgAppend.PadToSvg.PadToSvgAppend(group, poly, _stage.LevelScale);
                    if (!object.ReferenceEquals(null, svgcontainer))
                    {
                        var padId = (int)poly.PadId;

                        svgcontainer.Id = string.Format(SvgItemIdPadFormat, padId);
                        svgcontainer.AddClass("gelib-pad");

                        var scaledPos = poly.Origin.Clone().Scale(1 / _stage.LevelScale);

                        svgcontainer.SetDataAttribute("nc-x", poly.Origin.X.ToString(StandardDoubleToStringFormat));
                        svgcontainer.SetDataAttribute("nc-y", poly.Origin.Y.ToString(StandardDoubleToStringFormat));
                        svgcontainer.SetDataAttribute("nc-z", poly.Origin.Z.ToString(StandardDoubleToStringFormat));

                        svgcontainer.SetDataAttribute("sc-x", scaledPos.X.ToString(StandardDoubleToStringFormat));
                        svgcontainer.SetDataAttribute("sc-y", scaledPos.Y.ToString(StandardDoubleToStringFormat));
                        svgcontainer.SetDataAttribute("sc-z", scaledPos.Z.ToString(StandardDoubleToStringFormat));

                        svgcontainer.SetDataAttribute("room-id", poly.Room.ToString());

                        // check for linked ai script
                        if (_context.PadIdToAiCommandBlock.ContainsKey(padId))
                        {
                            var aiIdJson = string.Join(", ", _context.PadIdToAiCommandBlock[padId].OrderBy(x => x).Select(x => x.ToString()));
                            svgcontainer.SetDataAttribute("x-ai-id", $"[{aiIdJson}]");
                        }
                    }
                }
            }
        }

        private void AddStanGroupToSvgDoc(SvgDocument svg, List<HullPoints> tilePolygons)
        {
            var group1 = svg.AddGroup();
            group1.Id = SvgStanLayerId;
            foreach (var poly in tilePolygons)
            {
                if (object.ReferenceEquals(null, poly.Points))
                {
                    throw new NullReferenceException();
                }

                var polyline = group1.AddPolyLine();

                polyline.AddClass("svg-logical-item");
                polyline.AddClass("gelib-stan");

                polyline.Id = string.Format(SvgItemIdTileFormat, poly.Room, poly.OrderIndex);
                polyline.SetPoints(poly.Points.To1dArray(), StandardDoubleToStringFormat);

                polyline.SetDataAttribute("n-min-x", poly.NaturalMin.X.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("n-max-x", poly.NaturalMax.X.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("n-min-y", poly.NaturalMin.Y.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("n-max-y", poly.NaturalMax.Y.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("n-min-z", poly.NaturalMin.Z.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("n-max-z", poly.NaturalMax.Z.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-min-x", poly.ScaledMin.X.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-max-x", poly.ScaledMax.X.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-min-y", poly.ScaledMin.Y.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-max-y", poly.ScaledMax.Y.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-min-z", poly.ScaledMin.Z.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("s-max-z", poly.ScaledMax.Z.ToString(StandardDoubleToStringFormat));
                polyline.SetDataAttribute("room-id", poly.Room.ToString());
            }
        }

        private void AddIntroGroupToSvgDoc(SvgDocument svg, List<PointPosition> introPolygons)
        {
            if (introPolygons.Any())
            {
                var group = svg.AddGroup();
                group.Id = SvgSetupIntroLayerId;
                foreach (var poly in introPolygons)
                {
                    var point = poly.Origin.To2DXZ().Scale(1.0 / _stage.LevelScale);

                    var container = group.AddGroup();

                    container.AddClass("svg-logical-item");

                    container.Id = string.Format(SvgItemIdSetupIntroFormat, poly.Room, poly.OrderIndex);

                    var path = container.AddPath();

                    path.Stroke = "#00ffff";
                    path.StrokeWidth = 3;
                    path.Fill = "#ffff00";

                    // spawn star
                    path.D = "M 56.419353,49.677419 31.397828,59.992939 36.287734,86.612021 18.745014,66.002808 -5.060173,78.879133 9.119355,55.826418 -10.482966,37.165343 l 26.30615,6.361852 11.690287,-24.409504 2.078567,26.984556 z";

                    double halfw = 36;
                    double halfh = 39;
                    double rotAngle = 45;

                    path.Transform = $"translate({point.X - halfw}, {point.Y - halfh}) rotate({rotAngle} {halfw} {halfh}) scale(2)";

                    container.SetDataAttribute("nc-x", poly.Origin.X.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("nc-y", poly.Origin.Y.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("nc-z", poly.Origin.Z.ToString(StandardDoubleToStringFormat));

                    var scaledPos = poly.Origin.Clone().Scale(1 / _stage.LevelScale);

                    container.SetDataAttribute("sc-x", scaledPos.X.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("sc-y", scaledPos.Y.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("sc-z", scaledPos.Z.ToString(StandardDoubleToStringFormat));

                    container.SetDataAttribute("room-id", poly.Room.ToString());
                }
            }
        }

        private void AddPathWaypointGroupToSvgDoc(SvgDocument svg, List<RenderLine> pathWaypointLines)
        {
            if (pathWaypointLines.Any())
            {
                var group = svg.AddGroup();
                group.Id = SvgWaypointLayerId;

                foreach (var renderLine in pathWaypointLines)
                {
                    var p1 = renderLine.P1.Scale(1.0 / _stage.LevelScale);
                    var p2 = renderLine.P2.Scale(1.0 / _stage.LevelScale);

                    var container = group.AddGroup();

                    container.AddClass("svg-logical-item");

                    container.Id = string.Format(SvgItemIdPathWaypointFormat, renderLine.WaypointIndex, renderLine.TableIndex);

                    var line = container.AddLine();

                    line.SetX1(p1.X, StandardDoubleToStringFormat);
                    line.SetY1(p1.Z, StandardDoubleToStringFormat);
                    line.SetX2(p2.X, StandardDoubleToStringFormat);
                    line.SetY2(p2.Z, StandardDoubleToStringFormat);

                    // style is managed via css
                    line.AddClass("gelib-wpline");

                    container.SetDataAttribute("n-min-x", renderLine.Bbox.MinX.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("n-max-x", renderLine.Bbox.MaxX.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("n-min-y", renderLine.Bbox.MinY.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("n-max-y", renderLine.Bbox.MaxY.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("n-min-z", renderLine.Bbox.MinZ.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("n-max-z", renderLine.Bbox.MaxZ.ToString(StandardDoubleToStringFormat));

                    var scaledPos = renderLine.Bbox.Scale(1 / _stage.LevelScale);

                    container.SetDataAttribute("s-min-x", scaledPos.MinX.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("s-max-x", scaledPos.MaxX.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("s-min-y", scaledPos.MinY.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("s-max-y", scaledPos.MaxY.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("s-min-z", scaledPos.MinZ.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("s-max-z", scaledPos.MaxZ.ToString(StandardDoubleToStringFormat));

                    container.SetDataAttribute("room-id-p1", renderLine.Pad1RoomId.ToString());
                    container.SetDataAttribute("room-id-p2", renderLine.Pad2RoomId.ToString());
                }
            }
        }

        private void AddPatrolPathGroupToSvgDoc(SvgDocument svg, List<RenderPolyline> patrolPathLines)
        {
            if (!patrolPathLines.Any())
            {
                return;
            }

            var group = svg.AddGroup();
            group.Id = SvgPatrolPathLayerId;

            foreach (var pathset in patrolPathLines)
            {
                var container = group.AddGroup();
                container.AddClass("svg-logical-item");

                container.Id = string.Format(SvgItemIdPatrolPathFormat, pathset.OrderIndex);

                var scaledPoints = pathset.Points.Select(x => x.Scale(1.0 / _stage.LevelScale).To2DXZ()).To1dArray();

                var polyline = container.AddPolyLine();
                polyline.SetPoints(scaledPoints.ToArray(), StandardDoubleToStringFormat);

                // style is managed via css
                polyline.AddClass("gelib-patrol");
                polyline.FillOpacity = 0;

                container.SetDataAttribute("n-min-x", pathset.Bbox.MinX.ToString(StandardDoubleToStringFormat));
                container.SetDataAttribute("n-max-x", pathset.Bbox.MaxX.ToString(StandardDoubleToStringFormat));
                container.SetDataAttribute("n-min-y", pathset.Bbox.MinY.ToString(StandardDoubleToStringFormat));
                container.SetDataAttribute("n-max-y", pathset.Bbox.MaxY.ToString(StandardDoubleToStringFormat));
                container.SetDataAttribute("n-min-z", pathset.Bbox.MinZ.ToString(StandardDoubleToStringFormat));
                container.SetDataAttribute("n-max-z", pathset.Bbox.MaxZ.ToString(StandardDoubleToStringFormat));

                var scaledPos = pathset.Bbox.Scale(1 / _stage.LevelScale);

                container.SetDataAttribute("s-min-x", scaledPos.MinX.ToString(StandardDoubleToStringFormat));
                container.SetDataAttribute("s-max-x", scaledPos.MaxX.ToString(StandardDoubleToStringFormat));
                container.SetDataAttribute("s-min-y", scaledPos.MinY.ToString(StandardDoubleToStringFormat));
                container.SetDataAttribute("s-max-y", scaledPos.MaxY.ToString(StandardDoubleToStringFormat));
                container.SetDataAttribute("s-min-z", scaledPos.MinZ.ToString(StandardDoubleToStringFormat));
                container.SetDataAttribute("s-max-z", scaledPos.MaxZ.ToString(StandardDoubleToStringFormat));

                // check for linked ai script
                var patrolId = (int)pathset.OrderIndex;
                if (_context.PathIdToAiCommandBlock.ContainsKey(patrolId))
                {
                    var aiIdJson = string.Join(", ", _context.PathIdToAiCommandBlock[patrolId].OrderBy(x => x).Select(x => x.ToString()));
                    container.SetDataAttribute("x-ai-id", $"[{aiIdJson}]");
                }
            }
        }

        /// <summary>
        /// Adds prop information as attributes to SVG node.
        /// </summary>
        /// <param name="container">SVG element to add attributes on.</param>
        /// <param name="prop">Prop containing setup object.</param>
        private void AddPropAttributes(SvgContainer container, PropPointPosition prop)
        {
            if (object.ReferenceEquals(null, prop.SetupObject))
            {
                throw new NullReferenceException();
            }

            // guard does not implement SetupObjectGenericBase
            SetupObjectGenericBase? baseGenericObject = prop.SetupObject as SetupObjectGenericBase;

            if (object.ReferenceEquals(null, baseGenericObject))
            {
                // nothing to do for guards
                return;
            }

            container.SetDataAttribute("propdef-name", prop.SetupObject.Type.ToString());
            container.SetDataAttribute("propdef-id", ((int)prop.SetupObject.Type).ToString());

            if (baseGenericObject != null)
            {
                var propid = (PropId)baseGenericObject.ObjectId;
                container.SetDataAttribute("prop-name", propid.ToString());
                container.SetDataAttribute("prop-id", ((int)propid).ToString());
            }
        }

        private void AddAiListsToSvgDoc(SvgDocument svg, List<AiCommandBlock> scripts)
        {
            // copy to permute list.
            var scriptList = scripts.ToArray().ToList();

            // Global ai lists won't be included in the setup file, so manually add those.
            foreach (var id in _context.RefAiListId)
            {
                if (id < 0x400)
                {
                    scriptList.Add(GlobalAiScript.GetGlobalAiScript((GlobalAiList)id));
                }
            }

            scriptList = scriptList.OrderBy(x => x.Id).ToList();

            if (!scriptList.Any())
            {
                return;
            }

            var group = svg.AddGroup();
            group.Id = SvgAiListsId;

            foreach (var script in scriptList)
            {
                var container = group.AddGroup();
                container.Id = string.Format(SvgAiScriptFormat, script.Id);

                int lineIndex = 0;
                foreach (var command in script.Commands)
                {
                    lineIndex++;
                    var text = container.AddText();
                    var sb = new StringBuilder();

                    sb.Append($"{lineIndex:000}: [0x{command.CommandId:x2}] {command.DecompName}");

                    if (command is IAiFixedCommand fcommand)
                    {
                        if (fcommand.CommandParameters.Any(x => x.ParameterName == null))
                        {
                            throw new NullReferenceException();
                        }

                        var parameterKvp = fcommand.CommandParameters.ToDictionary(k => k.ParameterName!, v => v.ValueToString());

                        if (parameterKvp.Any())
                        {
                            sb.Append("(");
                            sb.Append(string.Join(", ", parameterKvp.Select(x => $"{x.Key}={x.Value}")));
                            sb.Append(")");
                        }

                        // Add any back references to parameter objects.
                        foreach (var p in fcommand.CommandParameters)
                        {
                            if (p.ParameterName == "chr_num")
                            {
                                if (_context.AiCommandBlockToChrId.ContainsKey(script.Id))
                                {
                                    var backId = p.GetIntValue(Lib.Architecture.ByteOrder.LittleEndien);
                                    if (_context.ChrIdToAiCommandBlock.ContainsKey(backId))
                                    {
                                        text.SetDataAttribute("x-chr-id", backId.ToString());
                                    }
                                }
                            }
                            else if (p.ParameterName == "pad"
                                || p.ParameterName == "chr_preset"
                                || p.ParameterName == "pad_preset")
                            {
                                if (_context.AiCommandBlockToPadId.ContainsKey(script.Id))
                                {
                                    var backId = p.GetIntValue(Lib.Architecture.ByteOrder.LittleEndien);
                                    if (_context.PadIdToAiCommandBlock.ContainsKey(backId))
                                    {
                                        text.SetDataAttribute("x-pad-id", backId.ToString());
                                    }
                                }
                            }
                            else if (p.ParameterName == "path_num")
                            {
                                if (_context.AiCommandBlockToPathId.ContainsKey(script.Id))
                                {
                                    var backId = p.GetIntValue(Lib.Architecture.ByteOrder.LittleEndien);
                                    if (_context.PathIdToAiCommandBlock.ContainsKey(backId))
                                    {
                                        text.SetDataAttribute("x-patrol-id", backId.ToString());
                                    }
                                }
                            }
                        }
                    }

                    text.Text = sb.ToString();
                }
            }
        }
    }
}
