using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Extensions;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Model;
using Getools.Lib.Game.Asset.SetupObject;
using Getools.Lib.Game.Enums;
using Getools.Lib.Math;
using Getools.Palantir.Render;
using SvgLib;
using static System.Formats.Asn1.AsnWriter;

namespace Getools.Palantir.SvgProp
{
    internal static class PropToSvg
    {
        internal static SvgContainer? SetupObjectToSvgAppend(SvgGroup appendTo, RenderPosition rp, double levelScale)
        {
            if (object.ReferenceEquals(null, rp))
            {
                throw new NullReferenceException($"{nameof(rp)}");
            }

            if (rp is PropPosition)
            {
                var pp = (PropPosition)rp;

                if (object.ReferenceEquals(null, pp.SetupObject))
                {
                    throw new NullReferenceException($"{nameof(pp.SetupObject)}");
                }

                switch (pp.SetupObject.Type)
                {
                    case PropDef.Door:
                        return SvgAppendPropDefaultModelBbox_door(appendTo, pp, levelScale, "#8d968e", 4, "#e1ffdb");
                    
                    case PropDef.Aircraft:
                    case PropDef.StandardProp:
                        return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale, "#8d968e", 4, "#e1ffdb");
                }

                throw new NotImplementedException();
            }
            else
            {
                //
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appendTo"></param>
        /// <param name="setupObject"></param>
        /// <param name="point">Output svg origin point to add object.</param>
        /// <param name="up">Original preset "up" vector.</param>
        /// <param name="orientation">Original preset "look" vector.</param>
        /// <returns></returns>
        internal static SvgContainer? SetupObjectToSvgAppend(SvgGroup appendTo, ISetupObject setupObject, Coord3dd point3d, Coord3dd up, Coord3dd orientation)
        {
            var scaleUpper = (setupObject.Scale & 0xff00) >> 8;
            var scaleLower = setupObject.Scale & 0xff;
            double scale = scaleUpper + ((double)scaleLower / (double)256);

            var point = point3d.To2DXZ();

            // Objects that don't depend on which PROP.
            switch (setupObject.Type)
            {
                case Lib.Game.Enums.PropDef.Alarm:
                    return SvgGroupAppendAlarm(appendTo, point);

                case Lib.Game.Enums.PropDef.AmmoBox:
                    return SvgAppendRect(appendTo, point, up, orientation, 40, 14, "#274d23", 4, "#66ed58");

                case Lib.Game.Enums.PropDef.Cctv:
                    return SvgGroupAppendCctv(appendTo, point, up, orientation);

                case Lib.Game.Enums.PropDef.Guard:
                    return SvgGroupAppendChr(appendTo, point, up, orientation);

                case Lib.Game.Enums.PropDef.Key:
                    return SvgGroupAppendKey(appendTo, point);

                case Lib.Game.Enums.PropDef.Safe:
                    return SvgAppendSafe(appendTo, point);
            }

            // guard does not implement SetupObjectGenericBase
            SetupObjectGenericBase baseObject = setupObject as SetupObjectGenericBase;

            if (baseObject == null)
            {
                return null;
            }

            switch ((PropId)baseObject.ObjectId)
            {
                case PropId.PROP_AMMO_CRATE1:
                    return SvgAppendRect(appendTo, point, up, orientation, 40, 14, "#274d23", 4, "#66ed58");

                case PropId.PROP_SEV_DOOR:
                case PropId.PROP_SEV_DOOR3:
                case PropId.PROP_SEVDOORWOOD:
                case PropId.PROP_SEVDOORWIND:
                case PropId.PROP_SEVDOORNOWIND:
                case PropId.PROP_SEVDOORMETSLIDE:
                case PropId.PROP_GAS_PLANT_MET1_DO1: // door
                case PropId.PROP_GAS_PLANT_SW_DO1: // door
                case PropId.PROP_GAS_PLANT_SW2_DO1: // door
                case PropId.PROP_GAS_PLANT_SW3_DO1: // door
                case PropId.PROP_GAS_PLANT_SW4_DO1: // door
                case PropId.PROP_GASPLANT_CLEAR_DOOR: // door
                case PropId.PROP_SEV_DOOR4_WIND: // door
                case PropId.PROP_SEV_DOOR_V1: // door
                case PropId.PROP_SEV_TRISLIDE: // door
                case PropId.PROP_STEEL_DOOR1: // door
                case PropId.PROP_STEEL_DOOR3: // door
                case PropId.PROP_SILO_LIFT_DOOR: // door
                case PropId.PROP_DOOR_DEST1: // door
                case PropId.PROP_DOORPRISON1: // door
                case PropId.PROP_TRAINEXTDOOR: // door
                case PropId.PROP_TRAIN_DOOR: // door
                case PropId.PROP_TRAIN_DOOR2: // door
                case PropId.PROP_TRAIN_DOOR3: // door
                case PropId.PROP_DOOR_ST_AREC1: // door
                case PropId.PROP_DOOR_ST_AREC2: // door
                case PropId.PROP_DOOR_EYELID: // door
                case PropId.PROP_DOOR_IRIS: // door
                case PropId.PROP_DOOR_AZTEC: // door
                case PropId.PROP_DOOR_WIN: // aztec glass sliding door
                case PropId.PROP_CRYPTDOOR1A: // door
                case PropId.PROP_CRYPTDOOR1B: // door
                case PropId.PROP_CRYPTDOOR2A: // door
                case PropId.PROP_CRYPTDOOR2B: // door
                case PropId.PROP_CRYPTDOOR3: // door
                case PropId.PROP_CRYPTDOOR4: // door
                    return SvgAppendRect(appendTo, point, up, orientation, 50, 18, "#8d968e", 4, "#e1ffdb");

                case PropId.PROP_ARCHSECDOOR1: // door
                case PropId.PROP_ARCHSECDOOR2: // door
                    return SvgAppendRect(appendTo, point, up, orientation, 80, 24, "#8d968e", 4, "#e1ffdb");

                case PropId.PROP_VERTDOOR: // blast door, control
                    return SvgAppendRect(appendTo, point, up, orientation, 80, 24, "#8d968e", 4, "#e1ffdb");

                case PropId.PROP_SILOTOPDOOR: // door
                    return SvgAppendRect(appendTo, point, up, orientation, 50, 50, "#8d968e", 4, "#e1ffdb");

                case PropId.PROP_CHRGOLDENEYEKEY:
                    return SvgGroupAppendKey(appendTo, point);

                case PropId.PROP_DOOR_ROLLER4:
                    return SvgAppendRect(appendTo, point, up, orientation, 60, 6, "#666666", 2, "#aaaaaa");

                case PropId.PROP_DOORSTATGATE:
                    return SvgAppendRect(appendTo, point, up, orientation, 60, 6, "#666666", 2, "#aaaaaa");

                case PropId.PROP_GAS_PLANT_WC_CUB1: // door
                    return SvgAppendRect(appendTo, point, up, orientation, 35, 14, "#666666", 4, "#94dff2");

                case PropId.PROP_LOCKER3:
                case PropId.PROP_LOCKER4:
                    return SvgAppendRect(appendTo, point, up, orientation, 40, 40, "#666666", 2, "#aaaaaa");

                case PropId.PROP_FILING_CABINET1:
                    return SvgAppendRect(appendTo, point, up, orientation, 20, 20, "#666666", 2, "#aaaaaa");

                case PropId.PROP_DOOR_ROLLERTRAIN:
                    return SvgAppendRect(appendTo, point, up, orientation, 80, 18, "#666666", 2, "#aaaaaa");

                case PropId.PROP_WOODEN_TABLE1:
                    return SvgAppendRect(appendTo, point, up, orientation, 50, 20, "#6b4b25", 1, "#d4a56e");
                
                case PropId.PROP_DESK1:
                case PropId.PROP_DESK_ARECIBO1:
                    return SvgAppendRect(appendTo, point, up, orientation, 90, 35, "#666666", 1, "#aaaaaa");

                case PropId.PROP_LABBENCH:
                    return SvgAppendRect(appendTo, point, up, orientation, 150, 60, "#666666", 1, "#aaaaaa");

                case PropId.PROP_BOOK1:
                    return SvgAppendRect(appendTo, point, up, orientation, 16, 6, "#ff0000", 1, "#ff0000");

                case PropId.PROP_CHRVIDEOTAPE:
                case PropId.PROP_CHRDOSSIERRED:
                    return SvgAppendRect(appendTo, point, up, orientation, 16, 6, "#ff0000", 1, "#ff0000");

                case PropId.PROP_CHRSTAFFLIST:
                    return SvgAppendRect(appendTo, point, up, orientation, 16, 16, "#ff0000", 1, "#ff0000");

                case PropId.PROP_CHRCLIPBOARD:
                    return SvgAppendRect(appendTo, point, up, orientation, 16, 16, "#ff0000", 1, "#ff0000");

                case PropId.PROP_DISK_DRIVE1:
                    return SvgAppendRect(appendTo, point, up, orientation, 16, 16, "#ff0000", 1, "#ff0000");

                case PropId.PROP_CHRCIRCUITBOARD:
                    return SvgAppendRect(appendTo, point, up, orientation, 12, 12, "#009900", 1, "#00ff00");

                case PropId.PROP_CHRDATTAPE:
                    return SvgAppendRect(appendTo, point, up, orientation, 16, 6, "#ff0000", 1, "#ff0000");

                case PropId.PROP_BOOKSHELF1:
                    return SvgAppendRect(appendTo, point, up, orientation, 70, 15, "#6b4b25", 1, "#d4a56e");

                case PropId.PROP_SAFEDOOR:
                    return SvgAppendRect(appendTo, point, up, orientation, 20, 8, "#6b4b25", 4, "#d4a56e");

                case PropId.PROP_HATCHSEVX:
                    return SvgAppendRect(appendTo, new Coord2dd(point.X, point.Y), up, orientation, 50, 50, "#6b4b25", 4, "#d4a56e");

                case PropId.PROP_SEVDISH:
                case PropId.PROP_SATDISH:
                    return SvgAppendSatelliteDish(appendTo, point, up, orientation);

                case PropId.PROP_WOOD_SM_CRATE4:
                case PropId.PROP_WOOD_SM_CRATE5:
                case PropId.PROP_WOOD_LG_CRATE1:
                case PropId.PROP_CARD_BOX1:
                case PropId.PROP_CARD_BOX2:
                case PropId.PROP_CARD_BOX3:
                case PropId.PROP_CARD_BOX4_LG:
                case PropId.PROP_CARD_BOX5_LG:
                case PropId.PROP_CARD_BOX6_LG:
                    return SvgAppendRect(appendTo, point, up, orientation, 22, 22, "#6b4b25", 2, "#d4a56e");

                case PropId.PROP_BOXES2X4: // metal crates
                    return SvgAppendRect(appendTo, point, up, orientation, 88, 44, "#666666", 2, "#aaaaaa");

                case PropId.PROP_BOXES3X4: // metal crates
                    return SvgAppendRect(appendTo, point, up, orientation, 88, 66, "#666666", 2, "#aaaaaa");

                case PropId.PROP_BOXES4X4: // metal crates
                    return SvgAppendRect(appendTo, point, up, orientation, 88, 88, "#666666", 2, "#aaaaaa");

                case PropId.PROP_METAL_CRATE1:
                case PropId.PROP_METAL_CRATE3:
                    return SvgAppendRect(appendTo, point, up, orientation, 22, 22, "#666666", 2, "#aaaaaa");
                
                case PropId.PROP_METAL_CHAIR1:
                    return SvgAppendRect(appendTo, point, up, orientation, 16, 16, "#666666", 2, "#aaaaaa");
                
                case PropId.PROP_SEC_PANEL:
                    return SvgAppendRect(appendTo, point, up, orientation, 50, 20, "#666666", 2, "#aaaaaa");

                case PropId.PROP_OIL_DRUM7:
                    return SvgAppendCircle(appendTo, point, 16, "#666666", 2, "#aaaaaa");

                case PropId.PROP_ICBM:
                    return SvgAppendCircle(appendTo, new Coord2dd(point.X + 50, point.Y + 50), 100, "#666666", 2, "#aaaaaa");

                case PropId.PROP_SHUTTLE:
                    return SvgAppendCircle(appendTo, point, 80, "#666666", 2, "#aaaaaa");

                case PropId.PROP_PADLOCK:
                    return SvgAppendPadlock(appendTo, point, up, orientation);

                case PropId.PROP_CHRGRENADELAUNCH:
                    return SvgAppendGrenadeLauncher(appendTo, point, up, orientation);

                case PropId.PROP_CHRSNIPERRIFLE:
                    return SvgAppendSniperRifle(appendTo, point, up, orientation);

                case PropId.PROP_DAMGATEDOOR:
                    return SvgAppendRect(appendTo, point, up, orientation, 20, 100, "#666666", 6, "#aaaaaa");
                
                case PropId.PROP_DAMCHAINDOOR:
                    return SvgAppendRect(appendTo, point, up, orientation, 55, 10, "#666666", 2, "#aaaaaa");

                case PropId.PROP_MAINFRAME1:
                case PropId.PROP_MAINFRAME2:
                case PropId.PROP_DOOR_MF:
                    return SvgAppendRect(appendTo, point, up, orientation, 40, 40, "#666666", 2, "#94dff2");

                case PropId.PROP_CONSOLE_SEVA:
                case PropId.PROP_CONSOLE_SEVB:
                case PropId.PROP_CONSOLE_SEVC:
                case PropId.PROP_CONSOLE_SEVD:
                case PropId.PROP_CONSOLE_SEV2A:
                case PropId.PROP_CONSOLE_SEV2B:
                case PropId.PROP_CONSOLE_SEV2C:
                case PropId.PROP_CONSOLE_SEV2D:
                case PropId.PROP_CONSOLE_SEV_GEA:
                    return SvgAppendRect(appendTo, point, up, orientation, 30, 30, "#666666", 2, "#94dff2");

                case PropId.PROP_KIT_UNITS1:
                    return SvgAppendRect(appendTo, point, up, orientation, 30, 30, "#666666", 2, "#94dff2");

                case PropId.PROP_RADIO_UNIT1:
                case PropId.PROP_RADIO_UNIT2:
                case PropId.PROP_RADIO_UNIT3:
                case PropId.PROP_RADIO_UNIT4:
                    return SvgAppendRect(appendTo, point, up, orientation, 30, 30, "#666666", 2, "#94dff2");

                case PropId.PROP_DOORCONSOLE:
                    return SvgAppendRect(appendTo, point, up, orientation, 30, 30, "#666666", 2, "#94dff2");

                case PropId.PROP_BODYARMOUR:
                case PropId.PROP_BODYARMOURVEST:
                    return SvgAppendRect(appendTo, point, up, orientation, 20, 20, "#666666", 2, "#557db5");

                case PropId.PROP_GUN_RUNWAY1:
                case PropId.PROP_ROOFGUN:
                case PropId.PROP_GROUNDGUN:
                    return SvgAppendHeavyGun(appendTo, point, up, orientation);

                case PropId.PROP_GASBARRELS:
                    return SvgAppendRect(appendTo, point, up, orientation, 30, 30, "#666666", 2, "#ffffff");

                case PropId.PROP_DEST_SEAWOLF:
                    return SvgAppendRect(appendTo, point, up, orientation, 60, 60, "#827a3b", 2, "#c9c079");

                case PropId.PROP_TUNING_CONSOLE1:
                    return SvgAppendRect(appendTo, point, up, orientation, 80, 30, "#666666", 2, "#aaaaaa");

                case PropId.PROP_BRIDGE_CONSOLE1B:
                case PropId.PROP_BRIDGE_CONSOLE2B:
                    return SvgAppendRect(appendTo, point, up, orientation, 80, 30, "#666666", 2, "#aaaaaa");

                case PropId.PROP_TV1:
                    return SvgAppendRect(appendTo, point, up, orientation, 16, 16, "#666666", 2, "#aaaaaa");

                case PropId.PROP_DEST_ENGINE:
                    return SvgAppendRect(appendTo, point, up, orientation, 100, 100, "#666666", 4, "#aaaaaa");

                case PropId.PROP_SAT1_REFLECT:
                    return SvgAppendRect(appendTo, point, up, orientation, 120, 80, "#666666", 2, "#0000ff");

                case PropId.PROP_PLANE:
                    return SvgAppendPlane(appendTo, point, up, orientation);

                case PropId.PROP_TANK:
                    return SvgAppendTank(appendTo, point, up, orientation);

                case PropId.PROP_TIGER:
                case PropId.PROP_MILCOPTER:
                    return SvgAppendHelicopter(appendTo, point, up, orientation, scale);
                
                case PropId.PROP_HELICOPTER: // cradle
                    scale = 0.4;
                    return SvgAppendHelicopter(appendTo, point, up, orientation, scale);

                case PropId.PROP_ST_PETE_ROOM_1I:
                case PropId.PROP_ST_PETE_ROOM_2I:
                case PropId.PROP_ST_PETE_ROOM_3T:
                case PropId.PROP_ST_PETE_ROOM_5C:
                case PropId.PROP_ST_PETE_ROOM_6C:
                    return SvgAppendRect(appendTo, point, up, orientation, 600, 600, "#666666", 2, "#cccccc");

                case PropId.PROP_CARGOLF:
                case PropId.PROP_CARWEIRD:
                    return SvgAppendRect(appendTo, point, up, orientation, 120, 90, "#916b2a", 2, "#ffdfa8");

                case PropId.PROP_BARRICADE:
                    return SvgAppendRect(appendTo, point, up, orientation, 120, 6, "#916b2a", 2, "#ffff00");

                case PropId.PROP_LANDMINE:
                    return SvgAppendRect(appendTo, point, up, orientation, 20, 20, "#1e4a1b", 2, "#2e8c27");

                case PropId.PROP_HATCHDOOR: // train floor door
                    return SvgAppendRect(appendTo, point, up, orientation, 20, 20, "#888888", 2, "#eeeeee");

                case PropId.PROP_PLANT1:
                case PropId.PROP_PLANT11:
                case PropId.PROP_PLANT2:
                case PropId.PROP_PLANT2B:
                case PropId.PROP_PLANT3:
                    return SvgAppendRect(appendTo, point, up, orientation, 30, 30, "#1e4a1b", 2, "#4cf540");

                case PropId.PROP_JUNGLE3_TREE:
                case PropId.PROP_PALM:
                case PropId.PROP_PALMTREE:
                    return SvgAppendRect(appendTo, point, up, orientation, 60, 60, "#1e4a1b", 2, "#4cf540");

                case PropId.PROP_DOOR_AZT_DESK:
                    return SvgAppendRect(appendTo, point, up, orientation, 60, 60, "#666666", 2, "#aaaaaa");

                case PropId.PROP_DOOR_AZT_DESK_TOP:
                    return SvgAppendRect(appendTo, point, up, orientation, 40, 40, "#666666", 2, "#aaaaaa");

                case PropId.PROP_DOOR_AZT_CHAIR:
                    return SvgAppendRect(appendTo, point, up, orientation, 20, 20, "#666666", 2, "#aaaaaa");

                // no particular svg to draw
                case PropId.PROP_DESK_LAMP2:
                case PropId.PROP_MOTORBIKE:
                case PropId.PROP_CARBMW:
                case PropId.PROP_STOOL1:
                case PropId.PROP_JERRY_CAN1:
                case PropId.PROP_KEYBOARD1:
                case PropId.PROP_CHRMAP:
                case PropId.PROP_DAMTUNDOOR:
                case PropId.PROP_SWIVEL_CHAIR1:
                case PropId.PROP_ICBM_NOSE: //fallthrough
                case PropId.PROP_DEST_GUN: //fallthrough
                case PropId.PROP_DEST_EXOCET: //fallthrough
                case PropId.PROP_HATCHBOLT: //fallthrough
                case PropId.PROP_KEY_HOLDER: //fallthrough
                case PropId.PROP_CHRTHROWKNIFE: //fallthrough
                case PropId.PROP_CHRWPPKSIL: //fallthrough
                case PropId.PROP_CHRWPPK: //fallthrough
                case PropId.PROP_CHRBLUEPRINTS: //fallthrough
                case PropId.PROP_BRAKEUNIT: //fallthrough

                case PropId.PROP_SHUTTLE_DOOR_L: //fallthrough
                case PropId.PROP_SHUTTLE_DOOR_R: //fallthrough
                case PropId.PROP_DISC_READER: //fallthrough

                case PropId.PROP_CHRKALASH: // ak-47, used on Depot //fallthrough
                case PropId.PROP_CHRMP5K: // used on Depot //fallthrough
                case PropId.PROP_CHRROCKETLAUNCH: // used on Depot //fallthrough
                
                case PropId.PROP_CHRGOLDEN: // fallthrough
                case PropId.PROP_MODEMBOX: // fallthrough

                // on facility, this is the light to show when the double gated doors are available (red/green)
                case PropId.PROP_TVSCREEN:
                    return SvgAppendRect(appendTo, point, up, orientation, 8, 8, "#916b2a", 1, "#ffdfa8");

                // ignore these
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
                // other
                case PropId.PROP_CHRDOORDECODER:
                case PropId.PROP_CHRPLASTIQUE:
                case PropId.PROP_CHRBLACKBOX:
                case PropId.PROP_CHRGRENADE:
                case PropId.PROP_CHRM16:
                    return null;
            }

            //return null;
            throw new NotSupportedException($"{nameof(SetupObjectToSvgAppend)}: could not resolve PROPDEF {setupObject.Type}, PROP {(PropId)baseObject.ObjectId} to svg.");
        }

        private static SvgGroup SvgAppendPropDefaultModelBbox_door(SvgGroup group, PropPosition pp, double levelScale, string stroke, double strokeWidth, string fill)
        {
            var scaleUpper = (pp.SetupObject.Scale & 0xff00) >> 8;
            var scaleLower = pp.SetupObject.Scale & 0xff;
            double setupScale = scaleUpper + ((double)scaleLower / (double)256);

            if (object.ReferenceEquals(null, pp.Bbox))
            {
                throw new NullReferenceException($"{nameof(pp.Bbox)} is null, doors require 3d bounding box");
            }

            // The game does some weird translation.
            var bb2 = new BoundingBoxd();
            bb2.MaxZ = pp.Bbox.MinX;
            bb2.MinZ = pp.Bbox.MaxX;
            bb2.MaxY = pp.Bbox.MinY;
            bb2.MinY = pp.Bbox.MaxY;
            bb2.MaxX = pp.Bbox.MinZ;
            bb2.MinX = pp.Bbox.MaxZ;

            var modelData = Getools.Lib.Game.Asset.Model.ModelDataResolver.GetModelDataFromPropId(pp.Prop);

            // The axes don't match, but this is what the game does.
            double xscale = (bb2.MinY - bb2.MaxY) / (modelData.BboxMaxX - modelData.BboxMinX);
            double yscale = (bb2.MinX - bb2.MaxX) / (modelData.BboxMaxY - modelData.BboxMinY);
            double zscale = (bb2.MinZ - bb2.MaxZ) / (modelData.BboxMaxZ - modelData.BboxMinZ);

            if ((xscale <= 0.000001f) || (yscale <= 0.000001f) || (zscale <= 0.000001f))
            {
                xscale =
                    yscale =
                    zscale = 1.0f;
            }

            double modelScale = xscale;

            if (modelScale < yscale)
            {
                modelScale = yscale;
            }

            if (modelScale < zscale)
            {
                modelScale = zscale;
            }

            var pos = Getools.Lib.Math.Pad.GetCenter(pp.Origin, pp.Up, pp.Look, pp.Bbox).Scale(1.0 / levelScale);

            // if (!(pp.SetupObject->flags & 0x1000)) // prop flag PROPFLAG_00001000 "Absolute Position"
            //

            double modelSizeX = modelData.BboxMaxX - modelData.BboxMinX;
            double modelSizeY = modelData.BboxMaxY - modelData.BboxMinY;
            double modelSizeZ = modelData.BboxMaxZ - modelData.BboxMinZ;

            modelSizeX *= setupScale * xscale / levelScale;
            modelSizeY *= setupScale * yscale / levelScale;
            modelSizeZ *= setupScale * zscale / levelScale;

            double halfw = modelSizeX / 2;
            double halfh = modelSizeZ / 2;

            double rotAngleRad = System.Math.Atan2(pp.Up.Z, pp.Up.X);
            rotAngleRad *= -1;

            if (pp.Look.Y > 0.9)
            {
                rotAngleRad = System.Math.Atan2(pp.Up.X, pp.Up.Z);
                rotAngleRad *= -1;
                rotAngleRad += System.Math.PI / 2;
            }

            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            container.Transform = $"translate({pos.X - halfw}, {pos.Z - halfh})";

            if (rotAngle != 0)
            {
                container.Transform += $" rotate({rotAngle} {halfw} {halfh})";
            }

            var rect = container.AddRect();

            rect.X = 0;
            rect.Y = 0;
            rect.Width = modelSizeX;
            rect.Height = modelSizeZ;
            rect.Stroke = stroke;
            rect.StrokeWidth = strokeWidth;
            rect.Fill = fill;

            return container;
        }

        private static SvgGroup SvgAppendPropDefaultModelBbox_prop(SvgGroup group, PropPosition pp, double levelScale, string stroke, double strokeWidth, string fill)
        {
            if (object.ReferenceEquals(null, pp.SetupObject))
            {
                throw new NullReferenceException($"{nameof(pp.SetupObject)} is null");
            }

            // guard does not implement SetupObjectGenericBase
            var standardObject = (SetupObjectGenericBase)pp.SetupObject;

            var scaleUpper = (pp.SetupObject.Scale & 0xff00) >> 8;
            var scaleLower = pp.SetupObject.Scale & 0xff;
            double setupScale = scaleUpper + ((double)scaleLower / (double)256);

            bool boundToPad3dDimensions = false;
            bool has3dBoundBox = !object.ReferenceEquals(null, pp.Bbox);

            double modelSizeX = 0;
            double modelSizeY = 0;
            double modelSizeZ = 0;

            var modelData = Getools.Lib.Game.Asset.Model.ModelDataResolver.GetModelDataFromPropId(pp.Prop);
            double modelScale = 1.0;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            //if ((standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag1_AbsolutePosition) > 0)
            if (has3dBoundBox)
            {
                pos = Getools.Lib.Math.Pad.GetCenter(pp.Origin, pp.Up, pp.Look, pp.Bbox!).Scale(1.0 / levelScale);
            }

            if (boundToPad3dDimensions)
            {
                modelSizeX = pp.Bbox!.MaxX - pp.Bbox.MinX;
                modelSizeY = pp.Bbox!.MaxY - pp.Bbox.MinY;
                modelSizeZ = pp.Bbox!.MaxZ - pp.Bbox.MinZ;

                // The game does some weird translation.
                var bb2 = new BoundingBoxd();
                bb2.MaxZ = pp.Bbox!.MinX;
                bb2.MinZ = pp.Bbox.MaxX;
                bb2.MaxY = pp.Bbox.MinY;
                bb2.MinY = pp.Bbox.MaxY;
                bb2.MaxX = pp.Bbox.MinZ;
                bb2.MinX = pp.Bbox.MaxZ;

                // The axes don't match, but this is what the game does.
                double xscale = (bb2.MinY - bb2.MaxY) / (modelData.BboxMaxX - modelData.BboxMinX);
                double yscale = (bb2.MinX - bb2.MaxX) / (modelData.BboxMaxY - modelData.BboxMinY);
                double zscale = (bb2.MinZ - bb2.MaxZ) / (modelData.BboxMaxZ - modelData.BboxMinZ);

                if ((xscale <= 0.000001f) || (yscale <= 0.000001f) || (zscale <= 0.000001f))
                {
                    xscale =
                        yscale =
                        zscale = 1.0f;
                }

                modelScale = xscale;

                if (modelScale < yscale)
                {
                    modelScale = yscale;
                }

                if (modelScale < zscale)
                {
                    modelScale = zscale;
                }

                pos = Getools.Lib.Math.Pad.GetCenter(pp.Origin, pp.Up, pp.Look, pp.Bbox);

                // if (!(pp.SetupObject->flags & 0x1000)) // prop flag PROPFLAG_00001000 "Absolute Position"
                //

                modelSizeX *= setupScale * xscale;
                modelSizeY *= setupScale * yscale;
                modelSizeZ *= setupScale * zscale;
            }
            else
            {
                modelScale = Getools.Lib.Game.Asset.Model.ModelDataResolver.GetModelScaleFromPropId(pp.Prop);

                if (has3dBoundBox && (standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag1_XToPresetBounds) > 0)
                {
                    modelSizeX = pp.Bbox!.MaxX - pp.Bbox.MinX;
                    modelSizeX *= setupScale / levelScale;
                }
                else
                {
                    modelSizeX = modelData.BboxMaxX - modelData.BboxMinX;
                    modelSizeX *= setupScale * modelScale;
                }

                if (has3dBoundBox && (standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag1_YToPresetBounds) > 0)
                {
                    modelSizeY = pp.Bbox!.MaxY - pp.Bbox.MinY;
                    modelSizeY *= setupScale / levelScale;
                }
                else
                {
                    modelSizeY = modelData.BboxMaxY - modelData.BboxMinY;
                    modelSizeY *= setupScale * modelScale;
                }

                if (has3dBoundBox && (standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag1_ZToPresetBounds) > 0)
                {
                    modelSizeZ = pp.Bbox!.MaxZ - pp.Bbox.MinZ;
                    modelSizeZ *= setupScale / levelScale;
                }
                else
                {
                    modelSizeZ = modelData.BboxMaxZ - modelData.BboxMinZ;
                    modelSizeZ *= setupScale * modelScale;
                }
            }

            double halfw = modelSizeX / 2;
            double halfh = modelSizeZ / 2;

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            container.Transform = $"translate({pos.X - halfw}, {pos.Z - halfh})";

            if (rotAngle != 0)
            {
                container.Transform += $" rotate({rotAngle} {halfw} {halfh})";
            }

            var rect = container.AddRect();

            rect.X = 0;
            rect.Y = 0;
            rect.Width = modelSizeX;
            rect.Height = modelSizeZ;
            rect.Stroke = stroke;
            rect.StrokeWidth = strokeWidth;
            rect.Fill = fill;

            return container;
        }

        private static SvgGroup SvgGroupAppendAlarm(SvgGroup group, Coord2dd point)
        {
            var alarmContainer = group.AddGroup();

            alarmContainer.AddClass("svg-logical-item");

            var rect = alarmContainer.AddRect();
            rect.X = point.X;
            rect.Y = point.Y;
            rect.Width = 36;
            rect.Height = 60;
            rect.Stroke = "#666666";
            rect.StrokeWidth = 1;
            rect.Fill = "#eeeeee";

            var bell = alarmContainer.AddCircle();
            bell.CX = point.X + 18;
            bell.CY = point.Y + 20;
            bell.R = 14;
            bell.Stroke = "#333333";
            bell.StrokeWidth = 2;
            bell.Fill = "red";

            return alarmContainer;
        }

        private static SvgGroup SvgGroupAppendKey(SvgGroup group, Coord2dd point)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var path = container.AddPath();

            path.Fill = "#cca800";
            path.Stroke = "#906a00";
            path.StrokeWidth = 2;

            path.D = "m 0.43595022,7.014382 c -0.30029153,5.40526 4.65446948,6.606259 4.65446948,6.606259 l 1.0508961,33.933061 4.5044672,-0.300226 -0.450449,-3.002907 4.80469,0.450449 1.351343,-3.303129 h -6.156033 v -4.354245 l 5.255139,0.750671 1.201115,-5.705583 -6.156028,0.300225 -0.300226,-18.618095 c 0,0 4.129114,-0.488029 4.542022,-6.9818504 C 14.428092,-2.0699217 0.57338466,-1.3909436 0.43595022,7.014382 Z M 7.7861017,2.5626133 c 2.5377613,-3.68e-5 4.5950313,2.057233 4.5949933,4.5949926 3.8e-5,2.5377596 -2.057232,4.5950291 -4.5949933,4.5949921 C 5.2484276,11.752514 3.1912911,9.6952796 3.1913279,7.1576059 3.1912911,4.6199322 5.2484276,2.5626979 7.7861017,2.5626133 Z";

            path.Transform = $"translate({point.X}, {point.Y})";

            return container;
        }

