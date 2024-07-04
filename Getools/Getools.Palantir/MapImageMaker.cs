using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
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
using Microsoft.Win32.SafeHandles;
using SvgLib;
using static Getools.Lib.Kaitai.Gen.Avtx;

namespace Getools.Palantir
{
    /// <summary>
    /// Class to convert from csharp library game data to output to an image format.
    /// </summary>
    public class MapImageMaker
    {
        // three decimal places
        private const string StandardDoubleToStringFormat = "0.###";

        private const double MaxPixelSizeError = 10000000;

        private static Regex _matchDigitsRegex = new Regex("([0-9]+)([a-zA-Z])([0-9]?)");

        private readonly Stage _stage;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapImageMaker"/> class.
        /// </summary>
        /// <param name="stage">Stage data.</param>
        public MapImageMaker(Stage stage)
        {
            _stage = stage;
        }

        /// <summary>
        /// Gets or sets the output width.
        /// Height will be automatically set based on stage data and map width/height ratio.
        /// </summary>
        public int OutputWidth { get; set; } = 2048;

        /// <summary>
        /// Create an SVG image from stage data within vertical bounds.
        /// </summary>
        /// <param name="zmin">Unscaled lower vertical boundary.</param>
        /// <param name="zmax">Unscaled upper vertical boundary.</param>
        /// <returns>Svg.</returns>
        public SvgDocument BoundingZToImageSvg(double zmin, double zmax)
        {
            var context = new ProcessedStageData()
            {
                OutputFormat = OutputImageFormat.Svg,
                Mode = SliceMode.BoundingBox,
                Zmin = double.MinValue,
                Zmax = double.MaxValue,
            };

            SliceCommon(context);
            var svgbuilder = new SvgBuilder(context, _stage)
            {
                OutputWidth = OutputWidth,
            };
            return svgbuilder.BuildSvg();
        }

        /// <summary>
        /// Create an SVG image from stage data along a single plane.
        /// </summary>
        /// <param name="z">Unscaled vertical offset.</param>
        /// <returns>Svg.</returns>
        public SvgDocument SliceZToImageSvg(double z)
        {
            var context = new ProcessedStageData()
            {
                OutputFormat = OutputImageFormat.Svg,
                Mode = SliceMode.Slice,
                Z = z,
            };

            SliceCommon(context);
            var svgbuilder = new SvgBuilder(context, _stage)
            {
                OutputWidth = OutputWidth,
            };
            return svgbuilder.BuildSvg();
        }

        /// <summary>
        /// Create an SVG image from stage data.
        /// </summary>
        /// <returns>Svg.</returns>
        public SvgDocument FullImageSvg()
        {
            var context = new ProcessedStageData()
            {
                OutputFormat = OutputImageFormat.Svg,
                Mode = SliceMode.Unbound,
                Zmin = double.MinValue,
                Zmax = double.MaxValue,
            };

            SliceCommon(context);
            var svgbuilder = new SvgBuilder(context, _stage)
            {
                OutputWidth = OutputWidth,
            };
            return svgbuilder.BuildSvg();
        }

        /// <summary>
        /// Process stage data into csharp types.
        /// </summary>
        /// <returns>Unbound stage data.</returns>
        public ProcessedStageData GetFullRawStage()
        {
            var context = new ProcessedStageData()
            {
                OutputFormat = OutputImageFormat.CsharpRaw,
                Mode = SliceMode.Unbound,
                Zmin = double.MinValue,
                Zmax = double.MaxValue,
            };

            SliceCommon(context);

            return context;
        }

        /// <summary>
        /// Entry point to begin processing stage data.
        /// </summary>
        /// <param name="context">Current processing context.</param>
        private void SliceCommon(ProcessedStageData context)
        {
            if (OutputWidth < 1)
            {
                throw new InvalidOperationException($"{nameof(OutputWidth)} must be a positive integer");
            }

            /***
             * Process the three main sources of information for the stage.
             * This will collect min/max values (native and scaled).
             * If a section is null, it will be skipped in the method.
             */

            SliceProcessBg(context);
            SliceProcessStan(context);
            SliceProcessSetup(context);

            //// After processing the data, reset any unset min/max value to zero.

            context.ScaledMax.X = (context.ScaledMax.X == double.MinValue) ? 0 : context.ScaledMax.X;
            context.ScaledMax.Y = (context.ScaledMax.Y == double.MinValue) ? 0 : context.ScaledMax.Y;
            context.ScaledMax.Z = (context.ScaledMax.Z == double.MinValue) ? 0 : context.ScaledMax.Z;

            context.ScaledMin.X = (context.ScaledMin.X == double.MaxValue) ? 0 : context.ScaledMin.X;
            context.ScaledMin.Y = (context.ScaledMin.Y == double.MaxValue) ? 0 : context.ScaledMin.Y;
            context.ScaledMin.Z = (context.ScaledMin.Z == double.MaxValue) ? 0 : context.ScaledMin.Z;

            context.NativeMax.X = (context.NativeMax.X == double.MinValue) ? 0 : context.NativeMax.X;
            context.NativeMax.Y = (context.NativeMax.Y == double.MinValue) ? 0 : context.NativeMax.Y;
            context.NativeMax.Z = (context.NativeMax.Z == double.MinValue) ? 0 : context.NativeMax.Z;

            context.NativeMin.X = (context.NativeMin.X == double.MaxValue) ? 0 : context.NativeMin.X;
            context.NativeMin.Y = (context.NativeMin.Y == double.MaxValue) ? 0 : context.NativeMin.Y;
            context.NativeMin.Z = (context.NativeMin.Z == double.MaxValue) ? 0 : context.NativeMin.Z;
        }

