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
using Getools.Lib.Game.Engine;
using Getools.Lib.Game.Enums;
using Getools.Lib.Math;
using Getools.Palantir.Render;
using Newtonsoft.Json.Serialization;
using SvgLib;
using static System.Formats.Asn1.AsnWriter;

namespace Getools.Palantir.SvgAppend
{
    /// <summary>
    /// Helper class to draw props onto SVG.
    /// </summary>
    internal static class PropToSvg
    {
        // three decimal places
        private const string StandardDoubleToStringFormat = "0.###";

        /// <summary>
        /// Main entry method to append prop to SVG.
        /// </summary>
        /// <param name="appendTo">Base SVG container to add item to.</param>
        /// <param name="rp">Object information for item to be added to SVG.</param>
        /// <param name="levelScale">Stage scale factor.</param>
        /// <returns>New item that was appended.</returns>
        internal static SvgContainer? SetupObjectToSvgAppend(SvgGroup appendTo, PointPosition rp, double levelScale)
        {
            if (object.ReferenceEquals(null, rp))
            {
                throw new NullReferenceException($"{nameof(rp)}");
            }

            if (rp is PropPointPosition)
            {
                var pp = (PropPointPosition)rp;

                if (object.ReferenceEquals(null, pp.SetupObject))
                {
                    throw new NullReferenceException($"{nameof(pp.SetupObject)}");
                }

                // guard does not implement SetupObjectGenericBase
                SetupObjectGenericBase? baseObject = pp.SetupObject as SetupObjectGenericBase;

                switch (pp.SetupObject.Type)
                {
                    case PropDef.Door:
                        return SvgAppendPropDefaultModelBbox_door(appendTo, pp, levelScale, "#8d968e", 4, "#e1ffdb");

                    case PropDef.Guard:
                        return SvgAppendPropDefaultModelBbox_chr(appendTo, pp, levelScale);

                    case PropDef.AmmoBox:
                        return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale, "#274d23", 4, "#66ed58");

                    case PropDef.Alarm:
                        return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale / 3, "#cccccc", 4, "#ff0000");

                    case PropDef.Armour:
                        return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale, "#0c1c63", 4, "#0000ff");

                    case PropDef.Key:
                        return SvgGroupAppendKey(appendTo, pp, levelScale);

                    case PropDef.Cctv:
                        return SvgGroupAppendCctv(appendTo, pp, levelScale);

                    case PropDef.Aircraft:
                        return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale, "#808018", 4, "#dbdb60");

                    case PropDef.Collectable:
                    case PropDef.Drone:
                    case PropDef.Safe:
                    case PropDef.SingleMonitor:
                    case PropDef.StandardProp:
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
                                return null;

                            case PropId.PROP_PADLOCK:
                                return SvgAppendPadlock(appendTo, pp, levelScale);

                            case PropId.PROP_GUN_RUNWAY1:
                            case PropId.PROP_ROOFGUN:
                            case PropId.PROP_GROUNDGUN:
                                return SvgAppendHeavyGun(appendTo, pp, levelScale);

                            case PropId.PROP_CHRGOLDENEYEKEY:
                                return SvgGroupAppendKey(appendTo, pp, levelScale);