        private static SvgGroup SvgGroupAppendCctv(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the cctv svg.
            container.Transform = $"translate({point.X - 10}, {point.Y}) rotate({rotAngle} 45 30)";

            SvgPath path = null;

            //

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "m 66.842669,47.084191 v 6.569513 H 45.49174 v -6.569513 z";

            //

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "M 34.862376,18.053258 11.514484,15.189837 11.001787,27.375645 1.1475142,15.878994 V 9.3094783 L 33.100271,14.969574 Z";

            //

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "m 11.001787,10.951858 22.993304,4.927136 v 26.27806 L 11.001787,37.229918 Z";

            //

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "M 33.995091,42.157054 75.054559,24.090887 V 15.878994 L 33.995091,27.375645 Z";

            //

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "m 45.49174,37.229918 v 9.854273 h 6.56952 V 34.165424 Z";

            //

            path = container.AddPath();

            path.Fill = "#ffffff";
            path.Stroke = "#838383";
            path.StrokeWidth = 0.94691;

            path.D = "m 13.857191,14.310917 17.230768,3.824947 V 38.535596 L 13.857191,34.710651 Z";

            //

            var ellipse = container.AddEllipse();

            ellipse.Fill = "#fefdff";
            ellipse.Stroke = "#000000";
            ellipse.StrokeWidth = 1.24148;

            ellipse.CX = 22.527639;
            ellipse.CY = 26.423256;
            ellipse.RX = 5.396447;
            ellipse.RY = 6.3876319;

            //

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "M 33.995091,27.375645 27.373429,13.647994 1.1475142,9.3094783 60.273151,1.0975849 h 14.781408 l 3.284759,1.6423788 V 14.236615 l -3.284759,1.642379 z";

            return container;
        }