        /// <summary>
        /// Helper method called from <see cref="SliceCommon(ProcessedStageData)"/>.
        /// </summary>
        /// <param name="context">Current processing context.</param>
        private void SliceProcessBg(ProcessedStageData context)
        {
            if (object.ReferenceEquals(null, _stage.Bg))
            {
                return;
            }

            if (object.ReferenceEquals(null, _stage.Bg.RoomDataTable))
            {
                throw new NullReferenceException();
            }

            Console.WriteLine("MapImageMaker: Found bg");

            foreach (var roomData in _stage.Bg.RoomDataTable.Entries)
            {
                if (object.ReferenceEquals(null, roomData.Points) || !roomData.Points.Any())
                {
                    // Console.WriteLine($"skipping empty room # {roomData.OrderIndex}");
                    continue;
                }

                //// Console.WriteLine($"process room # {roomData.OrderIndex}");

                var roomScaledMin = Coord3dd.MaxValue.Clone();
                var roomScaledMax = Coord3dd.MinValue.Clone();
                var roomNativeMin = Coord3dd.MaxValue.Clone();
                var roomNativeMax = Coord3dd.MinValue.Clone();

                if (object.ReferenceEquals(null, roomData.Coord))
                {
                    throw new NullReferenceException();
                }

                var center = roomData.Coord;
                var roomPoints = new List<Coord3dd>();

                foreach (var vtx in roomData.Points)
                {
                    if (object.ReferenceEquals(null, vtx.Ob))
                    {
                        throw new NullReferenceException();
                    }

                    var roomPoint = new Coord3dd()
                    {
                        X = (double)center.X + (double)vtx.Ob.X,
                        Y = (double)center.Y + (double)vtx.Ob.Y,
                        Z = (double)center.Z + (double)vtx.Ob.Z,
                    };

                    if (context.Mode == SliceMode.BoundingBox)
                    {
                        if (!(roomPoint.Y >= context.Zmin!.Value))
                        {
                            continue;
                        }

                        if (!(roomPoint.Y <= context.Zmax!.Value))
                        {
                            continue;
                        }

                        Getools.Lib.Math.Compare.SetMinMaxCompareY(roomNativeMin, roomNativeMax, roomPoint.Y, context.Zmin!.Value, context.Zmax!.Value);
                        Getools.Lib.Math.Compare.SetMinMaxCompareY(context.NativeMin, context.NativeMax, roomPoint.Y, context.Zmin!.Value, context.Zmax!.Value);
                    }
                    else if (context.Mode == SliceMode.Unbound)
                    {
                        Getools.Lib.Math.Compare.SetUnboundCompareY(roomNativeMin, roomNativeMax, roomPoint.Y);
                        Getools.Lib.Math.Compare.SetUnboundCompareY(context.NativeMin, context.NativeMax, roomPoint.Y);
                    }

                    Getools.Lib.Math.Compare.SetUnboundCompareX(roomNativeMin, roomNativeMax, roomPoint.X);
                    Getools.Lib.Math.Compare.SetUnboundCompareX(context.NativeMin, context.NativeMax, roomPoint.X);

                    Getools.Lib.Math.Compare.SetUnboundCompareZ(roomNativeMin, roomNativeMax, roomPoint.Z);
                    Getools.Lib.Math.Compare.SetUnboundCompareZ(context.NativeMin, context.NativeMax, roomPoint.Z);

                    var scaled = roomPoint.Scale(1.0 / _stage.LevelScale);

                    Getools.Lib.Math.Compare.SetUnboundCompareX(roomScaledMin, roomScaledMax, scaled.X);
                    Getools.Lib.Math.Compare.SetUnboundCompareX(context.ScaledMin, context.ScaledMax, scaled.X);
                    Getools.Lib.Math.Compare.SetUnboundCompareY(roomScaledMin, roomScaledMax, scaled.Y);
                    Getools.Lib.Math.Compare.SetUnboundCompareY(context.ScaledMin, context.ScaledMax, scaled.Y);
                    Getools.Lib.Math.Compare.SetUnboundCompareZ(roomScaledMin, roomScaledMax, scaled.Z);
                    Getools.Lib.Math.Compare.SetUnboundCompareZ(context.ScaledMin, context.ScaledMax, scaled.Z);

                    roomPoints.Add(roomPoint);
                }

                //// Console.WriteLine($"found {roomPoints.Count} points");

                List<Coord2dd> convexHull;

                if (context.Mode == SliceMode.Slice)
                {
                    convexHull = MeshToConvexHullAtY(roomPoints, context.Z!.Value);
                }
                else if (context.Mode == SliceMode.BoundingBox)
                {
                    convexHull = MeshToConvexHullYBounds(roomPoints, context.Zmin!.Value, context.Zmax!.Value);
                }
                else
                {
                    convexHull = MeshToConvexHullYBounds(roomPoints, double.MinValue, double.MaxValue);
                }

                //// Console.WriteLine($"convexHull result: {convexHull.Count} points");

                if (!convexHull.Any())
                {
                    continue;
                }

                var svgScaledPoints = new List<Coord2dd>();

                foreach (var p in convexHull)
                {
                    var scaled = new Coord2dd(p.X, p.Y).Scale(1.0 / _stage.LevelScale);

                    svgScaledPoints.Add(scaled);
                }

                // Add first point at end of list to create closed svg path.
                // Points are adjusted later, be careful not to link this
                // to an existing point.
                svgScaledPoints.Add(svgScaledPoints[0].Clone()); // created from scaled points

                var collection = new HullPoints()
                {
                    OrderIndex = roomData.OrderIndex,
                    Room = roomData.OrderIndex,
                    Points = svgScaledPoints,
                    Source = Enums.PolygonSource.Bg,
                };

                if (roomNativeMin.X < double.MaxValue)
                {
                    collection.NaturalMin.X = roomNativeMin.X;
                    collection.ScaledMin.X = roomScaledMin.X;
                }

                if (roomNativeMax.X > double.MinValue)
                {
                    collection.NaturalMax.X = roomNativeMax.X;
                    collection.ScaledMax.X = roomScaledMax.X;
                }

                if (roomNativeMin.Y < double.MaxValue)
                {
                    collection.NaturalMin.Y = roomNativeMin.Y;
                    collection.ScaledMin.Y = roomNativeMin.Y * (1 / _stage.LevelScale);
                }

                if (roomNativeMax.Y > double.MinValue)
                {
                    collection.NaturalMax.Y = roomNativeMax.Y;
                    collection.ScaledMax.Y = roomNativeMax.Y * (1 / _stage.LevelScale);
                }

                if (roomNativeMin.Z < double.MaxValue)
                {
                    collection.NaturalMin.Z = roomNativeMin.Z;
                    collection.ScaledMin.Z = roomScaledMin.Z;
                }

                if (roomNativeMax.Z > double.MinValue)
                {
                    collection.NaturalMax.Z = roomNativeMax.Z;
                    collection.ScaledMax.Z = roomScaledMax.Z;
                }

                context.RoomPolygons.Add(collection);

                // Console.WriteLine($"room min/max: {roomMinScaledX},{roomMinScaledY} and {roomMaxScaledX},{roomMaxScaledY}");
            }
        }