                            case PropId.PROP_MAINFRAME1:
                            case PropId.PROP_MAINFRAME2:
                            case PropId.PROP_DOOR_MF:
                                return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale, "#666666", 4, "#94dff2");

                            case PropId.PROP_AMMO_CRATE1:
                            case PropId.PROP_AMMO_CRATE2:
                            case PropId.PROP_AMMO_CRATE3:
                            case PropId.PROP_AMMO_CRATE4:
                            case PropId.PROP_AMMO_CRATE5:
                                return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale, "#274d23", 4, "#66ed58");

                            case PropId.PROP_CHRCIRCUITBOARD:
                                return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale, "#009900", 4, "#00ff00");

                            case PropId.PROP_CHRVIDEOTAPE:
                            case PropId.PROP_CHRDOSSIERRED:
                            case PropId.PROP_CHRSTAFFLIST:
                            case PropId.PROP_CHRCLIPBOARD:
                            case PropId.PROP_DISK_DRIVE1:
                            case PropId.PROP_CHRDATTAPE:
                                return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale, "#ff0000", 4, "#ff0000");

                            case PropId.PROP_TIGER:
                            case PropId.PROP_MILCOPTER:
                            case PropId.PROP_HELICOPTER:
                                return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale, "#808018", 4, "#dbdb60");

                            default:
                                return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale, "#916b2a", 4, "#ffdfa8");
                        }

                    case PropDef.Tank:
                        return SvgAppendPropDefaultModelBbox_prop(appendTo, pp, levelScale, "#255c25", 4, "#00ff00");
                }

                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Helper function to draw door in the usual manner. The doors bounding box is used.
        /// </summary>
        /// <param name="group">Base SVG container to add item to.</param>
        /// <param name="pp">Object information for item to be added to SVG.</param>
        /// <param name="levelScale">Stage scale factor.</param>
        /// <param name="stroke">Border edge color.</param>
        /// <param name="strokeWidth">Border edge thickness.</param>
        /// <param name="fill">Fill color.</param>
        /// <returns>New item that was appended.</returns>
        private static SvgGroup SvgAppendPropDefaultModelBbox_door(SvgGroup group, PropPointPosition pp, double levelScale, string stroke, double strokeWidth, string fill)
        {
            if (object.ReferenceEquals(null, pp.SetupObject))
            {
                throw new NullReferenceException();
            }

            // TODO: REMOVE

            //var scaleUpper = (pp.SetupObject.Scale & 0xff00) >> 8;
            //var scaleLower = pp.SetupObject.Scale & 0xff;
            //double setupScale = scaleUpper + ((double)scaleLower / 256D);

            //if (object.ReferenceEquals(null, pp.Bbox))
            //{
            //    throw new NullReferenceException($"{nameof(pp.Bbox)} is null, doors require 3d bounding box");
            //}

            //// The game does some weird translation.
            //var bb2 = new BoundingBoxd();
            //bb2.MaxZ = pp.Bbox.MinX;
            //bb2.MinZ = pp.Bbox.MaxX;
            //bb2.MaxY = pp.Bbox.MinY;
            //bb2.MinY = pp.Bbox.MaxY;
            //bb2.MaxX = pp.Bbox.MinZ;
            //bb2.MinX = pp.Bbox.MaxZ;

            //var modelData = Getools.Lib.Game.Asset.Model.ModelDataResolver.GetModelDataFromPropId(pp.Prop);

            //// The axes don't match, but this is what the game does.
            //double xscale = (bb2.MinY - bb2.MaxY) / (modelData.BboxMaxX - modelData.BboxMinX);
            //double yscale = (bb2.MinX - bb2.MaxX) / (modelData.BboxMaxY - modelData.BboxMinY);
            //double zscale = (bb2.MinZ - bb2.MaxZ) / (modelData.BboxMaxZ - modelData.BboxMinZ);

            //if ((xscale <= 0.000001f) || (yscale <= 0.000001f) || (zscale <= 0.000001f))
            //{
            //    xscale =
            //        yscale =
            //        zscale = 1.0f;
            //}

            //double modelScale = xscale;

            //if (modelScale < yscale)
            //{
            //    modelScale = yscale;
            //}

            //if (modelScale < zscale)
            //{
            //    modelScale = zscale;
            //}

            //var pos = Getools.Lib.Math.Pad.GetCenter(pp.Origin, pp.Up, pp.Look, pp.Bbox).Scale(1.0 / levelScale);

            ////// if (!(pp.SetupObject->flags & 0x1000)) // prop flag PROPFLAG_00001000 "Absolute Position"

            //double modelSizeX = modelData.BboxMaxX - modelData.BboxMinX;
            //double modelSizeY = modelData.BboxMaxY - modelData.BboxMinY;
            //double modelSizeZ = modelData.BboxMaxZ - modelData.BboxMinZ;

            //modelSizeX *= setupScale * xscale / levelScale;
            //modelSizeY *= setupScale * yscale / levelScale;
            //modelSizeZ *= setupScale * zscale / levelScale;

            //double halfw = modelSizeX / 2;
            //double halfh = modelSizeZ / 2;

            //double rotAngleRad = System.Math.Atan2(pp.Up.Z, pp.Up.X);
            //rotAngleRad *= -1;

            //if (pp.Look.Y > 0.9)
            //{
            //    rotAngleRad = System.Math.Atan2(pp.Up.X, pp.Up.Z);
            //    rotAngleRad *= -1;
            //    rotAngleRad += System.Math.PI / 2;
            //}

            //double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_door(pp, levelScale);

            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            container.Transform = $"translate({Format.DoubleToStringFormat(stagePosition.Origin.X - stagePosition.HalfModelSize.X, StandardDoubleToStringFormat)}, {Format.DoubleToStringFormat(stagePosition.Origin.Z - stagePosition.HalfModelSize.Z, StandardDoubleToStringFormat)})";

            if (stagePosition.RotationDegrees != 0)
            {
                container.Transform += $" rotate({Format.DoubleToStringFormat(stagePosition.RotationDegrees, StandardDoubleToStringFormat)} {Format.DoubleToStringFormat(stagePosition.HalfModelSize.X, StandardDoubleToStringFormat)} {Format.DoubleToStringFormat(stagePosition.HalfModelSize.Z, StandardDoubleToStringFormat)})";
            }

            var rect = container.AddRect();

            rect.X = 0;
            rect.Y = 0;
            rect.SetWidth(stagePosition.ModelSize.X, StandardDoubleToStringFormat);
            rect.SetHeight(stagePosition.ModelSize.Z, StandardDoubleToStringFormat);
            rect.Stroke = stroke;
            rect.StrokeWidth = strokeWidth;
            rect.Fill = fill;

            return container;
        }

        /// <summary>
        /// Helper function to a prop in the usual manner. The item's bounding box is used.
        /// </summary>
        /// <param name="group">Base SVG container to add item to.</param>
        /// <param name="pp">Object information for item to be added to SVG.</param>
        /// <param name="levelScale">Stage scale factor.</param>
        /// <param name="stroke">Border edge color.</param>
        /// <param name="strokeWidth">Border edge thickness.</param>
        /// <param name="fill">Fill color.</param>
        /// <returns>New item that was appended.</returns>
        private static SvgGroup SvgAppendPropDefaultModelBbox_prop(SvgGroup group, PropPointPosition pp, double levelScale, string stroke, double strokeWidth, string fill)
        {
            // TODO: REMOVE

            //if (object.ReferenceEquals(null, pp.SetupObject))
            //{
            //    throw new NullReferenceException($"{nameof(pp.SetupObject)} is null");
            //}

            //// guard does not implement SetupObjectGenericBase
            //double setupScale = 1.0;

            //bool toXPresetBounds = false;
            //bool toYPresetBounds = false;
            //bool toZPresetBounds = false;

            //if (pp.SetupObject is SetupObjectGenericBase)
            //{
            //    var standardObject = (SetupObjectGenericBase)pp.SetupObject;

            //    var scaleUpper = (pp.SetupObject.Scale & 0xff00) >> 8;
            //    var scaleLower = pp.SetupObject.Scale & 0xff;
            //    setupScale = scaleUpper + ((double)scaleLower / 256D);

            //    toXPresetBounds = (standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag.PropFlag1_XToPresetBounds) > 0;
            //    toYPresetBounds = (standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag.PropFlag1_YToPresetBounds) > 0;
            //    toZPresetBounds = (standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag.PropFlag1_ZToPresetBounds) > 0;
            //}

            //bool boundToPad3dDimensions = false;
            //bool has3dBoundBox = !object.ReferenceEquals(null, pp.Bbox);

            //double modelSizeX = 0;
            //double modelSizeY = 0;
            //double modelSizeZ = 0;

            //var modelData = Getools.Lib.Game.Asset.Model.ModelDataResolver.GetModelDataFromPropId(pp.Prop);
            //double modelScale = 1.0;

            //Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            ////// if ((standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag1_AbsolutePosition) > 0)

            //if (has3dBoundBox)
            //{
            //    pos = Getools.Lib.Math.Pad.GetCenter(pp.Origin, pp.Up, pp.Look, pp.Bbox!).Scale(1.0 / levelScale);
            //}

            //if (boundToPad3dDimensions)
            //{
            //    modelSizeX = pp.Bbox!.MaxX - pp.Bbox.MinX;
            //    modelSizeY = pp.Bbox!.MaxY - pp.Bbox.MinY;
            //    modelSizeZ = pp.Bbox!.MaxZ - pp.Bbox.MinZ;

            //    // The game does some weird translation.
            //    var bb2 = new BoundingBoxd();
            //    bb2.MaxZ = pp.Bbox!.MinX;
            //    bb2.MinZ = pp.Bbox.MaxX;
            //    bb2.MaxY = pp.Bbox.MinY;
            //    bb2.MinY = pp.Bbox.MaxY;
            //    bb2.MaxX = pp.Bbox.MinZ;
            //    bb2.MinX = pp.Bbox.MaxZ;

            //    // The axes don't match, but this is what the game does.
            //    double xscale = (bb2.MinY - bb2.MaxY) / (modelData.BboxMaxX - modelData.BboxMinX);
            //    double yscale = (bb2.MinX - bb2.MaxX) / (modelData.BboxMaxY - modelData.BboxMinY);
            //    double zscale = (bb2.MinZ - bb2.MaxZ) / (modelData.BboxMaxZ - modelData.BboxMinZ);

            //    if ((xscale <= 0.000001f) || (yscale <= 0.000001f) || (zscale <= 0.000001f))
            //    {
            //        xscale =
            //            yscale =
            //            zscale = 1.0f;
            //    }

            //    modelScale = xscale;

            //    if (modelScale < yscale)
            //    {
            //        modelScale = yscale;
            //    }

            //    if (modelScale < zscale)
            //    {
            //        modelScale = zscale;
            //    }

            //    pos = Getools.Lib.Math.Pad.GetCenter(pp.Origin, pp.Up, pp.Look, pp.Bbox);

            //    // if (!(pp.SetupObject->flags & 0x1000)) // prop flag PROPFLAG_00001000 "Absolute Position"
            //    ////

            //    modelSizeX *= setupScale * xscale;
            //    modelSizeY *= setupScale * yscale;
            //    modelSizeZ *= setupScale * zscale;
            //}
            //else
            //{
            //    modelScale = Getools.Lib.Game.Asset.Model.ModelDataResolver.GetModelScaleFromPropId(pp.Prop);

            //    if (has3dBoundBox && toXPresetBounds)
            //    {
            //        modelSizeX = pp.Bbox!.MaxX - pp.Bbox.MinX;
            //        modelSizeX *= setupScale / levelScale;
            //    }
            //    else
            //    {
            //        modelSizeX = modelData.BboxMaxX - modelData.BboxMinX;
            //        modelSizeX *= setupScale * modelScale;
            //    }

            //    if (has3dBoundBox && toYPresetBounds)
            //    {
            //        modelSizeY = pp.Bbox!.MaxY - pp.Bbox.MinY;
            //        modelSizeY *= setupScale / levelScale;
            //    }
            //    else
            //    {
            //        modelSizeY = modelData.BboxMaxY - modelData.BboxMinY;
            //        modelSizeY *= setupScale * modelScale;
            //    }

            //    if (has3dBoundBox && toZPresetBounds)
            //    {
            //        modelSizeZ = pp.Bbox!.MaxZ - pp.Bbox.MinZ;
            //        modelSizeZ *= setupScale / levelScale;
            //    }
            //    else
            //    {
            //        modelSizeZ = modelData.BboxMaxZ - modelData.BboxMinZ;
            //        modelSizeZ *= setupScale * modelScale;
            //    }
            //}

            //double halfw = modelSizeX / 2;
            //double halfh = modelSizeZ / 2;

            //double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            //rotAngleRad *= -1;
            //double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_prop(pp, levelScale);

            double modelSizeX = stagePosition.ModelSize.X;
            double modelSizeZ = stagePosition.ModelSize.Z;

            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            container.Transform = $"translate({Format.DoubleToStringFormat(stagePosition.Origin.X - stagePosition.HalfModelSize.X, StandardDoubleToStringFormat)}, {Format.DoubleToStringFormat(stagePosition.Origin.Z - stagePosition.HalfModelSize.X, StandardDoubleToStringFormat)})";

            if (stagePosition.RotationDegrees != 0)
            {
                container.Transform += $" rotate({Format.DoubleToStringFormat(stagePosition.RotationDegrees, StandardDoubleToStringFormat)} {Format.DoubleToStringFormat(stagePosition.HalfModelSize.X, StandardDoubleToStringFormat)} {Format.DoubleToStringFormat(stagePosition.HalfModelSize.Z, StandardDoubleToStringFormat)})";
            }

            var rect = container.AddRect();

            rect.X = 0;
            rect.Y = 0;
            rect.SetWidth(modelSizeX, StandardDoubleToStringFormat);
            rect.SetHeight(modelSizeZ, StandardDoubleToStringFormat);
            rect.Stroke = stroke;
            rect.StrokeWidth = strokeWidth;
            rect.Fill = fill;

            return container;
        }

        /// <summary>
        /// Helper function to draw a guard in the usual manner. A hand drawn SVG item is used for the image.
        /// </summary>
        /// <param name="group">Base SVG container to add item to.</param>
        /// <param name="pp">Object information for item to be added to SVG.</param>
        /// <param name="levelScale">Stage scale factor.</param>
        /// <returns>New item that was appended.</returns>
        private static SvgGroup SvgAppendPropDefaultModelBbox_chr(SvgGroup group, PropPointPosition pp, double levelScale)
        {
            // TODO: REMOVE

            //if (object.ReferenceEquals(null, pp.SetupObject))
            //{
            //    throw new NullReferenceException($"{nameof(pp.SetupObject)} is null");
            //}

            //// guard does not implement SetupObjectGenericBase
            //var guard = (SetupObjectGuard)pp.SetupObject!;
            //bool isClone = false;

            //if ((guard.Flags & Getools.Lib.Game.Flags.SetupChrFlags.GUARD_SETUP_FLAG_CHR_CLONE) > 0)
            //{
            //    isClone = true;
            //}

            //Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            //// const fot all stages
            //double scaleFactor = 7;

            //double modelSizeX = 12 * scaleFactor;
            //double modelSizeZ = 5 * scaleFactor;
            //double halfw = modelSizeX / 2;
            //double halfh = modelSizeZ / 2;

            //double translateX = pos.X - halfw;
            //double translateY = pos.Z - halfh;

            ////// chr ??
            //double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            ////// rotAngleRad *= -1; // but not for chr ??
            //double rotAngle = rotAngleRad * 180 / System.Math.PI;

            // guard does not implement SetupObjectGenericBase
            var guard = (SetupObjectGuard)pp.SetupObject!;
            bool isClone = false;

            if ((guard.Flags & Getools.Lib.Game.Flags.SetupChrFlags.GUARD_SETUP_FLAG_CHR_CLONE) > 0)
            {
                isClone = true;
            }

            // const for all stages
            double scaleFactor = 7;

            var stagePosition = Getools.Lib.Game.Engine.World.GetPropDefaultModelBbox_chr(pp, levelScale);

            double translateX = stagePosition.Origin.X - stagePosition.HalfModelSize.X;
            double translateY = stagePosition.Origin.Z - stagePosition.HalfModelSize.Z;
            double modelSizeX = stagePosition.ModelSize.X;
            double modelSizeY = stagePosition.ModelSize.Y;
            double modelSizeZ = stagePosition.ModelSize.Z;
            double halfw = modelSizeX / 2;
            double halfh = modelSizeZ / 2;

            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            container.Transform = $"translate({Format.DoubleToStringFormat(translateX, StandardDoubleToStringFormat)}, {Format.DoubleToStringFormat(translateY, StandardDoubleToStringFormat)})";

            if (stagePosition.RotationDegrees != 0)
            {
                container.Transform += $" rotate({Format.DoubleToStringFormat(stagePosition.RotationDegrees, StandardDoubleToStringFormat)} {Format.DoubleToStringFormat(stagePosition.HalfModelSize.X, StandardDoubleToStringFormat)} {Format.DoubleToStringFormat(stagePosition.HalfModelSize.Z, StandardDoubleToStringFormat)})";
            }

            ////

            var rect = container.AddRect();

            rect.X = 0;
            rect.Y = 0;
            rect.SetWidth(modelSizeX, StandardDoubleToStringFormat);
            rect.SetHeight(modelSizeZ, StandardDoubleToStringFormat);
            if (isClone)
            {
                rect.Fill = "#ffad3b";
            }
            else
            {
                rect.Fill = "#ffdd55";
            }

            rect.Stroke = "#a87928";
            rect.StrokeWidth = 1;

            ////

            var ellipse = container.AddEllipse();

            if (isClone)
            {
                ellipse.Fill = "#ffad3b";
            }
            else
            {
                ellipse.Fill = "#ffdd55";
            }

            ellipse.Stroke = "#a87928";
            ellipse.StrokeWidth = 1;

            ellipse.CX = 6 * scaleFactor;
            ellipse.CY = 3 * scaleFactor;
            ellipse.RX = 3.5 * scaleFactor;
            ellipse.RY = 3.5 * scaleFactor;

            ////

            var path = container.AddPath();

            if (isClone)
            {
                path.Fill = "#ffda91";
            }
            else
            {
                path.Fill = "#ffeeaa";
            }

            path.Stroke = "#a87928";
            path.StrokeWidth = 1;

            Coord2dd p1 = new Coord2dd(7.7, 4).Scale(scaleFactor);
            Coord2dd p2 = new Coord2dd(4.3, 0).Scale(scaleFactor);
            Coord2dd p3 = new Coord2dd(6, 0.3).Scale(scaleFactor);

            path.D = $"M {Format.DoubleToStringFormat(p1.X, StandardDoubleToStringFormat)},{Format.DoubleToStringFormat(p1.Y, StandardDoubleToStringFormat)} H {Format.DoubleToStringFormat(p2.X, StandardDoubleToStringFormat)} L {Format.DoubleToStringFormat(p3.X, StandardDoubleToStringFormat)},{Format.DoubleToStringFormat(p3.Y, StandardDoubleToStringFormat)} Z";

            ////

            return container;
        }

        /// <summary>
        /// Helper function to draw a key in the usual manner. A hand drawn SVG item is used for the image.
        /// </summary>
        /// <param name="group">Base SVG container to add item to.</param>
        /// <param name="pp">Object information for item to be added to SVG.</param>
        /// <param name="levelScale">Stage scale factor.</param>
        /// <returns>New item that was appended.</returns>
        private static SvgGroup SvgGroupAppendKey(SvgGroup group, PropPointPosition pp, double levelScale)
        {
            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            var path = container.AddPath();

            path.Fill = "#cca800";
            path.Stroke = "#906a00";
            path.StrokeWidth = 2;

            path.D = "m 0.43595022,7.014382 c -0.30029153,5.40526 4.65446948,6.606259 4.65446948,6.606259 l 1.0508961,33.933061 4.5044672,-0.300226 -0.450449,-3.002907 4.80469,0.450449 1.351343,-3.303129 h -6.156033 v -4.354245 l 5.255139,0.750671 1.201115,-5.705583 -6.156028,0.300225 -0.300226,-18.618095 c 0,0 4.129114,-0.488029 4.542022,-6.9818504 C 14.428092,-2.0699217 0.57338466,-1.3909436 0.43595022,7.014382 Z M 7.7861017,2.5626133 c 2.5377613,-3.68e-5 4.5950313,2.057233 4.5949933,4.5949926 3.8e-5,2.5377596 -2.057232,4.5950291 -4.5949933,4.5949921 C 5.2484276,11.752514 3.1912911,9.6952796 3.1913279,7.1576059 3.1912911,4.6199322 5.2484276,2.5626979 7.7861017,2.5626133 Z";

            path.Transform = $"translate({Format.DoubleToStringFormat(pos.X, StandardDoubleToStringFormat)}, {Format.DoubleToStringFormat(pos.Z, StandardDoubleToStringFormat)}) scale(3)";

            return container;
        }

        /// <summary>
        /// Helper function to draw a security camera in the usual manner. A hand drawn SVG item is used for the image.
        /// </summary>
        /// <param name="group">Base SVG container to add item to.</param>
        /// <param name="pp">Object information for item to be added to SVG.</param>
        /// <param name="levelScale">Stage scale factor.</param>
        /// <returns>New item that was appended.</returns>
        private static SvgGroup SvgGroupAppendCctv(SvgGroup group, PropPointPosition pp, double levelScale)
        {
            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the cctv svg.
            container.Transform = $"translate({Format.DoubleToStringFormat(pos.X - 10, StandardDoubleToStringFormat)}, {Format.DoubleToStringFormat(pos.Z, StandardDoubleToStringFormat)}) rotate({Format.DoubleToStringFormat(rotAngle, StandardDoubleToStringFormat)} 45 30)";

            SvgPath path;

            ////

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "m 66.842669,47.084191 v 6.569513 H 45.49174 v -6.569513 z";

            ////

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "M 34.862376,18.053258 11.514484,15.189837 11.001787,27.375645 1.1475142,15.878994 V 9.3094783 L 33.100271,14.969574 Z";

            ////

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "m 11.001787,10.951858 22.993304,4.927136 v 26.27806 L 11.001787,37.229918 Z";

            ////

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "M 33.995091,42.157054 75.054559,24.090887 V 15.878994 L 33.995091,27.375645 Z";

            ////

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "m 45.49174,37.229918 v 9.854273 h 6.56952 V 34.165424 Z";

            ////

            path = container.AddPath();

            path.Fill = "#ffffff";
            path.Stroke = "#838383";
            path.StrokeWidth = 0.94691;

            path.D = "m 13.857191,14.310917 17.230768,3.824947 V 38.535596 L 13.857191,34.710651 Z";

            ////

            var ellipse = container.AddEllipse();

            ellipse.Fill = "#fefdff";
            ellipse.Stroke = "#000000";
            ellipse.StrokeWidth = 1.24148;

            ellipse.CX = 22.527639;
            ellipse.CY = 26.423256;
            ellipse.RX = 5.396447;
            ellipse.RY = 6.3876319;

            ////

            path = container.AddPath();

            path.Fill = "#000000";
            path.Stroke = "#ffffff";
            path.StrokeWidth = 1.24148;

            path.D = "M 33.995091,27.375645 27.373429,13.647994 1.1475142,9.3094783 60.273151,1.0975849 h 14.781408 l 3.284759,1.6423788 V 14.236615 l -3.284759,1.642379 z";

            return container;
        }

        /// <summary>
        /// Helper function to draw a lock in the usual manner. A hand drawn SVG item is used for the image.
        /// </summary>
        /// <param name="group">Base SVG container to add item to.</param>
        /// <param name="pp">Object information for item to be added to SVG.</param>
        /// <param name="levelScale">Stage scale factor.</param>
        /// <returns>New item that was appended.</returns>
        private static SvgGroup SvgAppendPadlock(SvgGroup group, PropPointPosition pp, double levelScale)
        {
            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({Format.DoubleToStringFormat(pos.X - 8, StandardDoubleToStringFormat)}, {Format.DoubleToStringFormat(pos.Z - 18, StandardDoubleToStringFormat)}) rotate({Format.DoubleToStringFormat(rotAngle, StandardDoubleToStringFormat)} 8 18)";

            SvgRect rect;

            ////

            rect = container.AddRect();

            rect.Fill = "#b3b3b3";
            rect.Stroke = "#4d4d4d";
            rect.StrokeWidth = 0.349545;

            rect.Width = 13.661667;
            rect.Height = 16.166445;
            rect.X = 0.44301221;
            rect.Y = 10.843964;

            ////

            var path = container.AddPath();

            path.Fill = "#b3b3b3";
            path.Stroke = "#4d4d4d";
            path.StrokeWidth = 1.25657;

            path.D = "M 7.2412021,0.69852204 C 1.0085037,0.68070391 0.68529965,5.2915851 0.68481911,10.909995 L 13.865147,10.801897 C 13.826836,5.1838055 13.473901,0.71634014 7.2412021,0.69852204 Z M 7.4564339,3.2948227 c 3.7810651,0.013199 3.9947521,3.3229823 4.0179921,7.4858393 l -7.9954469,0.08011 C 3.4792705,6.6976769 3.6753692,3.28162 7.4564339,3.2948227 Z";

            ////

            return container;
        }

        /// <summary>
        /// Helper function to draw a drone gun in the usual manner. A hand drawn SVG item is used for the image.
        /// </summary>
        /// <param name="group">Base SVG container to add item to.</param>
        /// <param name="pp">Object information for item to be added to SVG.</param>
        /// <param name="levelScale">Stage scale factor.</param>
        /// <returns>New item that was appended.</returns>
        private static SvgGroup SvgAppendHeavyGun(SvgGroup group, PropPointPosition pp, double levelScale)
        {
            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            var container = group.AddGroup();

            container.AddClass("svg-logical-item");

            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            rotAngleRad *= -1;
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            // move svg to map coordinate point.
            // Rotate it by the preset definition, around the center of the svg.
            container.Transform = $"translate({Format.DoubleToStringFormat(pos.X - 30, StandardDoubleToStringFormat)}, {Format.DoubleToStringFormat(pos.Z - 32, StandardDoubleToStringFormat)}) rotate({Format.DoubleToStringFormat(rotAngle, StandardDoubleToStringFormat)} 30 32) scale(2)";

            SvgRect rect;

            ////

            var circle = container.AddCircle();

            circle.Fill = "#94dff2";
            circle.Stroke = "#000000";
            circle.StrokeWidth = 0.534929;

            circle.CY = 30.219355;
            circle.CX = 30.219355;
            circle.R = 29.951891;

            ////

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
    }
}