        private static SvgGroup SvgGroupAppendPc(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the cctv svg.
            container.Transform = $"translate({point.X - 5}, {point.Y}) rotate({rotAngle} 22 18)";

            SvgRect rect = null;

            //

            rect = container.AddRect();

            rect.Fill = "#b8b8b8";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 1.42494;

            rect.Width = 24.99893;
            rect.Height = 24.551754;
            rect.X = 10.50208;
            rect.Y = 1.8415037;

            //

            rect = container.AddRect();

            rect.Fill = "#9ddcff";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.420072;

            rect.Width = 20.906588;
            rect.Height = 15.832173;
            rect.X = 12.622778;
            rect.Y = 4.7739959;

            //

            rect = container.AddRect();

            rect.Fill = "#b8b8b8";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.420072;

            rect.Width = 42.312958;
            rect.Height = 8.9240217;
            rect.X = 2.3667684;
            rect.Y = 30.225342;

            //

            return container;
        }
       
        private static SvgGroup SvgAppendSatelliteDish(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({point.X}, {point.Y}) rotate({rotAngle} 16 16)";

            SvgPath path = null;

            //

            path = container.AddPath();

            path.Fill = "#595959";
            path.Stroke = "#000000";
            path.StrokeWidth = 0.310732;

            path.D = "M 17.54312,19.373808 13.6285,35.086467 h 7.528115 z";

            //

            path = container.AddPath();

            path.Fill = "#b8b8b8";
            path.Stroke = "#000000";
            path.StrokeWidth = 0.381342;

            path.D = "M 25.96118,21.714244 C 22.092571,27.397982 10.076827,29.242102 5.7462675,23.998513 3.5126855,21.294015 5.5689855,13.312264 7.2859505,10.910606 9.3967465,7.9580653 17.34594,0.78223698 20.671523,2.070418 c 6.341526,2.45642 9.158266,13.960089 5.289657,19.643826 z";

            //

            var ellipse = container.AddEllipse();

            ellipse.Fill = "#f0f0f0";
            ellipse.Stroke = "#000000";
            ellipse.StrokeWidth = 0.377576;

            ellipse.CX = 18.253674;
            ellipse.CY = 3.342999;
            ellipse.RX = 8.9326878;
            ellipse.RY = 13.262773;
            ellipse.Transform = "rotate(34.240976)";

            //

            path = container.AddPath();

            path.Fill = "#b8b8b8";
            path.Stroke = "#000000";
            path.StrokeWidth = 0.499999;

            path.D = "M 56.419355,91.548388 43.751728,80.055593 31.084102,68.562799 47.370968,63.338709 63.657832,58.11462 60.038594,74.831505 Z";
            path.Transform = "matrix(0.53729784,0.31334376,-0.00994123,0.24445983,-15.425907,-22.369103)";

            //

            return container;
        }