        /// <summary>
        /// Helper method called from <see cref="SliceCommon(ProcessedStageData)"/>.
        /// </summary>
        /// <param name="context">Current processing context.</param>
        private void SliceProcessStan(ProcessedStageData context)
        {
            if (object.ReferenceEquals(null, _stage.Stan))
            {
                return;
            }

            Console.WriteLine("MapImageMaker: Found stan");

            foreach (var tile in _stage.Stan.Tiles)
            {
                if (object.ReferenceEquals(null, tile.Points) || !tile.Points.Any())
                {
                    // Console.WriteLine($"skipping empty tile # {tile.OrderIndex}");
                    continue;
                }

                ////Console.WriteLine($"process tile # {tile.OrderIndex}");

                var tileScaledMin = Coord3dd.MaxValue.Clone();
                var tileScaledMax = Coord3dd.MinValue.Clone();
                var tileNativeMin = Coord3dd.MaxValue.Clone();
                var tileNativeMax = Coord3dd.MinValue.Clone();

                var levelTilePoints = new List<Coord3dd>();

                foreach (var point in tile.Points)
                {
                    var roomPoint = new Coord3dd()
                    {
                        X = (double)point.X,
                        Y = (double)point.Y,
                        Z = (double)point.Z,
                    };

                    if (context.Mode == SliceMode.BoundingBox)
                    {
                        if (!(roomPoint.Y >= context.Zmin!.Value))
                        {
                            continue;
                        }

                        if (!(roomPoint.Y <= context.Zmax!.Value))
                        {
                            continue;
                        }

                        Getools.Lib.Math.Compare.SetMinMaxCompareY(tileNativeMin, tileNativeMax, roomPoint.Y, context.Zmin!.Value, context.Zmax!.Value);
                        Getools.Lib.Math.Compare.SetMinMaxCompareY(context.NativeMin, context.NativeMax, roomPoint.Y, context.Zmin!.Value, context.Zmax!.Value);
                    }
                    else if (context.Mode == SliceMode.Unbound)
                    {
                        Getools.Lib.Math.Compare.SetUnboundCompareY(tileNativeMin, tileNativeMax, roomPoint.Y);
                        Getools.Lib.Math.Compare.SetUnboundCompareY(context.NativeMin, context.NativeMax, roomPoint.Y);
                    }

                    Getools.Lib.Math.Compare.SetUnboundCompareX(tileNativeMin, tileNativeMax, roomPoint.X);
                    Getools.Lib.Math.Compare.SetUnboundCompareX(context.NativeMin, context.NativeMax, roomPoint.X);

                    Getools.Lib.Math.Compare.SetUnboundCompareZ(tileNativeMin, tileNativeMax, roomPoint.Z);
                    Getools.Lib.Math.Compare.SetUnboundCompareZ(context.NativeMin, context.NativeMax, roomPoint.Z);

                    var scaled = roomPoint.Scale(1.0 / _stage.LevelScale);

                    Getools.Lib.Math.Compare.SetUnboundCompareX(tileScaledMin, tileScaledMax, scaled.X);
                    Getools.Lib.Math.Compare.SetUnboundCompareX(context.ScaledMin, context.ScaledMax, scaled.X);
                    Getools.Lib.Math.Compare.SetUnboundCompareY(tileScaledMin, tileScaledMax, scaled.Y);
                    Getools.Lib.Math.Compare.SetUnboundCompareY(context.ScaledMin, context.ScaledMax, scaled.Y);
                    Getools.Lib.Math.Compare.SetUnboundCompareZ(tileScaledMin, tileScaledMax, scaled.Z);
                    Getools.Lib.Math.Compare.SetUnboundCompareZ(context.ScaledMin, context.ScaledMax, scaled.Z);

                    levelTilePoints.Add(roomPoint);
                }

                ////Console.WriteLine($"found {levelTilePoints.Count} points");

                List<Coord2dd> convexHull;

                if (context.Mode == SliceMode.Slice)
                {
                    convexHull = MeshToConvexHullAtY(levelTilePoints, context.Z!.Value);
                }
                else if (context.Mode == SliceMode.BoundingBox)
                {
                    convexHull = MeshToConvexHullYBounds(levelTilePoints, context.Zmin!.Value, context.Zmax!.Value);
                }
                else
                {
                    convexHull = MeshToConvexHullYBounds(levelTilePoints, double.MinValue, double.MaxValue);
                }

                ////Console.WriteLine($"convexHull result: {convexHull.Count} points");

                if (!convexHull.Any())
                {
                    continue;
                }

                var svgScaledPoints = new List<Coord2dd>();

                foreach (var p in convexHull)
                {
                    var scaled = new Coord2dd(p.X, p.Y).Scale(1.0 / _stage.LevelScale);

                    svgScaledPoints.Add(scaled);
                }

                // Add first point at end of list to create closed svg path.
                // Points are adjusted later, be careful not to link this
                // to an existing point.
                svgScaledPoints.Add(svgScaledPoints[0].Clone()); // created from scaled points

                var collection = new HullPoints()
                {
                    OrderIndex = tile.OrderIndex,
                    Room = tile.Room,
                    Points = svgScaledPoints,
                    Source = Enums.PolygonSource.Stan,
                };

                if (tileNativeMin.X < double.MaxValue)
                {
                    collection.NaturalMin.X = tileNativeMin.X;
                    collection.ScaledMin.X = tileScaledMin.X;
                }

                if (tileNativeMax.X > double.MinValue)
                {
                    collection.NaturalMax.X = tileNativeMax.X;
                    collection.ScaledMax.X = tileScaledMax.X;
                }

                if (tileNativeMin.Y < double.MaxValue)
                {
                    collection.NaturalMin.Y = tileNativeMin.Y;
                    collection.ScaledMin.Y = tileNativeMin.Y * (1 / _stage.LevelScale);
                }

                if (tileNativeMax.Y > double.MinValue)
                {
                    collection.NaturalMax.Y = tileNativeMax.Y;
                    collection.ScaledMax.Y = tileNativeMax.Y * (1 / _stage.LevelScale);
                }

                if (tileNativeMin.Z < double.MaxValue)
                {
                    collection.NaturalMin.Z = tileNativeMin.Z;
                    collection.ScaledMin.Z = tileScaledMin.Z;
                }

                if (tileNativeMax.Z > double.MinValue)
                {
                    collection.NaturalMax.Z = tileNativeMax.Z;
                    collection.ScaledMax.Z = tileScaledMax.Z;
                }

                context.TilePolygons.Add(collection);

                //// Console.WriteLine($"tile min/max: {tileMinScaledX},{tileMinScaledY} and {tileMaxScaledX},{tileMaxScaledY}");
            }
        }

