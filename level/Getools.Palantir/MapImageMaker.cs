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

namespace Getools.Palantir
{
    public class MapImageMaker
    {
        // three decimal places
        private const string StandardDoubleToStringFormat = "0.###";

        private const double MaxPixelSizeError = 10000000;

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

        private static Regex _matchDigitsRegex = new Regex("([0-9]+)([a-zA-Z])([0-9]?)");

        public Stage Stage { get; set; }

        public double OffsetXAfterScale { get; set; } = 0.0;

        public double OffsetYAfterScale { get; set; } = 0.0;

        public MapImageMaker()
        {
        }

        public SvgDocument BoundingZToImage(double zmin, double zmax)
        {
            return SliceCommon(SliceMode.BoundingBox, null, zmin, zmax);
        }

        public SvgDocument SliceZToImage(double z)
        {
            return SliceCommon(SliceMode.Slice, z, null, null);
        }

        public SvgDocument FullImage()
        {
            return SliceCommon(SliceMode.Unbound, null, double.MinValue, double.MaxValue);
        }

        private enum SliceMode
        {
            Slice,
            BoundingBox,
            Unbound,
        }

        private SvgDocument SliceCommon(SliceMode mode, double? z, double? zmin, double? zmax)
        {
            var roomPolygons = new List<CollectionHullSvgPoints>();
            var tilePolygons = new List<CollectionHullSvgPoints>();
            var presetPolygons = new List<RenderPosition>();
            var introPolygons = new List<RenderPosition>();

            var setupPolygonsCollection = new Dictionary<PropDef, List<PropPosition>>();

            Coord3dd scaledMin = Coord3dd.MaxValue.Clone();
            Coord3dd scaledMax = Coord3dd.MinValue.Clone();
            Coord3dd nativeMin = Coord3dd.MaxValue.Clone();
            Coord3dd nativeMax = Coord3dd.MinValue.Clone();

            if (!object.ReferenceEquals(null, Stage.Bg))
            {
                Console.WriteLine("MapImageMaker: Found bg");

                foreach (var roomData in Stage.Bg.RoomDataTable.Entries)
                {
                    if (object.ReferenceEquals(null, roomData.Points) || !roomData.Points.Any())
                    {
                        // Console.WriteLine($"skipping empty room # {roomData.OrderIndex}");
                        continue;
                    }

                    // Console.WriteLine($"process room # {roomData.OrderIndex}");

                    var roomScaledMin = Coord3dd.MaxValue.Clone();
                    var roomScaledMax = Coord3dd.MinValue.Clone();
                    var roomNativeMin = Coord3dd.MaxValue.Clone();
                    var roomNativeMax = Coord3dd.MinValue.Clone();

                    var center = roomData.Coord;
                    var roomPoints = new List<Coord3dd>();

                    foreach (var vtx in roomData.Points)
                    {
                        var roomPoint = new Coord3dd()
                        {
                            X = (double)center.X + (double)vtx.Ob.X,
                            Y = (double)center.Y + (double)vtx.Ob.Y,
                            Z = (double)center.Z + (double)vtx.Ob.Z,
                        };

                        if (mode == SliceMode.BoundingBox)
                        {
                            if (!(roomPoint.Y >= zmin!.Value))
                            {
                                continue;
                            }

                            if (!(roomPoint.Y <= zmax!.Value))
                            {
                                continue;
                            }

                            Getools.Lib.Math.Compare.SetMinMaxCompareY(roomNativeMin, roomNativeMax, roomPoint.Y, zmin!.Value, zmax!.Value);
                            Getools.Lib.Math.Compare.SetMinMaxCompareY(nativeMin, nativeMax, roomPoint.Y, zmin!.Value, zmax!.Value);
                        }
                        else if (mode == SliceMode.Unbound)
                        {
                            Getools.Lib.Math.Compare.SetUnboundCompareY(roomNativeMin, roomNativeMax, roomPoint.Y);
                            Getools.Lib.Math.Compare.SetUnboundCompareY(nativeMin, nativeMax, roomPoint.Y);
                        }

                        Getools.Lib.Math.Compare.SetUnboundCompareX(roomNativeMin, roomNativeMax, roomPoint.X);
                        Getools.Lib.Math.Compare.SetUnboundCompareX(nativeMin, nativeMax, roomPoint.X);

                        Getools.Lib.Math.Compare.SetUnboundCompareZ(roomNativeMin, roomNativeMax, roomPoint.Z);
                        Getools.Lib.Math.Compare.SetUnboundCompareZ(nativeMin, nativeMax, roomPoint.Z);

                        var scaled = roomPoint.Scale(1.0 / Stage.LevelScale);

                        Getools.Lib.Math.Compare.SetUnboundCompareX(roomScaledMin, roomScaledMax, scaled.X);
                        Getools.Lib.Math.Compare.SetUnboundCompareX(scaledMin, scaledMax, scaled.X);
                        Getools.Lib.Math.Compare.SetUnboundCompareY(roomScaledMin, roomScaledMax, scaled.Y);
                        Getools.Lib.Math.Compare.SetUnboundCompareY(scaledMin, scaledMax, scaled.Y);
                        Getools.Lib.Math.Compare.SetUnboundCompareZ(roomScaledMin, roomScaledMax, scaled.Z);
                        Getools.Lib.Math.Compare.SetUnboundCompareZ(scaledMin, scaledMax, scaled.Z);

                        roomPoints.Add(roomPoint);
                    }

                    // Console.WriteLine($"found {roomPoints.Count} points");

                    List<Coord2dd> convexHull = null;

                    if (mode == SliceMode.Slice)
                    {
                        convexHull = MeshToConvexHullAtY(roomPoints, z!.Value);
                    }
                    else if (mode == SliceMode.BoundingBox)
                    {
                        convexHull = MeshToConvexHullYBounds(roomPoints, zmin!.Value, zmax!.Value);
                    }
                    else
                    {
                        convexHull = MeshToConvexHullYBounds(roomPoints, double.MinValue, double.MaxValue);
                    }

                    // Console.WriteLine($"convexHull result: {convexHull.Count} points");

                    if (!convexHull.Any())
                    {
                        continue;
                    }

                    var svgScaledPoints = new List<Coord2dd>();

                    foreach (var p in convexHull)
                    {
                        var scaled = new Coord2dd(p.X, p.Y).Scale(1.0 / Stage.LevelScale);

                        svgScaledPoints.Add(scaled);
                    }

                    // Add first point at end of list to create closed svg path.
                    // Points are adjusted later, be careful not to link this
                    // to an existing point.
                    svgScaledPoints.Add(svgScaledPoints[0].Clone()); // created from scaled points

                    var collection = new CollectionHullSvgPoints()
                    {
                        OrderIndex = roomData.OrderIndex,
                        Room = roomData.OrderIndex,
                        Points = svgScaledPoints,
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
                        collection.ScaledMin.Y = roomNativeMin.Y * (1 / Stage.LevelScale);
                    }

                    if (roomNativeMax.Y > double.MinValue)
                    {
                        collection.NaturalMax.Y = roomNativeMax.Y;
                        collection.ScaledMax.Y = roomNativeMax.Y * (1 / Stage.LevelScale);
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

                    roomPolygons.Add(collection);

                    // Console.WriteLine($"room min/max: {roomMinScaledX},{roomMinScaledY} and {roomMaxScaledX},{roomMaxScaledY}");
                }
            }

            if (!object.ReferenceEquals(null, Stage.Stan))
            {
                Console.WriteLine("MapImageMaker: Found stan");

                foreach (var tile in Stage.Stan.Tiles)
                {
                    if (object.ReferenceEquals(null, tile.Points) || !tile.Points.Any())
                    {
                        // Console.WriteLine($"skipping empty tile # {tile.OrderIndex}");
                        continue;
                    }

                    //Console.WriteLine($"process tile # {tile.OrderIndex}");

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

                        if (mode == SliceMode.BoundingBox)
                        {
                            if (!(roomPoint.Y >= zmin!.Value))
                            {
                                continue;
                            }

                            if (!(roomPoint.Y <= zmax!.Value))
                            {
                                continue;
                            }

                            Getools.Lib.Math.Compare.SetMinMaxCompareY(tileNativeMin, tileNativeMax, roomPoint.Y, zmin!.Value, zmax!.Value);
                            Getools.Lib.Math.Compare.SetMinMaxCompareY(nativeMin, nativeMax, roomPoint.Y, zmin!.Value, zmax!.Value);
                        }
                        else if (mode == SliceMode.Unbound)
                        {
                            Getools.Lib.Math.Compare.SetUnboundCompareY(tileNativeMin, tileNativeMax, roomPoint.Y);
                            Getools.Lib.Math.Compare.SetUnboundCompareY(nativeMin, nativeMax, roomPoint.Y);
                        }

                        Getools.Lib.Math.Compare.SetUnboundCompareX(tileNativeMin, tileNativeMax, roomPoint.X);
                        Getools.Lib.Math.Compare.SetUnboundCompareX(nativeMin, nativeMax, roomPoint.X);

                        Getools.Lib.Math.Compare.SetUnboundCompareZ(tileNativeMin, tileNativeMax, roomPoint.Z);
                        Getools.Lib.Math.Compare.SetUnboundCompareZ(nativeMin, nativeMax, roomPoint.Z);

                        var scaled = roomPoint.Scale(1.0 / Stage.LevelScale);

                        Getools.Lib.Math.Compare.SetUnboundCompareX(tileScaledMin, tileScaledMax, scaled.X);
                        Getools.Lib.Math.Compare.SetUnboundCompareX(scaledMin, scaledMax, scaled.X);
                        Getools.Lib.Math.Compare.SetUnboundCompareY(tileScaledMin, tileScaledMax, scaled.Y);
                        Getools.Lib.Math.Compare.SetUnboundCompareY(scaledMin, scaledMax, scaled.Y);
                        Getools.Lib.Math.Compare.SetUnboundCompareZ(tileScaledMin, tileScaledMax, scaled.Z);
                        Getools.Lib.Math.Compare.SetUnboundCompareZ(scaledMin, scaledMax, scaled.Z);

                        levelTilePoints.Add(roomPoint);
                    }

                    //Console.WriteLine($"found {levelTilePoints.Count} points");

                    List<Coord2dd> convexHull = null;

                    if (mode == SliceMode.Slice)
                    {
                        convexHull = MeshToConvexHullAtY(levelTilePoints, z!.Value);
                    }
                    else if (mode == SliceMode.BoundingBox)
                    {
                        convexHull = MeshToConvexHullYBounds(levelTilePoints, zmin!.Value, zmax!.Value);
                    }
                    else
                    {
                        convexHull = MeshToConvexHullYBounds(levelTilePoints, double.MinValue, double.MaxValue);
                    }

                    //Console.WriteLine($"convexHull result: {convexHull.Count} points");

                    if (!convexHull.Any())
                    {
                        continue;
                    }

                    var svgScaledPoints = new List<Coord2dd>();

                    foreach (var p in convexHull)
                    {
                        var scaled = new Coord2dd(p.X, p.Y).Scale(1.0 / Stage.LevelScale);

                        svgScaledPoints.Add(scaled);
                    }

                    // Add first point at end of list to create closed svg path.
                    // Points are adjusted later, be careful not to link this
                    // to an existing point.
                    svgScaledPoints.Add(svgScaledPoints[0].Clone()); // created from scaled points

                    var collection = new CollectionHullSvgPoints()
                    {
                        OrderIndex = tile.OrderIndex,
                        Room = tile.Room,
                        Points = svgScaledPoints,
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
                        collection.ScaledMin.Y = tileNativeMin.Y * (1 / Stage.LevelScale);
                    }

                    if (tileNativeMax.Y > double.MinValue)
                    {
                        collection.NaturalMax.Y = tileNativeMax.Y;
                        collection.ScaledMax.Y = tileNativeMax.Y * (1 / Stage.LevelScale);
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

                    tilePolygons.Add(collection);

                    //Console.WriteLine($"tile min/max: {tileMinScaledX},{tileMinScaledY} and {tileMaxScaledX},{tileMaxScaledY}");
                }
            }

            if (!object.ReferenceEquals(null, Stage.Setup) && (mode == SliceMode.BoundingBox || mode == SliceMode.Unbound))
            {
                Console.WriteLine("MapImageMaker: Found setup");

                var presets = Stage.Setup.SectionPadList.PadList;
                var presets3d = Stage.Setup.SectionPad3dList.Pad3dList;

                int setupObjectIndex = -1;
                byte roomId = 0;

                foreach (var setupObject in Stage.Setup.SectionObjects.Objects)
                {
                    setupObjectIndex++;

                    //Coord3dd? workingPoint = null;
                    //Coord3dd? up = null;
                    //Coord3dd? orientation = null;
                    string presetName = string.Empty;
                    Pad pad = null;

                    var baseObject = setupObject as SetupObjectBase;

                    // guard does not implement SetupObjectGenericBase
                    SetupObjectGenericBase baseGenericObject = setupObject as SetupObjectGenericBase;

                    if (object.ReferenceEquals(null, baseObject))
                    {
                        continue;
                    }

                    if (baseObject is not IHasPreset)
                    {
                        continue;
                    }

                    var presetId = ((IHasPreset)setupObject).Preset;

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
                        case Lib.Game.Enums.PropDef.Alarm:   // fallthrough
                        case Lib.Game.Enums.PropDef.Aircraft:   // fallthrough
                        case Lib.Game.Enums.PropDef.AmmoBox: // fallthrough
                        case Lib.Game.Enums.PropDef.Armour: // fallthrough
                        case Lib.Game.Enums.PropDef.Cctv: // fallthrough
                        case Lib.Game.Enums.PropDef.Collectable: // fallthrough
                        case Lib.Game.Enums.PropDef.Drone: // fallthrough
                        case Lib.Game.Enums.PropDef.Guard:   // fallthrough
                        case Lib.Game.Enums.PropDef.Key:   // fallthrough
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
                            }
                            else if ((presetId >= 10000) && (presetId - 10000) < presets3d.Count)
                            {
                                pad = presets3d[presetId - 10000];
                            }
                            else
                            {
                                throw new Exception("Could not resolve setup object point to preset (padlist) index");
                            }
                            break;
                        }

                        case Lib.Game.Enums.PropDef.Door:
                        {
                            // important! door always uses bounding box preset

                            if (presetId < presets3d.Count)
                            {
                                pad = presets3d[presetId];
                            }
                            else if ((presetId >= 10000) && (presetId - 10000) < presets3d.Count)
                            {
                                pad = presets3d[presetId - 10000];
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

                    // Points are scaled later, depending on type
                    var propPosition = new PropPosition()
                    {
                        OrderIndex = setupObjectIndex,
                        SetupObject = setupObject,
                        Origin = pad.Position.ToCoord3dd(),
                        Up = pad.Up.ToCoord3dd(),
                        Look = pad.Look.ToCoord3dd(),
                        Type = setupObject.Type,
                        Prop = PropId.PROP_MAX,
                    };

                    if (!object.ReferenceEquals(null, baseGenericObject))
                    {
                        propPosition.Prop = (PropId)baseGenericObject.ObjectId;
                    }

                    if (pad is Pad3d)
                    {
                        var pad3d = (Pad3d) pad;
                        propPosition.Bbox = pad3d.BoundingBox.ToBoundingBoxd();
                    }

                    presetName = pad.Name.GetString();

                    // Console.WriteLine($"Setup object point: {workingPoint}");

                    if (!object.ReferenceEquals(null, Stage.Stan))
                    {
                        TryGetRoomId(presetName, out roomId);
                        propPosition.Room = roomId;
                    }

                    bool withinBounds = false;

                    if (propPosition.Origin.Y >= zmin!.Value && propPosition.Origin.Y <= zmax!.Value)
                    {
                        withinBounds = true;
                    }

                    if (!withinBounds)
                    {
                        continue;
                    }

                    if (setupPolygonsCollection.ContainsKey(setupObject.Type))
                    {
                        setupPolygonsCollection[setupObject.Type].Add(propPosition);
                    }
                    else
                    {
                        var polygonCollection = new List<PropPosition>();
                        polygonCollection.Add(propPosition);
                        setupPolygonsCollection.Add(setupObject.Type, polygonCollection);
                    }
                }

                // add presets to the output.
                ushort index = 0;
                foreach (var pad in Stage.Setup.SectionPadList.PadList)
                {
                    roomId = 0;
                    if (!TryGetRoomId(pad.Name.GetString(), out roomId))
                    {
                        index++;
                        continue;
                    }

                    bool withinBounds = false;

                    if (pad.Position.Y >= zmin!.Value && pad.Position.Y <= zmax!.Value)
                    {
                        withinBounds = true;
                    }

                    if (!withinBounds)
                    {
                        continue;
                    }

                    Getools.Lib.Math.Compare.SetUnboundCompareX(nativeMin, nativeMax, pad.Position.X);
                    Getools.Lib.Math.Compare.SetUnboundCompareY(nativeMin, nativeMax, pad.Position.Y);
                    Getools.Lib.Math.Compare.SetUnboundCompareZ(nativeMin, nativeMax, pad.Position.Z);

                    var scaled = pad.Position.ToCoord3dd().Scale(1 / Stage.LevelScale);

                    Getools.Lib.Math.Compare.SetUnboundCompareX(scaledMin, scaledMax, scaled.X);
                    Getools.Lib.Math.Compare.SetUnboundCompareY(scaledMin, scaledMax, scaled.Y);
                    Getools.Lib.Math.Compare.SetUnboundCompareZ(scaledMin, scaledMax, scaled.Z);

                    presetPolygons.Add(new RenderPosition()
                    {
                        OrderIndex = index,
                        Room = roomId,
                        Origin = pad.Position.ToCoord3dd(),
                    });

                    index++;
                }

                index = 0;
                foreach (var pad in Stage.Setup.SectionPad3dList.Pad3dList)
                {
                    roomId = 0;
                    if (!TryGetRoomId(pad.Name.GetString(), out roomId))
                    {
                        //index++;
                        //continue;
                    }

                    bool withinBounds = false;

                    if (pad.Position.Y >= zmin!.Value && pad.Position.Y <= zmax!.Value)
                    {
                        withinBounds = true;
                    }

                    if (!withinBounds)
                    {
                        continue;
                    }

                    Getools.Lib.Math.Compare.SetUnboundCompareX(nativeMin, nativeMax, pad.Position.X);
                    Getools.Lib.Math.Compare.SetUnboundCompareY(nativeMin, nativeMax, pad.Position.Y);
                    Getools.Lib.Math.Compare.SetUnboundCompareZ(nativeMin, nativeMax, pad.Position.Z);

                    var scaled = pad.Position.ToCoord3dd().Scale(1 / Stage.LevelScale);

                    Getools.Lib.Math.Compare.SetUnboundCompareX(scaledMin, scaledMax, scaled.X);
                    Getools.Lib.Math.Compare.SetUnboundCompareY(scaledMin, scaledMax, scaled.Y);
                    Getools.Lib.Math.Compare.SetUnboundCompareZ(scaledMin, scaledMax, scaled.Z);

                    presetPolygons.Add(new RenderPosition()
                    {
                        OrderIndex = index + 10000, // back to bound3d id convention
                        Room = roomId,
                        Origin = pad.Position.ToCoord3dd(),
                    });

                    index++;
                }

                // look for spawn location
                foreach (var intro in Stage.Setup.SectionIntros.Intros)
                {
                    if (intro is not IntroSpawn)
                    {
                        continue;
                    }

                    var introSpawn = intro as IntroSpawn;

                    var presetId = (ushort)introSpawn.Unknown_00;

                    var preset = presets[presetId];
                    var presetName = preset.Name.GetString();
                    var workingPoint = preset.Position.ToCoord3dd();

                    if (!object.ReferenceEquals(null, Stage.Stan))
                    {
                        TryGetRoomId(presetName, out roomId);
                    }

                    bool withinBounds = false;

                    if (workingPoint.Y >= zmin!.Value && workingPoint.Y <= zmax!.Value)
                    {
                        withinBounds = true;
                    }

                    if (!withinBounds)
                    {
                        continue;
                    }

                    var hullpoints = new RenderPosition()
                    {
                        OrderIndex = setupObjectIndex,
                        Room = roomId,
                        Up = Coord3dd.Zero.Clone(),
                        Origin = preset.Position.ToCoord3dd(),
                    };

                    introPolygons.Add(hullpoints);

                    // only the first spawn is used in single player
                    break;
                }
            }

            var svg = SvgDocument.Create();

            scaledMax.X = (scaledMax.X == double.MinValue) ? 0 : scaledMax.X;
            scaledMax.Y = (scaledMax.Y == double.MinValue) ? 0 : scaledMax.Y;
            scaledMax.Z = (scaledMax.Z == double.MinValue) ? 0 : scaledMax.Z;

            scaledMin.X = (scaledMin.X == double.MaxValue) ? 0 : scaledMin.X;
            scaledMin.Y = (scaledMin.Y == double.MaxValue) ? 0 : scaledMin.Y;
            scaledMin.Z = (scaledMin.Z == double.MaxValue) ? 0 : scaledMin.Z;

            nativeMax.X = (nativeMax.X == double.MinValue) ? 0 : nativeMax.X;
            nativeMax.Y = (nativeMax.Y == double.MinValue) ? 0 : nativeMax.Y;
            nativeMax.Z = (nativeMax.Z == double.MinValue) ? 0 : nativeMax.Z;

            nativeMin.X = (nativeMin.X == double.MaxValue) ? 0 : nativeMin.X;
            nativeMin.Y = (nativeMin.Y == double.MaxValue) ? 0 : nativeMin.Y;
            nativeMin.Z = (nativeMin.Z == double.MaxValue) ? 0 : nativeMin.Z;

            var svgWidth = Math.Abs(scaledMax.X - scaledMin.X);
            var svgHeight = Math.Abs(scaledMax.Z - scaledMin.Z);

            if (!double.IsFinite(svgWidth) || svgWidth > MaxPixelSizeError)
            {
                throw new Exception("Invalid image width");
            }

            if (!double.IsFinite(svgHeight) || svgHeight > MaxPixelSizeError || svgHeight == 0)
            {
                throw new Exception("Invalid image height");
            }

            // normalize to positive values
            var adjustx = 0 - scaledMin.X;
            var adjusty = 0 - scaledMin.Z;

            double naturalRatioWh = svgWidth / svgHeight;
            double naturalRatioHw = svgHeight / svgWidth;

            svg.Width = 2048;
            svg.Height = svg.Width * naturalRatioHw;

            svg.ViewBox = new SvgViewBox()
            {
                Width = svgWidth,
                Height = svgHeight,
                Left = scaledMin.X,
                Top = scaledMin.Z,
            };

            svg.SetDataAttribute("adjustx", adjustx.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("adjusty", adjusty.ToString(StandardDoubleToStringFormat));

            svg.SetDataAttribute("level-scale", Stage.LevelScale.ToString(StandardDoubleToStringFormat));

            svg.SetDataAttribute("n-min-x", nativeMin.X.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("n-max-x", nativeMax.X.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("n-min-y", nativeMin.Y.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("n-max-y", nativeMax.Y.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("n-min-z", nativeMin.Z.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("n-max-z", nativeMax.Z.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("s-min-x", scaledMin.X.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("s-max-x", scaledMax.X.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("s-min-y", scaledMin.Y.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("s-max-y", scaledMax.Y.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("s-min-z", scaledMin.Z.ToString(StandardDoubleToStringFormat));
            svg.SetDataAttribute("s-max-z", scaledMax.Z.ToString(StandardDoubleToStringFormat));

            var cssText = new StringBuilder();
            cssText.AppendLine(".gelib-stan { stroke: #9e87a3; stroke-width: 2; fill: #fdf5ff; }");
            cssText.AppendLine(".gelib-room { stroke: blue; stroke-width: 4; fill: #ffffff; fill-opacity: 0; }");
            cssText.AppendLine(".gelib-pad { stroke: #9c009e; stroke-width: 4; fill: #ff00ff; }");

            svg.SetStylesheet(cssText.ToString());

            var group1 = svg.AddGroup();
            group1.Id = SvgStanLayerId;
            foreach (var poly in tilePolygons)
            {
                var polyline = group1.AddPolyLine();

                polyline.AddClass("svg-logical-item");
                polyline.AddClass("gelib-stan");

                polyline.Id = string.Format(SvgItemIdTileFormat, poly.Room, poly.OrderIndex);
                polyline.SetPoints(poly.Points.To1dArray(), StandardDoubleToStringFormat);

                foreach (var kvp in poly.SvgDataAttributes)
                {
                    polyline.SetDataAttribute(kvp.Key, kvp.Value);
                }

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

            // room boundaries need to be above the tiles.
            var group2 = svg.AddGroup();
            group2.Id = SvgBgLayerId;
            foreach (var poly in roomPolygons)
            {
                var polyline = group2.AddPolyLine();

                polyline.AddClass("svg-logical-item");
                polyline.AddClass("gelib-room");

                polyline.Id = string.Format(SvgItemIdRoomFormat, poly.OrderIndex);
                polyline.SetPoints(poly.Points.To1dArray(), StandardDoubleToStringFormat);

                foreach (var kvp in poly.SvgDataAttributes)
                {
                    polyline.SetDataAttribute(kvp.Key, kvp.Value);
                }

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

            if (presetPolygons.Any())
            {
                var group = svg.AddGroup();
                group.Id = SvgPadLayerId;
                foreach (var poly in presetPolygons)
                {
                    var svgcontainer = SvgAppend.PadToSvg.PadToSvgAppend(group, poly, Stage.LevelScale);
                    if (!object.ReferenceEquals(null, svgcontainer))
                    {
                        svgcontainer.Id = string.Format(SvgItemIdPadFormat, poly.Room, poly.OrderIndex);
                        svgcontainer.AddClass("gelib-pad");

                        var scaledPos = poly.Origin.Clone().Scale(1 / Stage.LevelScale);

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

            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.AmmoBox, svg, SvgSetupAmmoLayerId, SvgItemIdSetupAmmoFormat);

            // safe should be under door z layer.
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.Safe, svg, SvgSetupSafeLayerId, SvgItemIdSetupSafeFormat);
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.Door, svg, SvgSetupDoorLayerId, SvgItemIdSetupDoorFormat);
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.Alarm, svg, SvgSetupAlarmLayerId, SvgItemIdSetupAlarmFormat);
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.Cctv, svg, SvgSetupCctvLayerId, SvgItemIdSetupCctvFormat);
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.Drone, svg, SvgSetupDroneLayerId, SvgItemIdSetupDroneFormat);
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.Aircraft, svg, SvgSetupAircraftLayerId, SvgItemIdSetupAircraftFormat);
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.Tank, svg, SvgSetupTankLayerId, SvgItemIdSetupTankFormat);
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.StandardProp, svg, SvgSetupStandardPropLayerId, SvgItemIdSetupStandardPropFormat);
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.SingleMonitor, svg, SvgSetupSingleMonitorLayerId, SvgItemIdSetupSingleMonitorFormat);
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.Collectable, svg, SvgSetupCollectableLayerId, SvgItemIdSetupCollectableFormat);
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.Armour, svg, SvgSetupBodyArmorLayerId, SvgItemIdSetupBodyArmorFormat);

            // keys should be on top of tables (StandardProp)
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.Key, svg, SvgSetupKeyLayerId, SvgItemIdSetupKeyFormat);

            // guards should be one of the highest z layers
            AddSetupGroupToSvgDoc(setupPolygonsCollection, PropDef.Guard, svg, SvgSetupChrLayerId, SvgItemIdSetupChrFormat);

            if (introPolygons.Any())
            {
                var group = svg.AddGroup();
                group.Id = SvgSetupIntroLayerId;
                foreach (var poly in introPolygons)
                {
                    var point = poly.Origin.To2DXZ().Scale(1.0 / Stage.LevelScale);

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

                    var scaledPos = poly.Origin.Clone().Scale(1 / Stage.LevelScale);

                    container.SetDataAttribute("sc-x", scaledPos.X.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("sc-y", scaledPos.Y.ToString(StandardDoubleToStringFormat));
                    container.SetDataAttribute("sc-z", scaledPos.Z.ToString(StandardDoubleToStringFormat));

                    container.SetDataAttribute("room-id", poly.Room.ToString());
                }
            }

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
                    var svgprop = SvgAppend.PropToSvg.SetupObjectToSvgAppend(group, poly, Stage.LevelScale);
                    if (!object.ReferenceEquals(null, svgprop))
                    {
                        svgprop.Id = string.Format(itemFormatString, poly.Room, poly.OrderIndex);

                        AddPropAttributes(svgprop, poly);

                        svgprop.SetDataAttribute("nc-x", poly.Origin.X.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("nc-y", poly.Origin.Y.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("nc-z", poly.Origin.Z.ToString(StandardDoubleToStringFormat));

                        var scaledPos = poly.Origin.Clone().Scale(1 / Stage.LevelScale);

                        svgprop.SetDataAttribute("sc-x", scaledPos.X.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("sc-y", scaledPos.Y.ToString(StandardDoubleToStringFormat));
                        svgprop.SetDataAttribute("sc-z", scaledPos.Z.ToString(StandardDoubleToStringFormat));

                        svgprop.SetDataAttribute("room-id", poly.Room.ToString());
                    }
                }
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
        /// <param name="mesh"></param>
        /// <param name="z"></param>
        /// <returns></returns>
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
        /// <param name="mesh"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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
        /// <param name="mesh"></param>
        /// <param name="ymin"></param>
        /// <param name="ymax"></param>
        /// <returns></returns>
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

        private bool TryGetRoomId(string presetName, out byte roomId)
        {
            roomId = 0;

            if (string.IsNullOrEmpty(presetName))
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
                    tileGroupId = 8 * Base26ToInt(rematch.Groups[2].Value[0]);
                    if (!string.IsNullOrEmpty(rematch.Groups[3].Value))
                    {
                        tileGroupId += int.Parse(rematch.Groups[3].Value);
                    }
                    tile = Stage.Stan.Tiles.FirstOrDefault(x => x.TileId == tilenameDecimal && x.GroupId == tileGroupId);
                }
                else
                {
                    tile = Stage.Stan.Tiles.FirstOrDefault(x => x.TileId == tilenameDecimal);
                }

                if (!object.ReferenceEquals(null, tile))
                {
                    roomId = tile.Room;
                    return true;
                }
            }

            return false;
        }

        private int Base26ToInt(char c)
        {
            return (int)(c - 'a');
        }

        private void AddPropAttributes(SvgContainer container, PropPosition prop)
        {
            var baseObject = prop.SetupObject as SetupObjectBase;

            // guard does not implement SetupObjectGenericBase
            SetupObjectGenericBase baseGenericObject = prop.SetupObject as SetupObjectGenericBase;

            container.SetDataAttribute("propdef-name", prop.SetupObject.Type.ToString());
            container.SetDataAttribute("propdef-id", ((int)prop.SetupObject.Type).ToString());

            if (baseGenericObject != null)
            {
                var propid = (PropId)baseGenericObject.ObjectId;
                container.SetDataAttribute("prop-name", propid.ToString());
                container.SetDataAttribute("prop-id", ((int)propid).ToString());
            }
        }

        private class CollectionHullSvgPoints
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

            public Dictionary<string, string> SvgDataAttributes { get; set; } = new Dictionary<string, string>();
        }

        private class SingularSvgPoints
        {
            public int OrderIndex { get; set; }

            public int Room { get; set; }

            // native preset coordinate value
            public Coord3dd NaturalOrigin { get; set; } = Coord3dd.Zero.Clone();

            public Coord2dd Point { get; set; }

            public ISetupObject? SetupObject { get; set; }

            public Coord3dd Up { get; set; }

            public Coord3dd Orientation { get; set; }

            public Dictionary<string, string> SvgDataAttributes { get; set; } = new Dictionary<string, string>();
        }
    }
}
