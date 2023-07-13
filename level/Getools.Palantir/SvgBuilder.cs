using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;
using Getools.Lib.Extensions;
using SvgLib;
using Microsoft.Win32.SafeHandles;
using static Getools.Lib.Kaitai.Gen.Avtx;
using Getools.Lib.Game.Asset.SetupObject;
using System.Text.RegularExpressions;
using Getools.Lib.Game.Asset.Stan;
using System.Drawing;
using Getools.Lib.Game.Enums;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Getools.Lib.Game.Asset.Intro;
using Getools.Palantir.Render;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.Bg;
using Getools.Lib.Kaitai.Gen;

namespace Getools.Palantir
{
    /// <summary>
    /// Internal helper class. Used to translated the processed stage data to an output SVG document.
    /// </summary>
    internal class SvgBuilder
    {
        // three decimal places
        private const string StandardDoubleToStringFormat = "0.###";

        private const string SvgBgLayerId = "svg-bg-room-layer";
        private const string SvgStanLayerId = "svg-stan-tile-layer";
        private const string SvgPadLayerId = "svg-pad-layer";

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

        private const string SvgItemIdRoomFormat = "svg-room-{0}";
        private const string SvgItemIdTileFormat = "svg-room-{0}-tile-{1}";
        private const string SvgItemIdPadFormat = "svg-room-{0}-pad-{1}";
        private const string SvgItemIdSetupAlarmFormat = "svg-room-{0}-setup-alarm-{1}";
        private const string SvgItemIdSetupAmmoFormat = "svg-room-{0}-setup-ammo-{1}";
        private const string SvgItemIdSetupAircraftFormat = "svg-room-{0}-setup-aircraft-{1}";
        private const string SvgItemIdSetupBodyArmorFormat = "svg-room-{0}-setup-bodyarmor-{1}";
        private const string SvgItemIdSetupChrFormat = "svg-room-{0}-setup-chr-{1}";
        private const string SvgItemIdSetupCctvFormat = "svg-room-{0}-setup-cctv-{1}";
        private const string SvgItemIdSetupCollectableFormat = "svg-room-{0}-setup-collectable-{1}";
        private const string SvgItemIdSetupDoorFormat = "svg-room-{0}-setup-door-{1}";
        private const string SvgItemIdSetupDroneFormat = "svg-room-{0}-setup-drone-{1}";
        private const string SvgItemIdSetupKeyFormat = "svg-room-{0}-setup-key-{1}";
        private const string SvgItemIdSetupSafeFormat = "svg-room-{0}-setup-safe-{1}";
        private const string SvgItemIdSetupSingleMonitorFormat = "svg-room-{0}-setup-singlemonitor-{1}";
        private const string SvgItemIdSetupStandardPropFormat = "svg-room-{0}-setup-prop-{1}";
        private const string SvgItemIdSetupTankFormat = "svg-room-{0}-setup-tank-{1}";

        private const string SvgItemIdSetupIntroFormat = "svg-room-{0}-setup-intro-{1}";

        private const double MaxPixelSizeError = 10000000;

        private readonly ProcessedStageDataContext _context;
        private readonly Stage _stage;

        public SvgBuilder(ProcessedStageDataContext context, Stage stage)
        {
            _context = context;
            _stage = stage;
        }

        public int OutputWidth { get; set; } = 2048;

        /// <summary>
        /// Final processing step, should be called after the stage has been "sliced".
        /// </summary>
        /// <returns>SVG.</returns>
        /// <exception cref="Exception"></exception>
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

            svg.SetStylesheet(cssText.ToString());

            AddStanGroupToSvgDoc(svg, _context.TilePolygons);

            // room boundaries need to be above the tiles.
            AddBgGroupToSvgDoc(svg, _context.RoomPolygons);

            AddPadGroupToSvgDoc(svg, _context.PresetPolygons);

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

            return svg;
        }

        private void AddSetupGroupToSvgDoc(Dictionary<PropDef, List<PropPosition>> collection, PropDef key, SvgDocument svg, string groupId, string itemFormatString)
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
                        svgprop.Id = string.Format(itemFormatString, poly.Room, poly.OrderIndex);

                        AddPropAttributes(svgprop, poly);

                        svgprop.SetDataAttribute("nc-x", poly.Origin.X.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("nc-y", poly.Origin.Y.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("nc-z", poly.Origin.Z.ToString(StandardDoubleToStringFormat));

                        var scaledPos = poly.Origin.Clone().Scale(1 / _stage.LevelScale);

                        svgprop.SetDataAttribute("sc-x", scaledPos.X.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("sc-y", scaledPos.Y.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("sc-z", scaledPos.Z.ToString(StandardDoubleToStringFormat));

                        svgprop.SetDataAttribute("room-id", poly.Room.ToString());
                    }
                }
            }
        }

        private void AddBgGroupToSvgDoc(SvgDocument svg, List<CollectionHullSvgPoints> roomPolygons)
        {
            var group2 = svg.AddGroup();
            group2.Id = SvgBgLayerId;
            foreach (var poly in roomPolygons)
            {
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

        private void AddPadGroupToSvgDoc(SvgDocument svg, List<RenderPosition> presetPolygons)
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
                        svgcontainer.Id = string.Format(SvgItemIdPadFormat, poly.Room, poly.OrderIndex);
                        svgcontainer.AddClass("gelib-pad");

                        var scaledPos = poly.Origin.Clone().Scale(1 / _stage.LevelScale);

                        svgcontainer.SetDataAttribute("nc-x", poly.Origin.X.ToString(StandardDoubleToStringFormat));
                        svgcontainer.SetDataAttribute("nc-y", poly.Origin.Y.ToString(StandardDoubleToStringFormat));
                        svgcontainer.SetDataAttribute("nc-z", poly.Origin.Z.ToString(StandardDoubleToStringFormat));

                        svgcontainer.SetDataAttribute("sc-x", scaledPos.X.ToString(StandardDoubleToStringFormat));
                        svgcontainer.SetDataAttribute("sc-y", scaledPos.Y.ToString(StandardDoubleToStringFormat));
                        svgcontainer.SetDataAttribute("sc-z", scaledPos.Z.ToString(StandardDoubleToStringFormat));
                        svgcontainer.SetDataAttribute("room-id", poly.Room.ToString());
                    }
                }
            }
        }

        private void AddStanGroupToSvgDoc(SvgDocument svg, List<CollectionHullSvgPoints> tilePolygons)
        {
            var group1 = svg.AddGroup();
            group1.Id = SvgStanLayerId;
            foreach (var poly in tilePolygons)
            {
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

        private void AddIntroGroupToSvgDoc(SvgDocument svg, List<RenderPosition> introPolygons)
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


        /// <summary>
        /// Adds prop information as attributes to SVG node.
        /// </summary>
        /// <param name="container">SVG element to add attributes on.</param>
        /// <param name="prop">Prop containing setup object.</param>
        /// <exception cref="NullReferenceException"></exception>
        private void AddPropAttributes(SvgContainer container, PropPosition prop)
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
    }
}