        /// <summary>
        /// Helper method called from <see cref="SliceCommon(ProcessedStageData)"/>.
        /// </summary>
        /// <param name="context">Current processing context.</param>
        private void SliceProcessSetup(ProcessedStageData context)
        {
            if (object.ReferenceEquals(null, _stage.Setup))
            {
                return;
            }

            if (context.Mode == SliceMode.BoundingBox || context.Mode == SliceMode.Unbound)
            {
                // process data below.
            }
            else
            {
                return;
            }

            Console.WriteLine("MapImageMaker: Found setup");

            if (object.ReferenceEquals(null, _stage.Setup.SectionPadList))
            {
                throw new NullReferenceException();
            }

            if (object.ReferenceEquals(null, _stage.Setup.SectionPad3dList))
            {
                throw new NullReferenceException();
            }

            var presets = _stage.Setup.SectionPadList.PadList;
            var presets3d = _stage.Setup.SectionPad3dList.Pad3dList;

            int setupObjectIndex = -1;
            byte roomId = 0;

            // Preprocess the AI scripts. This will look for references to setup objects so an association
            // can be tracked between object<->script.
            SetupProcessAi(context);

            if (object.ReferenceEquals(null, _stage.Setup.SectionObjects))
            {
                throw new NullReferenceException();
            }

            foreach (var setupObject in _stage.Setup.SectionObjects.Objects)
            {
                setupObjectIndex++;

                string? presetName = string.Empty;
                Pad? pad = null;

                var baseObject = setupObject as SetupObjectBase;

                // guard does not implement SetupObjectGenericBase
                SetupObjectGenericBase? baseGenericObject = setupObject as SetupObjectGenericBase;

                if (object.ReferenceEquals(null, baseObject))
                {
                    continue;
                }

                if (baseObject is not IHasPreset)
                {
                    continue;
                }

                var presetId = ((IHasPreset)setupObject).Preset;
                int usePresetId = -1;

                if (setupObject.Type == PropDef.Key || setupObject.Type == PropDef.Collectable)
                {
                    if ((((SetupObjectGenericBase)setupObject).Flags1 & 0x00004000) > 0)
                    {
                        // is attached to guard
                        continue;
                    }
                }

                switch (setupObject.Type)
                {
                    case Lib.Game.Enums.PropDef.Alarm: // fallthrough
                    case Lib.Game.Enums.PropDef.Aircraft: // fallthrough
                    case Lib.Game.Enums.PropDef.AmmoBox: // fallthrough
                    case Lib.Game.Enums.PropDef.Armour: // fallthrough
                    case Lib.Game.Enums.PropDef.Cctv: // fallthrough
                    case Lib.Game.Enums.PropDef.Collectable: // fallthrough
                    case Lib.Game.Enums.PropDef.Drone: // fallthrough
                    case Lib.Game.Enums.PropDef.Guard: // fallthrough
                    case Lib.Game.Enums.PropDef.Key: // fallthrough
                    case Lib.Game.Enums.PropDef.Safe:
                    case Lib.Game.Enums.PropDef.SingleMonitor:
                    case Lib.Game.Enums.PropDef.StandardProp:
                    case Lib.Game.Enums.PropDef.Tank:
                        {
                            if (presetId > 60000)
                            {
                                // "embedded preset"
                                // Not sure what the cutoff is, this might just be negative short.
                                // Values are 65535, 65534, 65533...
                                continue;
                            }
                            else if (presetId < presets.Count)
                            {
                                pad = presets[presetId];
                                usePresetId = presetId;
                            }
                            else if ((presetId >= 10000) && (presetId - 10000) < presets3d.Count)
                            {
                                pad = presets3d[presetId - 10000];
                                usePresetId = presetId - 10000;
                            }
                            else
                            {
                                throw new Exception("Could not resolve setup object point to preset (padlist) index");
                            }

                            break;
                        }

                    case Lib.Game.Enums.PropDef.Door:
                        {
                            //// important! door always uses bounding box preset

                            if (presetId < presets3d.Count)
                            {
                                pad = presets3d[presetId];
                                usePresetId = presetId;
                            }
                            else if ((presetId >= 10000) && (presetId - 10000) < presets3d.Count)
                            {
                                pad = presets3d[presetId - 10000];
                                usePresetId = presetId - 10000;
                            }
                            else
                            {
                                throw new Exception("Could not resolve setup object point to preset (padlist) index");
                            }

                            break;
                        }
                }

                if (object.ReferenceEquals(null, pad))
                {
                    continue;
                }

                // if this is a guard, add the init script to the tracked reference scripts, in case this is
                // a global script that needs to be included.
                if (setupObject.Type == PropDef.Guard)
                {
                    var guard = (SetupObjectGuard)setupObject;
                    context.RefAiListId.Add((int)guard.ActionPathAssignment);
                }

                if (usePresetId < 0)
                {
                    usePresetId = setupObjectIndex;
                }

                if (object.ReferenceEquals(null, pad.Position))
                {
                    throw new NullReferenceException();
                }

                if (object.ReferenceEquals(null, pad.Up))
                {
                    throw new NullReferenceException();
                }

                if (object.ReferenceEquals(null, pad.Look))
                {
                    throw new NullReferenceException();
                }

                // Points are scaled later, depending on type
                var propPosition = new PropPointPosition()
                {
                    OrderIndex = setupObjectIndex,
                    PadId = usePresetId,
                    SetupObject = setupObject,
                    Origin = pad.Position.ToCoord3dd(),
                    Up = pad.Up.ToCoord3dd(),
                    Look = pad.Look.ToCoord3dd(),
                    Type = setupObject.Type,
                    Prop = PropId.PROP_MAX,
                };

                // If this is a prop, set the prop id.
                if (!object.ReferenceEquals(null, baseGenericObject))
                {
                    propPosition.Prop = (PropId)baseGenericObject.ObjectId;
                }

                if (pad is Pad3d)
                {
                    var pad3d = (Pad3d)pad;

                    if (object.ReferenceEquals(null, pad3d.BoundingBox))
                    {
                        throw new NullReferenceException();
                    }

                    propPosition.Bbox = pad3d.BoundingBox.ToBoundingBoxd();
                }

                if (object.ReferenceEquals(null, pad.Name))
                {
                    throw new NullReferenceException();
                }

                // pad name is used to resolve the room id.
                presetName = pad.Name.GetString();

                //// Console.WriteLine($"Setup object point: {workingPoint}");

                if (!object.ReferenceEquals(null, _stage.Stan))
                {
                    TryGetRoomId(presetName, out roomId);
                    propPosition.Room = roomId;
                }

                bool withinBounds = false;

                if (propPosition.Origin.Y >= context.Zmin!.Value && propPosition.Origin.Y <= context.Zmax!.Value)
                {
                    withinBounds = true;
                }

                if (!withinBounds)
                {
                    continue;
                }

                if (context.SetupPolygonsCollection.ContainsKey(setupObject.Type))
                {
                    context.SetupPolygonsCollection[setupObject.Type].Add(propPosition);
                }
                else
                {
                    var polygonCollection = new List<PropPointPosition>();
                    polygonCollection.Add(propPosition);
                    context.SetupPolygonsCollection.Add(setupObject.Type, polygonCollection);
                }
            }

            SliceProcessSetupPad(context);
            SliceProcessSetupPad3d(context);
            SliceProcessSetupIntro(context);

            SliceProcessPathWaypoints(context);
            SliceProcessPatrolPaths(context);
        }