        private static SvgGroup SvgAppendSafe(SvgGroup group, Coord2dd point)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var safe = container.AddRect();

            safe.X = point.X;
            safe.Y = point.Y;
            safe.Width = 24;
            safe.Height = 24;
            safe.Stroke = "#000000";
            safe.StrokeWidth = 1;
            safe.Fill = "#7a7a7a";

            return container;
        }

        private static SvgGroup SvgAppendRect(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation, double width, double height, string stroke, double strokeWidth, string fill)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            double rotAngle;
            rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;
            rotAngle *= -1;

            var halfw = width / 2;
            var halfh = height / 2;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({point.X - halfw}, {point.Y - halfh})";
            if (rotAngle != 0)
            {
                container.Transform += $"rotate({rotAngle} {halfw} {halfh})";
            }

            var rect = container.AddRect();

            rect.X = 0;
            rect.Y = 0;
            rect.Width = width;
            rect.Height = height;
            rect.Stroke = stroke;
            rect.StrokeWidth = strokeWidth;
            rect.Fill = fill;

            return container;
        }

        private static SvgGroup SvgAppendCircle(SvgGroup group, Coord2dd point, double r, string stroke, double strokeWidth, string fill)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var halfr = r / 2;

            container.Transform = $"translate({point.X - halfr}, {point.Y - halfr})";

            var circle = container.AddCircle();

            circle.CX = 0;
            circle.CY = 0;
            circle.R = r;

            circle.Stroke = stroke;
            circle.StrokeWidth = strokeWidth;
            circle.Fill = fill;

            return container;
        }

        private static SvgGroup SvgGroupAppendChr(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;
            var upAngle = System.Math.Atan2(up.X, up.Z) * 180 / System.Math.PI;

            rotAngle += upAngle;

            // magic
            //if (up.Y < 0)
            {
                rotAngle *= -1;
                rotAngle += 180;
            }

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({point.X}, {point.Y}) rotate({rotAngle} 8 6)";

            SvgRect rect = null;

            //

            rect = container.AddRect();

            rect.Fill = "#c8ab37";
            rect.Stroke = "#aa4400";
            rect.StrokeWidth = 0.569023;

            rect.Width = 37.525127;
            rect.Height = 15.142623;
            rect.X = 0.28451145;
            rect.Y = 5.3028398;

            //

            var ellipse = container.AddEllipse();

            ellipse.Fill = "#ffdd55";
            ellipse.Stroke = "#aa4400";
            ellipse.StrokeWidth = 0.876555;

            ellipse.CX = 18.947899;
            ellipse.CY = 12.724317;
            ellipse.RX = 11.806371;
            ellipse.RY = 12.027679;

            //

            var path = container.AddPath(); 

            path.Fill = "#ffeeaa";
            path.Stroke = "#aa4400";
            path.StrokeWidth = 0.701189;

            path.D = "M 24.591874,12.183001 H 13.453438 L 19.02266,0.97511731 Z";

            //

            return container;
        }

        private static SvgGroup SvgAppendPadlock(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({point.X - 8}, {point.Y - 18}) rotate({rotAngle} 8 18)";

            SvgRect rect = null;

            //

            rect = container.AddRect();

            rect.Fill = "#b3b3b3";
            rect.Stroke = "#4d4d4d";
            rect.StrokeWidth = 0.349545;

            rect.Width = 13.661667;
            rect.Height = 16.166445;
            rect.X = 0.44301221;
            rect.Y = 10.843964;

            //

            var path = container.AddPath(); 

            path.Fill = "#b3b3b3";
            path.Stroke = "#4d4d4d";
            path.StrokeWidth = 1.25657;

            path.D = "M 7.2412021,0.69852204 C 1.0085037,0.68070391 0.68529965,5.2915851 0.68481911,10.909995 L 13.865147,10.801897 C 13.826836,5.1838055 13.473901,0.71634014 7.2412021,0.69852204 Z M 7.4564339,3.2948227 c 3.7810651,0.013199 3.9947521,3.3229823 4.0179921,7.4858393 l -7.9954469,0.08011 C 3.4792705,6.6976769 3.6753692,3.28162 7.4564339,3.2948227 Z";

            //

            return container;
        }

        private static SvgGroup SvgAppendGrenadeLauncher(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({point.X - 17}, {point.Y - 7}) rotate({rotAngle} 17 7)";

            SvgRect rect = null;

            //

            rect = container.AddRect();

            rect.Fill = "#333333";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.384321;

            rect.Width = 33.90937;
            rect.Height = 4.803947;
            rect.X = 0.85984176;
            rect.Y = 3.118021;

            //

            rect = container.AddRect();

            rect.Fill = "#808080";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.421;

            rect.Width = 9.7854452;
            rect.Height = 11.541807;
            rect.X = 13.674533;
            rect.Y = 0.50181764;

            //

            rect = container.AddRect();

            rect.Fill = "#a05a2c";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.421;

            rect.Width = 2.8854518;
            rect.Height = 9.4090824;
            rect.X = 0.75272673;
            rect.Y = 3.0109062;

            return container;
        }

        private static SvgGroup SvgAppendSniperRifle(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({point.X - 17}, {point.Y - 7}) rotate({rotAngle} 17 7)";

            SvgRect rect = null;

            //

            rect = container.AddRect();

            rect.Fill = "#a05a2c";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.421;

            rect.Width = 6.7419353;
            rect.Height = 12.419354;
            rect.X = 1.0356053;
            rect.Y = 6.142941;

            //

            rect = container.AddRect();

            rect.Fill = "#a05a2c";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.322107;

            rect.Width = 4.0199499;
            rect.Height = 8.5073996;
            rect.X = 11.986159;
            rect.Y = 8.4556026;

            //

            rect = container.AddRect();

            rect.Fill = "#ffffff";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.421;

            rect.Width = 15.967742;
            rect.Height = 2.8387096;
            rect.X = 34.53373;
            rect.Y = 6.734549;

            //

            rect = container.AddRect();

            rect.Fill = "#a05a2c";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.337451;

            rect.Width = 38.406128;
            rect.Height = 5.004549;
            rect.X = 0.51353806;
            rect.Y = 5.5458531;

            //

            rect = container.AddRect();

            rect.Fill = "#ffffff";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.421;

            rect.Width = 17.032257;
            rect.Height = 6.3870969;
            rect.X = 47.429684;
            rect.Y = 4.7346644;

            //

            rect = container.AddRect();

            rect.Fill = "#808080";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.345472;

            rect.Width = 15.236415;
            rect.Height = 3.9787538;
            rect.X = 11.919128;
            rect.Y = 0.64669603;

            return container;
        }

        private static SvgGroup SvgAppendHeavyGun(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({point.X - 30}, {point.Y - 32}) rotate({rotAngle} 30 32)";

            SvgRect rect = null;

            //

            var circle = container.AddCircle();

            circle.Fill = "#94dff2";
            circle.Stroke = "#000000";
            circle.StrokeWidth = 0.534929;

            circle.CY = 30.219355;
            circle.CX = 30.219355;
            circle.R = 29.951891;

            //

            rect = container.AddRect();

            rect.Fill = "#158fae";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.699793;

            rect.Width = 8.3660135;
            rect.Height = 44.629238;
            rect.X = 26.036348;
            rect.Y = 30.569252;

            return container;
        }

        private static SvgGroup SvgAppendPlane(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({point.X - 63}, {point.Y - 73}) rotate({rotAngle} 63 73)";

            SvgRect rect = null;

            //

            var path = container.AddPath();

            path.Fill = "#ececec";
            path.Stroke = "#000000";
            path.StrokeWidth = 1.23946;

            path.D = "M 47.480847,0.61972968 H 77.482135 L 68.72964,155.5836 H 56.233342 Z";

            //

            rect = container.AddRect();

            rect.Fill = "#ececec";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 1.14123;

            rect.Width = 123.82175;
            rect.Height = 23.851362;
            rect.X = 0.57061696;
            rect.Y = 31.81136;

            //

            rect = container.AddRect();

            rect.Fill = "#ececec";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 1.13162;

            rect.Width = 61.349876;
            rect.Height = 17.348799;
            rect.X = 31.806553;
            rect.Y = 125.52879;

            //

            rect = container.AddRect();

            rect.Fill = "#d40000";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.622655;

            rect.Width = 11.873645;
            rect.Height = 24.369942;
            rect.X = 112.49959;
            rect.Y = 31.552073;

            //

            rect = container.AddRect();

            rect.Fill = "#d40000";
            rect.Stroke = "#000000";
            rect.StrokeWidth = 0.622655;

            rect.Width = 11.873645;
            rect.Height = 24.369942;
            rect.X = 0.31132731;
            rect.Y = 31.552073;

            return container;
        }

        private static SvgGroup SvgAppendTank(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({point.X - 55}, {point.Y - 123}) rotate({rotAngle} 55 123)";

            SvgRect rect = null;

            //

            rect = container.AddRect();

            rect.Fill = "#006000";
            rect.Stroke = "#1aa12e";
            rect.StrokeWidth = 4.58041;

            rect.Width = 102.84116;
            rect.Height = 148.87898;
            rect.X = 2.2902074;
            rect.Y = 48.328026;

            //

            rect = container.AddRect();

            rect.Fill = "#006000";
            rect.Stroke = "#1aa12e";
            rect.StrokeWidth = 4.47882;

            rect.Width = 13.936307;
            rect.Height = 87.596825;
            rect.X = 46.742638;
            rect.Y = 2.2394102;

            //

            var ellipse = container.AddEllipse();

            ellipse.Fill = "#006000";
            ellipse.Stroke = "#1aa12e";
            ellipse.StrokeWidth = 4.23444;

            ellipse.CX = 53.710789;
            ellipse.CY = 122.76752;
            ellipse.RX = 28.574659;
            ellipse.RY = 28.574663;

            return container;
        }

        private static SvgGroup SvgAppendHelicopter(SvgGroup group, Coord2dd point, Coord3dd up, Coord3dd orientation, double scale)
        {
            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var rotAngle = System.Math.Atan2(orientation.X, orientation.Z) * 180 / System.Math.PI;

            double halfw = 75;
            double halfh = 75;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({point.X - halfh}, {point.Y - halfw}) rotate({rotAngle} {halfw} {halfh}) scale({scale})";

            SvgRect rect = null;

            //

            var ellipse = container.AddEllipse();

            ellipse.Fill = "#b3b3b3";
            ellipse.Stroke = "#000000";
            ellipse.StrokeWidth = 3.14351;

            ellipse.CX = 73.629028;
            ellipse.CY = 129.87097;
            ellipse.RX = 11.815351;
            ellipse.RY = 84.912117;

            //

            ellipse = container.AddEllipse();

            ellipse.Fill = "#b3b3b3";
            ellipse.Stroke = "#000000";
            ellipse.StrokeWidth = 4.14933;

            ellipse.CX = 73.451607;
            ellipse.CY = 74.693535;
            ellipse.RX = 31.005981;
            ellipse.RY = 56.376945;

            //

            var path = container.AddPath();

            path.Fill = "#b3b3b3";
            path.Stroke = "#000000";
            path.StrokeWidth = 3;

            path.D = "M 73.451613,63.870966 144.41935,4.967742 80.903224,69.548387 151.16129,139.80645 75.580644,77.354836 3.9032259,139.45161 64.580645,71.677418 2.8387095,4.967742 Z";

            return container;
        }
    }
}
