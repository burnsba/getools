using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Dto;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Win.Controls;
using Gebug64.Win.Enum;
using Gebug64.Win.Event;
using Gebug64.Win.Mvvm;
using Gebug64.Win.ViewModels.Config;
using Gebug64.Win.ViewModels.Game;
using Gebug64.Win.ViewModels.Map;
using Gebug64.Win.Windows.Mdi;
using Gebug64.Win.Wpf;
using Getools.Lib.Converters;
using Getools.Lib.Extensions;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Bg;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.SetupObject;
using Getools.Lib.Game.Asset.Stan;
using Getools.Lib.Game.Engine;
using Getools.Lib.Game.EnumModel;
using Getools.Lib.Game.Enums;
using Getools.Lib.Kaitai.Gen;
using Getools.Palantir;
using Getools.Palantir.Render;
using Getools.Utility.Logging;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// Splits the primary map building methods into their own file.
    /// </summary>
    public partial class MapWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// This variable holds the last intro coordinates read. Used to set Bond's initial position on stage load.
        /// </summary>
        private Coord3dd _lastIntroPosition = Coord3dd.Zero.Clone();

        /// <summary>
        /// Queue up layers to add to the main collection, all in one batch.
        /// This minimizes multiple calls to dispatcher.invoke.
        /// </summary>
        private List<MapLayerViewModel> _mapBuildLayersToAdd = new List<MapLayerViewModel>();

        private void LoadStageEntry(LevelIdX stageid)
        {
            lock (_lock)
            {
                if (_isLoading)
                {
                    return;
                }

                _isLoading = true;
            }

            _mapBuildLayersToAdd.Clear();

            // Fighting with the dispatcher and dependency source object creation on the
            // right thread is not really making this any faster, so just going to block
            // the main thread.
            LoadStage(stageid);

            // wait for the UI thread before signalling the map is ready to be used.
            _dispatcher.BeginInvoke(() =>
            {
                foreach (var layer in _mapBuildLayersToAdd)
                {
                    Layers.Add(layer);
                }

                IsMapLoaded = true;
            });

            lock (_lock)
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Don't call this method directly, use <see cref="LoadStageEntry"/> to ensure proper locks/notifications.
        /// </summary>
        /// <param name="stageid">Stage to load.</param>
        private void LoadStage(LevelIdX stageid)
        {
            IsMapLoaded = false;

            if (!LevelIdX.SinglePlayerStages.Contains(stageid))
            {
                _logger.LogError($"{nameof(LoadStage)}: id {stageid} not supported");
                return;
            }

            if (string.IsNullOrEmpty(_setupBinFolder))
            {
                _logger.LogError($"{nameof(LoadStage)}: setup bin folder not set");
                return;
            }

            if (string.IsNullOrEmpty(_stanBinFolder))
            {
                _logger.LogError($"{nameof(LoadStage)}: stan bin folder not set");
                return;
            }

            if (string.IsNullOrEmpty(_bgBinFolder))
            {
                _logger.LogError($"{nameof(LoadStage)}: bg bin folder not set");
                return;
            }

            StageSetupFile? setup = null;
            StandFile? stan = null;
            BgFile? bg = null;

            var setupFileMap = Getools.Lib.Game.Asset.Setup.SourceMap.GetFileMap(Getools.Lib.Game.Enums.Version.Ntsc, stageid.Id);
            if (string.IsNullOrEmpty(setupFileMap.Filename))
            {
                _logger.LogError($"{nameof(LoadStage)}: Could not resolve setup relative path");
                return;
            }

            var stanFileMap = Getools.Lib.Game.Asset.Stan.SourceMap.GetFileMap(stageid.Id);
            if (string.IsNullOrEmpty(stanFileMap.Filename))
            {
                _logger.LogError($"{nameof(LoadStage)}: Could not resolve stan relative path");
                return;
            }

            var bgFileMap = Getools.Lib.Game.Asset.Bg.SourceMap.GetFileMap(Getools.Lib.Game.Enums.Version.Ntsc, stageid.Id);
            if (string.IsNullOrEmpty(bgFileMap.Filename))
            {
                _logger.LogError($"{nameof(LoadStage)}: Could not resolve bg relative path");
                return;
            }

            string setupFilePath;
            string stanFilePath;
            string bgFilePath;

            if (string.IsNullOrEmpty(setupFileMap.Dir))
            {
                setupFilePath = Path.Combine(_setupBinFolder, setupFileMap.Filename + ".bin");
            }
            else
            {
                setupFilePath = Path.Combine(_setupBinFolder, setupFileMap.Dir, setupFileMap.Filename + ".bin");
            }

            if (string.IsNullOrEmpty(stanFileMap.Dir))
            {
                stanFilePath = Path.Combine(_stanBinFolder, stanFileMap.Filename + ".bin");
            }
            else
            {
                stanFilePath = Path.Combine(_stanBinFolder, stanFileMap.Dir, stanFileMap.Filename + ".bin");
            }

            if (string.IsNullOrEmpty(bgFileMap.Dir))
            {
                bgFilePath = Path.Combine(_bgBinFolder, bgFileMap.Filename + ".bin");
            }
            else
            {
                bgFilePath = Path.Combine(_bgBinFolder, bgFileMap.Dir, bgFileMap.Filename + ".bin");
            }

            if (!File.Exists(setupFilePath))
            {
                _logger.LogError($"{nameof(LoadStage)}: setup not found: {setupFilePath}");
                return;
            }

            if (!File.Exists(stanFilePath))
            {
                _logger.LogError($"{nameof(LoadStage)}: stan not found: {stanFilePath}");
                return;
            }

            if (!File.Exists(bgFilePath))
            {
                _logger.LogError($"{nameof(LoadStage)}: bg not found: {bgFilePath}");
                return;
            }

            setup = SetupConverters.ReadFromBinFile(setupFilePath);
            stan = StanConverters.ReadFromBinFile(stanFilePath, "ignore");
            bg = BgConverters.ReadFromBinFile(bgFilePath);

            _levelScale = Getools.Lib.Game.Asset.StageScale.GetStageScale(stageid.Id);

            var stage = new Getools.Palantir.Stage()
            {
                Bg = bg,
                Setup = setup,
                Stan = stan,
                LevelScale = _levelScale,
            };

            var mim = new Getools.Palantir.MapImageMaker(stage);

            var rawStage = mim.GetFullRawStage();

            // Find offset to zero axis.
            _adjustx = 0 - rawStage.ScaledMin.X;
            _adjusty = 0 - rawStage.ScaledMin.Z;

            // The resulting output should cover the range of the level.
            // This will be the "view box". It might have to scale to accomodate
            // the desired output width.
            var outputVeiwboxWidth = Math.Abs(rawStage.ScaledMax.X - rawStage.ScaledMin.X);
            var outputViewboxHeight = Math.Abs(rawStage.ScaledMax.Z - rawStage.ScaledMin.Z);

            WaitClearMap();

            AddStanLayer(rawStage, _adjustx, _adjusty);

            // room boundaries need to be above the tiles.
            AddBgLayer(rawStage, _adjustx, _adjusty);

            AddPadLayer(rawStage, _adjustx, _adjusty, _levelScale);

            // draw path waypoints on top of pads
            AddPathWaypointLayer(rawStage, _adjustx, _adjusty, _levelScale);

            // draw patrol paths on top of pads and waypoints
            AddPatrolPathLayer(rawStage, _adjustx, _adjusty, _levelScale);

            AddSetupLayer(rawStage, PropDef.AmmoBox, _adjustx, _adjusty, _levelScale);

            // safe should be under door z layer.
            AddSetupLayer(rawStage, PropDef.Safe, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Door, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Alarm, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Cctv, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Drone, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Aircraft, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Tank, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.StandardProp, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.SingleMonitor, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Collectable, _adjustx, _adjusty, _levelScale);
            AddSetupLayer(rawStage, PropDef.Armour, _adjustx, _adjusty, _levelScale);

            // keys should be on top of tables (StandardProp)
            AddSetupLayer(rawStage, PropDef.Key, _adjustx, _adjusty, _levelScale);

            // guards should be one of the highest z layers
            AddSetupLayer(rawStage, PropDef.Guard, _adjustx, _adjusty, _levelScale);
            _guardLayer = _mapBuildLayersToAdd.First(x => x.LayerId == UiMapLayer.SetupGuard);

            // Intro star should be above other layers
            AddIntroLayer(rawStage, _adjustx, _adjusty, _levelScale);

            // Create a reference for Bond and place in a new layer, on top of everything else.
            AddBondLayer(rawStage, _adjustx, _adjusty, _levelScale);

            MapScaledWidth = outputVeiwboxWidth;
            MapScaledHeight = outputViewboxHeight;

            _logger.LogInformation($"Map viewer: done loading {stageid.Name}");

            MapMinVertical = rawStage.ScaledMin.Y;
            MapMaxVertical = rawStage.ScaledMax.Y;
            MapSelectedMinVertical = rawStage.ScaledMin.Y;
            MapSelectedMaxVertical = rawStage.ScaledMax.Y;
        }

        private void WaitClearMap()
        {
            if (_dispatcher.Thread == Thread.CurrentThread)
            {
                Layers.Clear();
            }
            else
            {
                bool done = false;

                _dispatcher.BeginInvoke(() =>
                {
                    Layers.Clear();
                    done = true;
                });

                while (!done)
                {
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Removes items that were added from gebug messges receieved at runtime.
        /// </summary>
        private void RemoveRuntimeItems()
        {
            if (object.ReferenceEquals(null, _guardLayer))
            {
                return;
            }

            _guardLayer!.DispatchRemoveAll(_dispatcher);
        }

        private void ToggleLayerVisibility(UiMapLayer layerId)
        {
            var layer = Layers.FirstOrDefault(x => x.LayerId == layerId);
            if (object.ReferenceEquals(null, layer))
            {
                return;
            }

            layer.IsVisible = !layer.IsVisible;

            _appConfig.Map.SetMapLayerVisibility(layerId, layer.IsVisible);
        }

        private void AddBgLayer(ProcessedStageData rawStage, double adjustx, double adjusty)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.Bg)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.Bg],
            };

            foreach (var poly in rawStage.RoomPolygons)
            {
                var pc = new PointCollection();

                var first = poly.Points!.First();

                foreach (var p in poly.Points!)
                {
                    pc.Add(new Point(p.X + adjustx, p.Y + adjusty));
                }

                pc = pc.AbsoluteToRelative();

                mo = new MapObjectPoly(first.X + adjustx, first.Y + adjusty)
                {
                    Points = pc,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 12,
                    Fill = Brushes.Transparent,
                };

                mo.ScaledMin = poly.ScaledMin.Clone();
                mo.ScaledMax = poly.ScaledMax.Clone();

                mo.DataSource = new Game.Bg(_logger, _connectionServiceResolver)
                {
                    LayerIndexId = poly.Room,
                };

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            _mapBuildLayersToAdd.Add(layer);
        }

        private void AddStanLayer(ProcessedStageData rawStage, double adjustx, double adjusty)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.Stan)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.Stan],
            };

            foreach (var poly in rawStage.TilePolygons)
            {
                var pc = new PointCollection();

                var first = poly.Points!.First();

                foreach (var p in poly.Points!)
                {
                    pc.Add(new Point(p.X + adjustx, p.Y + adjusty));
                }

                pc = pc.AbsoluteToRelative();

                mo = new MapObjectPoly(first.X + adjustx, first.Y + adjusty)
                {
                    Points = pc,
                    Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#9e87a3")!,
                    StrokeThickness = 2,
                    Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#fdf5ff")!,
                };

                mo.ScaledMin = poly.ScaledMin.Clone();
                mo.ScaledMax = poly.ScaledMax.Clone();

                mo.DataSource = new Game.Stan(_logger, _connectionServiceResolver)
                {
                    LayerIndexId = poly.OrderIndex,
                    LayerInstanceId = poly.StanNameId,
                };

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            _mapBuildLayersToAdd.Add(layer);
        }

        private void AddIntroLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.SetupIntro)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupIntro],
            };

            foreach (var pp in rawStage.IntroPolygons)
            {
                mo = GetPropDefaultBbox_intro(pp, adjustx, adjusty, levelScale);

                mo.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

                mo.DataSource = new Game.GameObject(_logger, _connectionServiceResolver);

                _lastIntroPosition = mo.ScaledOrigin.Clone();

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            _mapBuildLayersToAdd.Add(layer);
        }

        private void AddBondLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;

            layer = new MapLayerViewModel(Enum.UiMapLayer.Bond)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.Bond],
            };

            Bond = GetPropDefaultBond(new PropPointPosition(), adjustx, adjusty, levelScale);

            // set Bond's position to (last) intro
            Bond.ScaledOrigin.X = _lastIntroPosition.X;
            Bond.ScaledOrigin.Y = _lastIntroPosition.Y;
            Bond.ScaledOrigin.Z = _lastIntroPosition.Z;

            layer.DispatchAddEntity(_dispatcher, Bond);

            _mapBuildLayersToAdd.Add(layer);
        }

        private void AddSetupLayer(ProcessedStageData rawStage, PropDef setupLayerKey, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            var uilayerid = PropDefToUiLayer(setupLayerKey);

            layer = new MapLayerViewModel(uilayerid)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[uilayerid],
            };

            if (!rawStage.SetupPolygonsCollection.ContainsKey(setupLayerKey))
            {
                _mapBuildLayersToAdd.Add(layer);
                return;
            }

            var collection = rawStage.SetupPolygonsCollection[setupLayerKey];

            foreach (var pp in collection)
            {
                if (object.ReferenceEquals(null, pp.SetupObject))
                {
                    throw new NullReferenceException($"{nameof(pp.SetupObject)}");
                }

                // msgGuard does not implement SetupObjectGenericBase
                SetupObjectGenericBase? baseObject = pp.SetupObject as SetupObjectGenericBase;

                switch (pp.SetupObject.Type)
                {
                    case PropDef.Door:
                        mo = GetPropDefaultModelBbox_door(pp, adjustx, adjusty, levelScale);
                        break;

                    case PropDef.Guard:
                        mo = GetPropDefaultModelBbox_chr(pp, adjustx, adjusty, levelScale);

                        // Set chr id from setup file
                        var guard = (SetupObjectGuard)pp.SetupObject;
                        mo.DataSource!.LayerInstanceId = (int)guard.ObjectId;

                        break;

                    case PropDef.AmmoBox:
                        mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#274d23", 4, "#66ed58");
                        break;

                    case PropDef.Alarm:
                        mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#cccccc", 1, "#ff0000");
                        break;

                    case PropDef.Armour:
                        mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#0c1c63", 4, "#0000ff");
                        break;

                    case PropDef.Key:
                        mo = GetPropDefaultBbox_key(pp, adjustx, adjusty, levelScale);
                        break;

                    case PropDef.Cctv:
                        mo = GetPropDefaultBbox_cctv(pp, adjustx, adjusty, levelScale);
                        break;

                    case PropDef.Aircraft:
                        mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#808018", 4, "#dbdb60");
                        break;

                    case PropDef.Collectable: // fallthrough
                    case PropDef.Drone:
                    case PropDef.Safe:
                    case PropDef.SingleMonitor:
                    case PropDef.StandardProp:
                        {
                            if (object.ReferenceEquals(null, baseObject))
                            {
                                throw new NullReferenceException();
                            }

                            switch ((PropId)baseObject.ObjectId)
                            {
                                // hat
                                case PropId.PROP_HATFURRY:
                                case PropId.PROP_HATFURRYBROWN:
                                case PropId.PROP_HATFURRYBLACK:
                                case PropId.PROP_HATTBIRD:
                                case PropId.PROP_HATTBIRDBROWN:
                                case PropId.PROP_HATHELMET:
                                case PropId.PROP_HATHELMETGREY:
                                case PropId.PROP_HATMOON:
                                case PropId.PROP_HATBERET:
                                case PropId.PROP_HATBERETBLUE:
                                case PropId.PROP_HATBERETRED:
                                case PropId.PROP_HATPEAKED:
                                    // skip
                                    continue;

                                case PropId.PROP_PADLOCK:
                                    mo = GetPropDefaultBbox_lock(pp, adjustx, adjusty, levelScale);
                                    break;

                                case PropId.PROP_GUN_RUNWAY1:
                                case PropId.PROP_ROOFGUN:
                                case PropId.PROP_GROUNDGUN:
                                    mo = GetPropDefaultBbox_heavygun(pp, adjustx, adjusty, levelScale);
                                    break;

                                case PropId.PROP_CHRGOLDENEYEKEY:
                                    mo = GetPropDefaultBbox_key(pp, adjustx, adjusty, levelScale);
                                    break;

                                case PropId.PROP_MAINFRAME1:
                                case PropId.PROP_MAINFRAME2:
                                case PropId.PROP_DOOR_MF:
                                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#666666", 4, "#94dff2");
                                    break;

                                case PropId.PROP_AMMO_CRATE1:
                                case PropId.PROP_AMMO_CRATE2:
                                case PropId.PROP_AMMO_CRATE3:
                                case PropId.PROP_AMMO_CRATE4:
                                case PropId.PROP_AMMO_CRATE5:
                                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#274d23", 4, "#66ed58");
                                    break;

                                case PropId.PROP_CHRCIRCUITBOARD:
                                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#009900", 4, "#00ff00");
                                    break;

                                case PropId.PROP_CHRVIDEOTAPE:
                                case PropId.PROP_CHRDOSSIERRED:
                                case PropId.PROP_CHRSTAFFLIST:
                                case PropId.PROP_CHRCLIPBOARD:
                                case PropId.PROP_DISK_DRIVE1:
                                case PropId.PROP_CHRDATTAPE:
                                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#ff0000", 4, "#ff0000");
                                    break;

                                case PropId.PROP_TIGER:
                                case PropId.PROP_MILCOPTER:
                                case PropId.PROP_HELICOPTER:
                                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#808018", 4, "#dbdb60");
                                    break;

                                case PropId.PROP_ALARM1:
                                case PropId.PROP_ALARM2:
                                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#cccccc", 1, "#ff0000");
                                    break;

                                default:
                                    mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#916b2a", 4, "#ffdfa8");
                                    break;
                            }

                            break;
                        }

                    case PropDef.Tank:
                        mo = GetPropDefaultModelBbox_prop(pp, adjustx, adjusty, levelScale, "#255c25", 4, "#00ff00");
                        break;

                    default:
                        throw new NotImplementedException();
                }

                mo.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

                mo.DataSource!.PropDefType = pp.SetupObject.Type;
                mo.DataSource!.LayerIndexId = pp.SetupObject.SetupSectionTypeIndex;

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            _mapBuildLayersToAdd.Add(layer);
        }

        private void AddPadLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.SetupPad)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupPad],
            };

            var collection = rawStage.PresetPolygons;

            foreach (var pp in collection)
            {
                mo = GetPadBbox(pp, adjustx, adjusty, levelScale);

                mo.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

                mo.DataSource = new Game.Pad(_logger, _connectionServiceResolver)
                {
                    LayerIndexId = pp.PadId,
                };

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            _mapBuildLayersToAdd.Add(layer);
        }

        private void AddPathWaypointLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.SetupPathWaypoint)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupPathWaypoint],
            };

            var collection = rawStage.PathWaypointLines;

            foreach (var renderLine in collection)
            {
                var p1 = renderLine.P1.Scale(1.0 / levelScale);
                var p2 = renderLine.P2.Scale(1.0 / levelScale);

                double uix = p1.X + adjustx;
                double uiy = p1.Z + adjusty;

                double uix2 = p2.X - p1.X;
                double uiy2 = p2.Z - p1.Z;

                mo = new MapObjectLine(uix, uiy)
                {
                    P1 = new Point(0, 0),
                    P2 = new Point(uix2, uiy2),
                    StrokeThickness = 12,
                    Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#ff80ff")!,
                };

                var scaledPos = renderLine.Bbox.Scale(1 / levelScale);

                mo.ScaledMin.X = scaledPos.MinX;
                mo.ScaledMin.Y = scaledPos.MinY;
                mo.ScaledMin.Z = scaledPos.MinZ;
                mo.ScaledMax.X = scaledPos.MaxX;
                mo.ScaledMax.Y = scaledPos.MaxY;
                mo.ScaledMax.Z = scaledPos.MaxZ;

                mo.DataSource = new Game.GameObject(_logger, _connectionServiceResolver);

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            _mapBuildLayersToAdd.Add(layer);
        }

        private void AddPatrolPathLayer(ProcessedStageData rawStage, double adjustx, double adjusty, double levelScale)
        {
            MapLayerViewModel layer;
            MapObject mo;

            layer = new MapLayerViewModel(Enum.UiMapLayer.SetupPatrolPath)
            {
                IsVisible = _appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupPatrolPath],
            };

            var collection = rawStage.PatrolPathLines;

            foreach (var pathset in collection)
            {
                var pc = new PointCollection();

                var scaledPoints = pathset.Points.Select(x => x.Scale(1.0 / levelScale).To2DXZ());
                var first = scaledPoints.First();

                foreach (var p in scaledPoints)
                {
                    pc.Add(new Point(p.X + adjustx, p.Y + adjusty));
                }

                pc = pc.AbsoluteToRelative();

                mo = new MapObjectPolyLine(first.X + adjustx, first.Y + adjusty)
                {
                    Points = pc,
                    Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#3fb049")!,
                    StrokeThickness = 18,
                };

                var scaledPos = pathset.Bbox.Scale(1 / levelScale);

                mo.ScaledMin.X = scaledPos.MinX;
                mo.ScaledMin.Y = scaledPos.MinY;
                mo.ScaledMin.Z = scaledPos.MinZ;
                mo.ScaledMax.X = scaledPos.MaxX;
                mo.ScaledMax.Y = scaledPos.MaxY;
                mo.ScaledMax.Z = scaledPos.MaxZ;

                mo.DataSource = new Game.GameObject(_logger, _connectionServiceResolver);

                layer.DispatchAddEntity(_dispatcher, mo);
            }

            _mapBuildLayersToAdd.Add(layer);
        }

        private MapObject GetPropDefaultModelBbox_prop(PropPointPosition pp, double adjustx, double adjusty, double levelScale, string stroke, double strokeWidth, string fill)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_prop(pp, levelScale);

            var mor = new MapObjectRect(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

            mor.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom(stroke)!;
            mor.StrokeThickness = strokeWidth;
            mor.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(fill)!;

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X;
            mor.UiHeight = stagePosition.ModelSize.Z;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPropDefaultModelBbox_door(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_door(pp, levelScale);

            var mor = new MapObjectRect(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

            mor.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#8d968e")!;
            mor.StrokeThickness = 2;
            mor.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#e1ffdb")!;

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X;
            mor.UiHeight = stagePosition.ModelSize.Z;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPropDefaultModelBbox_chr(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_chr(pp, levelScale);

            var mor = new MapObjectResourceImage(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/chr.png";

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X * _wpfGuardModelScaleFactor;
            mor.UiHeight = stagePosition.ModelSize.Z * _wpfGuardModelScaleFactor;

            mor.DataSource = new Game.Chr(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPropDefaultBond(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_chr(pp, levelScale);

            var mor = new MapObjectResourceImage(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.ScaledOrigin = pp.Origin.Clone().Scale(1 / levelScale);

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/bond.png";

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X * _wpfGuardModelScaleFactor;
            mor.UiHeight = stagePosition.ModelSize.Z * _wpfGuardModelScaleFactor;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPropDefaultBbox_key(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            double w = 34;
            double h = 112;
            double hw = w / 2;
            double hh = h / 2;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var mor = new MapObjectResourceImage(
                pos.X - hw + adjustx,
                pos.Z - hh + adjusty);

            mor.ScaledOrigin = pos;

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/key.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPropDefaultBbox_cctv(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            double w = 90;
            double h = 90;
            double hw = w / 2;
            double hh = h / 2;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var mor = new MapObjectResourceImage(
                pos.X - hw + adjustx,
                pos.Z - hh + adjusty);

            mor.ScaledOrigin = pos;

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/cctv.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPropDefaultBbox_lock(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            double w = 34;
            double h = 112;
            double hw = w / 2;
            double hh = h / 2;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var mor = new MapObjectResourceImage(
                pos.X - hw + adjustx,
                pos.Z - hh + adjusty);

            mor.ScaledOrigin = pos;

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/lock.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPropDefaultBbox_heavygun(PropPointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            double w = 140;
            double h = 140;
            double hw = w / 2;
            double hh = h / 2;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var mor = new MapObjectResourceImage(
                pos.X - hw + adjustx,
                pos.Z - hh + adjusty);

            mor.ScaledOrigin = pos;

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/heavy_gun.png";

            mor.RotationDegree = rotAngle;
            mor.UiWidth = w;
            mor.UiHeight = h;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPropDefaultBbox_intro(PointPosition pp, double adjustx, double adjusty, double levelScale)
        {
            double w = 90;
            double h = 90;
            double hw = w / 2;
            double hh = h / 2;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            var mor = new MapObjectResourceImage(
                pos.X - hw + adjustx,
                pos.Z - hh + adjusty);

            mor.ScaledOrigin = pos;

            mor.ResourcePath = "/Gebug64.Win;component/Resource/Image/star.png";

            mor.UiWidth = w;
            mor.UiHeight = h;

            mor.DataSource = new Game.Prop(_logger, _connectionServiceResolver)
            {
                PropPos = mor.ScaledOrigin.Clone(),
            };

            return mor;
        }

        private MapObject GetPadBbox(PointPosition rp, double adjustx, double adjusty, double levelScale)
        {
            var stagePosition = Getools.Lib.Game.Engine.World.GetPadBbox(rp, levelScale);

            var mor = new MapObjectRect(
                stagePosition.Origin.X - stagePosition.HalfModelSize.X + adjustx,
                stagePosition.Origin.Z - stagePosition.HalfModelSize.Z + adjusty);

            mor.ScaledOrigin = rp.Origin.Clone().Scale(1 / levelScale);

            mor.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#9c009e")!;
            mor.StrokeThickness = 4;
            mor.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#ff00ff")!;

            mor.RotationDegree = stagePosition.RotationDegrees;
            mor.UiWidth = stagePosition.ModelSize.X;
            mor.UiHeight = stagePosition.ModelSize.Z;

            return mor;
        }
    }
}