        /// <summary>
        /// Helper method called from <see cref="SliceCommon(ProcessedStageData)"/>.
        /// </summary>
        /// <param name="context">Current processing context.</param>
        private void SliceProcessSetupPad(ProcessedStageData context)
        {
            if (object.ReferenceEquals(null, _stage.Setup!.SectionPadList))
            {
                throw new NullReferenceException();
            }

            byte roomId;

            // add presets to the output.
            ushort index = 0;
            foreach (var pad in _stage.Setup!.SectionPadList.PadList)
            {
                roomId = 0;
                if (!TryGetRoomId(pad.Name?.GetString(), out roomId))
                {
                    // continue outer foreach, increment index
                    index++;
                    continue;
                }

                bool withinBounds = false;

                if (object.ReferenceEquals(null, pad.Position))
                {
                    throw new NullReferenceException();
                }

                if (pad.Position.Y >= context.Zmin!.Value && pad.Position.Y <= context.Zmax!.Value)
                {
                    withinBounds = true;
                }

                if (!withinBounds)
                {
                    // continue outer foreach, increment index
                    index++;
                    continue;
                }

                Getools.Lib.Math.Compare.SetUnboundCompareX(context.NativeMin, context.NativeMax, pad.Position.X);
                Getools.Lib.Math.Compare.SetUnboundCompareY(context.NativeMin, context.NativeMax, pad.Position.Y);
                Getools.Lib.Math.Compare.SetUnboundCompareZ(context.NativeMin, context.NativeMax, pad.Position.Z);

                var scaled = pad.Position.ToCoord3dd().Scale(1 / _stage.LevelScale);

                Getools.Lib.Math.Compare.SetUnboundCompareX(context.ScaledMin, context.ScaledMax, scaled.X);
                Getools.Lib.Math.Compare.SetUnboundCompareY(context.ScaledMin, context.ScaledMax, scaled.Y);
                Getools.Lib.Math.Compare.SetUnboundCompareZ(context.ScaledMin, context.ScaledMax, scaled.Z);

                if (object.ReferenceEquals(null, pad.Up))
                {
                    throw new NullReferenceException();
                }

                if (object.ReferenceEquals(null, pad.Look))
                {
                    throw new NullReferenceException();
                }

                context.PresetPolygons.Add(new PointPosition()
                {
                    OrderIndex = index,
                    Room = roomId,
                    Origin = pad.Position.ToCoord3dd(),
                    Up = pad.Up.ToCoord3dd(),
                    Look = pad.Look.ToCoord3dd(),
                });

                // continue outer foreach, increment index
                index++;
            }
        }

        /// <summary>
        /// Helper method called from <see cref="SliceCommon(ProcessedStageData)"/>.
        /// </summary>
        /// <param name="context">Current processing context.</param>
        private void SliceProcessSetupPad3d(ProcessedStageData context)
        {
            if (object.ReferenceEquals(null, _stage.Setup!.SectionPad3dList))
            {
                throw new NullReferenceException();
            }

            byte roomId;
            ushort index = 0;
            foreach (var pad in _stage.Setup!.SectionPad3dList.Pad3dList)
            {
                roomId = 0;
                if (!TryGetRoomId(pad.Name?.GetString(), out roomId))
                {
                    // don't do anything if the room can't be resolved.
                }

                bool withinBounds = false;

                if (object.ReferenceEquals(null, pad.Position))
                {
                    throw new NullReferenceException();
                }

                if (pad.Position.Y >= context.Zmin!.Value && pad.Position.Y <= context.Zmax!.Value)
                {
                    withinBounds = true;
                }

                if (!withinBounds)
                {
                    // continue outer foreach, increment index
                    index++;
                    continue;
                }

                Getools.Lib.Math.Compare.SetUnboundCompareX(context.NativeMin, context.NativeMax, pad.Position.X);
                Getools.Lib.Math.Compare.SetUnboundCompareY(context.NativeMin, context.NativeMax, pad.Position.Y);
                Getools.Lib.Math.Compare.SetUnboundCompareZ(context.NativeMin, context.NativeMax, pad.Position.Z);

                var scaled = pad.Position.ToCoord3dd().Scale(1 / _stage.LevelScale);

                Getools.Lib.Math.Compare.SetUnboundCompareX(context.ScaledMin, context.ScaledMax, scaled.X);
                Getools.Lib.Math.Compare.SetUnboundCompareY(context.ScaledMin, context.ScaledMax, scaled.Y);
                Getools.Lib.Math.Compare.SetUnboundCompareZ(context.ScaledMin, context.ScaledMax, scaled.Z);

                if (object.ReferenceEquals(null, pad.Up))
                {
                    throw new NullReferenceException();
                }

                if (object.ReferenceEquals(null, pad.Look))
                {
                    throw new NullReferenceException();
                }

                if (object.ReferenceEquals(null, pad.BoundingBox))
                {
                    throw new NullReferenceException();
                }

                context.PresetPolygons.Add(new PointPosition()
                {
                    OrderIndex = index,
                    PadId = index + 10000, // back to bound3d id convention
                    Room = roomId,
                    Origin = pad.Position.ToCoord3dd(),
                    Up = pad.Up.ToCoord3dd(),
                    Look = pad.Look.ToCoord3dd(),
                    Bbox = pad.BoundingBox.ToBoundingBoxd(),
                });

                index++;
            }
        }

        /// <summary>
        /// Helper method called from <see cref="SliceCommon(ProcessedStageData)"/>.
        /// </summary>
        /// <param name="context">Current processing context.</param>
        private void SliceProcessSetupIntro(ProcessedStageData context)
        {
            if (object.ReferenceEquals(null, _stage.Setup!.SectionPadList))
            {
                throw new NullReferenceException();
            }

            if (object.ReferenceEquals(null, _stage.Setup.SectionPad3dList))
            {
                throw new NullReferenceException();
            }

            if (object.ReferenceEquals(null, _stage.Setup.SectionIntros))
            {
                throw new NullReferenceException();
            }

            byte roomId;
            ushort index = 0;

            var presets = _stage.Setup!.SectionPadList.PadList;
            var presets3d = _stage.Setup.SectionPad3dList.Pad3dList;

            // look for spawn location
            foreach (var intro in _stage.Setup.SectionIntros.Intros)
            {
                index++;
                roomId = 0;

                if (intro is not IntroSpawn)
                {
                    continue;
                }

                var introSpawn = intro as IntroSpawn;

                var presetId = (ushort)introSpawn!.Unknown_00;
                var preset = presets[presetId];

                if (object.ReferenceEquals(null, preset.Name))
                {
                    throw new NullReferenceException();
                }

                if (object.ReferenceEquals(null, preset.Position))
                {
                    throw new NullReferenceException();
                }

                var presetName = preset.Name.GetString();
                var workingPoint = preset.Position.ToCoord3dd();

                if (!object.ReferenceEquals(null, _stage.Stan))
                {
                    TryGetRoomId(presetName, out roomId);
                }

                bool withinBounds = false;

                if (workingPoint.Y >= context.Zmin!.Value && workingPoint.Y <= context.Zmax!.Value)
                {
                    withinBounds = true;
                }

                if (!withinBounds)
                {
                    // continue outer foreach, increment index
                    index++;
                    continue;
                }

                var hullpoints = new PointPosition()
                {
                    OrderIndex = index,
                    Room = roomId,
                    Up = Coord3dd.Zero.Clone(),
                    Origin = preset.Position.ToCoord3dd(),
                };

                context.IntroPolygons.Add(hullpoints);

                // only the first spawn is used in single player
                break;
            }
        }

        /// <summary>
        /// Helper method called from <see cref="SliceCommon(ProcessedStageData)"/>.
        /// </summary>
        /// <param name="context">Current processing context.</param>
        private void SliceProcessPathWaypoints(ProcessedStageData context)
        {
            if (object.ReferenceEquals(null, _stage.Setup!.SectionPathTables))
            {
                throw new NullReferenceException();
            }

            if (object.ReferenceEquals(null, _stage.Setup.SectionPadList))
            {
                throw new NullReferenceException();
            }

            int room1Id;
            int room2Id;

            var pathTables = _stage.Setup!.SectionPathTables.PathTables;
            var pads = _stage.Setup.SectionPadList.PadList;

            // add presets to the output.
            ushort index = 0;
            foreach (var waypoint in _stage.Setup.SectionPathTables.PathTables)
            {
                byte b = 0;
                room1Id = 0;
                room2Id = 0;

                // list terminates with -1
                if ((int)waypoint.PadId < 0)
                {
                    break;
                }

                var pad = pads[(int)waypoint.PadId];

                if (!TryGetRoomId(pad.Name?.GetString(), out b))
                {
                    // continue outer foreach, increment index
                    index++;
                    continue;
                }

                room1Id = b;

                if (object.ReferenceEquals(null, pad.Position))
                {
                    throw new NullReferenceException();
                }

                bool withinBounds = false;
                if (pad.Position.Y >= context.Zmin!.Value && pad.Position.Y <= context.Zmax!.Value)
                {
                    withinBounds = true;
                }

                if (!withinBounds)
                {
                    // continue outer foreach, increment index
                    index++;
                    continue;
                }

                var pathTable = waypoint.Entry;
                int tableIndex = 0;

                if (object.ReferenceEquals(null, pathTable))
                {
                    throw new NullReferenceException();
                }

                foreach (var id in pathTable.Ids)
                {
                    // list terminates with -1
                    if (id < 0)
                    {
                        break;
                    }

                    var secondWaypoint = pathTables[id];
                    var pad2 = pads[(int)secondWaypoint.PadId];

                    if (!TryGetRoomId(pad2.Name?.GetString(), out b))
                    {
                        // continue inner foreach
                        tableIndex++;
                        continue;
                    }

                    room2Id = b;
                    withinBounds = false;

                    if (object.ReferenceEquals(null, pad2.Position))
                    {
                        throw new NullReferenceException();
                    }

                    if (pad2.Position.Y >= context.Zmin!.Value && pad2.Position.Y <= context.Zmax!.Value)
                    {
                        withinBounds = true;
                    }

                    if (!withinBounds)
                    {
                        // continue inner foreach
                        tableIndex++;
                        continue;
                    }

                    //// pad global min/max comparison already happened in the pad section.

                    var rl = new RenderLine(pad.Position.ToCoord3dd(), pad2.Position.ToCoord3dd())
                    {
                        WaypointIndex = index,
                        TableIndex = tableIndex,
                        Pad1RoomId = room1Id,
                        Pad2RoomId = room2Id,
                        Pad1Id = (int)waypoint.PadId,
                        Pad2Id = (int)secondWaypoint.PadId,
                    };

                    context.PathWaypointLines.Add(rl);

                    tableIndex++;
                }

                // continue outer foreach, increment index
                index++;
            }
        }

        /// <summary>
        /// Helper method called from <see cref="SliceCommon(ProcessedStageData)"/>.
        /// </summary>
        /// <param name="context">Current processing context.</param>
        private void SliceProcessPatrolPaths(ProcessedStageData context)
        {
            if (object.ReferenceEquals(null, _stage.Setup!.SectionPathTables))
            {
                throw new NullReferenceException();
            }

            if (object.ReferenceEquals(null, _stage.Setup.SectionPadList))
            {
                throw new NullReferenceException();
            }

            if (object.ReferenceEquals(null, _stage.Setup.SectionPathSets))
            {
                throw new NullReferenceException();
            }

            var waypoints = _stage.Setup!.SectionPathTables.PathTables;
            var pads = _stage.Setup.SectionPadList.PadList;

            ushort index = 0;
            foreach (var patrolpath in _stage.Setup.SectionPathSets.PathSets)
            {
                if (object.ReferenceEquals(null, patrolpath.EntryPointer))
                {
                    throw new NullReferenceException();
                }

                // list terminates with null pointer to pathset.
                if (patrolpath.EntryPointer.IsNull || patrolpath.Entry == null)
                {
                    break;
                }

                var polyline = new RenderPolyline()
                {
                    OrderIndex = index,
                };

                foreach (var pathWaypointId in patrolpath.Entry.Ids)
                {
                    // list terminates with value of -1
                    if (pathWaypointId < 0)
                    {
                        break;
                    }

                    var waypoint = waypoints[pathWaypointId];
                    var pad = pads[(int)waypoint.PadId];

                    bool withinBounds = false;

                    if (object.ReferenceEquals(null, pad.Position))
                    {
                        throw new NullReferenceException();
                    }

                    if (pad.Position.Y >= context.Zmin!.Value && pad.Position.Y <= context.Zmax!.Value)
                    {
                        withinBounds = true;
                    }

                    if (!withinBounds)
                    {
                        // continue outer foreach
                        continue;
                    }

                    var pos = pad.Position.ToCoord3dd();
                    polyline.Bbox.IncludePoint(pos);
                    polyline.Points.Add(pos);
                }

                if (polyline.Points.Any())
                {
                    context.PatrolPathLines.Add(polyline);
                }

                // continue outer foreach, increment index
                index++;
            }
        }

        /// <summary>
        /// Helper method called from <see cref="SliceCommon(ProcessedStageData)"/>.
        /// </summary>
        /// <param name="context">Current processing context.</param>
        private void SetupProcessAi(ProcessedStageData context)
        {
            if (object.ReferenceEquals(null, _stage.Setup!.SectionAiLists))
            {
                throw new NullReferenceException();
            }

            if (!_stage.Setup!.SectionAiLists.AiLists.Any())
            {
                return;
            }

            foreach (var ailisty in _stage.Setup.SectionAiLists.AiLists.Where(x => x.EntryPointer != null))
            {
                if (ailisty.EntryPointer!.IsNull || ailisty.Function == null)
                {
                    // end of section
                    break;
                }

                var fff = ailisty.Function.GetParsedAiBlock();
                fff.Id = (int)ailisty.Id;

                if (ailisty.Id < 0x401)
                {
                    fff.ScriptType = AiScriptType.Global;
                }
                //// starts at 0x401
                else if (ailisty.Id > 0x400 && ailisty.Id < 0x1000)
                {
                    fff.ScriptType = AiScriptType.Entity;
                }
                else
                {
                    fff.ScriptType = AiScriptType.Background;
                }

                foreach (var command in fff.Commands)
                {
                    if (command is IAiFixedCommand fcommand)
                    {
                        foreach (var p in fcommand.CommandParameters)
                        {
                            int pvalue = p.GetIntValue(Lib.Architecture.ByteOrder.LittleEndien);

                            if (p.ParameterName == "chr_num")
                            {
                                // chr_num has some reserved values, ignore those:

                                /***
                                 * define CHR_BOND_CINEMA -8
                                 * define CHR_CLONE       -7
                                 * define CHR_SEE_SHOT    -6
                                 * define CHR_SEE_DIE     -5
                                 * define CHR_PRESET      -4
                                 * define CHR_SELF        -3
                                */
                                if (pvalue > 0xf0 || pvalue < 0)
                                {
                                    continue;
                                }

                                if (context.ChrIdToAiCommandBlock.ContainsKey(pvalue))
                                {
                                    context.ChrIdToAiCommandBlock[pvalue].Add(fff.Id);
                                }
                                else
                                {
                                    var list = new HashSet<int>()
                                    {
                                        fff.Id,
                                    };
                                    context.ChrIdToAiCommandBlock.Add(pvalue, list);
                                }

                                if (context.AiCommandBlockToChrId.ContainsKey(fff.Id))
                                {
                                    context.AiCommandBlockToChrId[fff.Id].Add(pvalue);
                                }
                                else
                                {
                                    var list = new HashSet<int>()
                                    {
                                        pvalue,
                                    };
                                    context.AiCommandBlockToChrId.Add(fff.Id, list);
                                }
                            }
                            else if (p.ParameterName == "pad" || p.ParameterName == "chr_preset" || p.ParameterName == "pad_preset")
                            {
                                // 9000 is Special ID for selecting PadPreset in AI list.
                                // stored as chr->padpreset1
                                if (pvalue == 9000)
                                {
                                    continue;
                                }

                                if (context.PadIdToAiCommandBlock.ContainsKey(pvalue))
                                {
                                    context.PadIdToAiCommandBlock[pvalue].Add(fff.Id);
                                }
                                else
                                {
                                    var list = new HashSet<int>()
                                    {
                                        fff.Id,
                                    };
                                    context.PadIdToAiCommandBlock.Add(pvalue, list);
                                }

                                if (context.AiCommandBlockToPadId.ContainsKey(fff.Id))
                                {
                                    context.AiCommandBlockToPadId[fff.Id].Add(pvalue);
                                }
                                else
                                {
                                    var list = new HashSet<int>()
                                    {
                                        pvalue,
                                    };
                                    context.AiCommandBlockToPadId.Add(fff.Id, list);
                                }
                            }
                            else if (p.ParameterName == "path_num")
                            {
                                if (context.PathIdToAiCommandBlock.ContainsKey(pvalue))
                                {
                                    context.PathIdToAiCommandBlock[pvalue].Add(fff.Id);
                                }
                                else
                                {
                                    var list = new HashSet<int>()
                                    {
                                        fff.Id,
                                    };
                                    context.PathIdToAiCommandBlock.Add(pvalue, list);
                                }

                                if (context.AiCommandBlockToPathId.ContainsKey(fff.Id))
                                {
                                    context.AiCommandBlockToPathId[fff.Id].Add(pvalue);
                                }
                                else
                                {
                                    var list = new HashSet<int>()
                                    {
                                        pvalue,
                                    };
                                    context.AiCommandBlockToPathId.Add(fff.Id, list);
                                }
                            }
                            else if (p.ParameterName == "ai_list")
                            {
                                context.RefAiListId.Add(pvalue);
                            }
                        }
                    }
                }

                context.AiScripts.Add(fff);
            }
        }

        /// <summary>
        /// Cross apply all room points, and for each finite line segment that cross the plane
        /// at the given z point, project to 2d and add it to the list.
        /// Create a convex hull from these points.
        /// </summary>
        /// <remarks>
        /// "walking" coordinates are (x,z), vertical axis is y.
        /// </remarks>
        /// <param name="mesh">Starting list of 3d points describing a mesh.</param>
        /// <param name="z">Z value to slice mesh at (check for any lines that cross this value).</param>
        /// <returns>Convex hull of points from any lines that cross the parameter value.</returns>
        private List<Coord2dd> MeshToConvexHullAtZ(List<Coord3dd> mesh, double z)
        {
            var points = Getools.Lib.Math.Geometry.PlaneIntersectZ(mesh, z);
            var points2d = points.Select(x => x.To2DXY()).ToList();
            var convexHull = Getools.Lib.Math.Geometry.GetConvexHull(points2d);

            return convexHull;
        }

        /// <summary>
        /// Cross apply all room points, and for each finite line segment that cross the plane
        /// at the given y point, project to 2d and add it to the list.
        /// Create a convex hull from these points.
        /// </summary>
        /// <remarks>
        /// "walking" coordinates are (x,z), vertical axis is y.
        /// </remarks>
        /// <param name="mesh">Starting list of 3d points describing a mesh.</param>
        /// <param name="y">Y value to slice mesh at (check for any lines that cross this value).</param>
        /// <returns>Convex hull of points from any lines that cross the parameter value.</returns>
        private List<Coord2dd> MeshToConvexHullAtY(List<Coord3dd> mesh, double y)
        {
            var points = Getools.Lib.Math.Geometry.PlaneIntersectY(mesh, y);
            var points2d = points.Select(x => x.To2DXZ()).ToList();
            var convexHull = Getools.Lib.Math.Geometry.GetConvexHull(points2d);

            return convexHull;
        }

        /// <summary>
        /// For each point, if it is between the upper and lower bounds,
        /// project to 2d and create a convex hull of these points.
        /// </summary>
        /// <param name="mesh">Starting list of 3d points describing a mesh.</param>
        /// <param name="ymin">Min Y value to allow points.</param>
        /// <param name="ymax">Max Y value to allow points.</param>
        /// <returns>Convex hull of points from any lines that are between the bounds.</returns>
        private List<Coord2dd> MeshToConvexHullYBounds(List<Coord3dd> mesh, double ymin, double ymax)
        {
            var points2d = new List<Coord2dd>();

            foreach (var p in mesh)
            {
                if (p.Y >= ymin && p.Y <= ymax)
                {
                    points2d.Add(p.To2DXZ());
                }
            }

            var convexHull = Getools.Lib.Math.Geometry.GetConvexHull(points2d);

            return convexHull;
        }

        /// <summary>
        /// Helper function to convert string pad name, to find matching stan tile, to find room id.
        /// </summary>
        /// <param name="presetName">String from ROM.</param>
        /// <param name="roomId">Room id result or zero.</param>
        /// <returns>True if able to decode to room id, false otherwise.</returns>
        private bool TryGetRoomId(string? presetName, out byte roomId)
        {
            roomId = 0;

            if (string.IsNullOrEmpty(presetName))
            {
                return false;
            }

            if (object.ReferenceEquals(null, _stage.Stan))
            {
                return false;
            }

            var rematch = _matchDigitsRegex.Match(presetName);
            if (rematch.Success)
            {
                var tilenameDecimalString = rematch.Groups[1].Value;
                var tilenameDecimal = int.Parse(tilenameDecimalString);
                bool useTileGroup = rematch.Groups.Count > 3 && !string.IsNullOrEmpty(rematch.Groups[2].Value);

                int tileGroupId = 0;

                StandTile? tile = null;
                if (useTileGroup)
                {
                    // This is how to convert from encoded format to int id.
                    tileGroupId = 8 * Getools.Lib.Math.Convert.Base26ToInt(rematch.Groups[2].Value[0]);
                    if (!string.IsNullOrEmpty(rematch.Groups[3].Value))
                    {
                        tileGroupId += int.Parse(rematch.Groups[3].Value);
                    }

                    tile = _stage.Stan.Tiles.FirstOrDefault(x => x.TileId == tilenameDecimal && x.GroupId == tileGroupId);
                }
                else
                {
                    tile = _stage.Stan.Tiles.FirstOrDefault(x => x.TileId == tilenameDecimal);
                }

                if (!object.ReferenceEquals(null, tile))
                {
                    roomId = tile.Room;
                    return true;
                }
            }

            return false;
        }
    }
}
