using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Extensions;
using Getools.Lib.Game.Asset.SetupObject;

namespace Getools.Lib.Game.Engine
{
    /// <summary>
    /// Methods for resolving position and orientation information according to the
    /// retail logic of the game.
    /// </summary>
    public static class World
    {
        /// <summary>
        /// For a given prop, gets the runtime position and bounding box size for drawing on some type of screen.
        /// </summary>
        /// <param name="pp">Prop point source.</param>
        /// <param name="levelScale">Level scale.</param>
        /// <returns>Door size and position information.</returns>
        /// <exception cref="NullReferenceException">
        /// <see cref="PointPosition.Bbox"/> and <see cref="PointPosition.SetupObject"/> are required.
        /// </exception>
        public static RuntimePropPosition GetPropDefaultModelBbox_door(PropPointPosition pp, double levelScale)
        {
            if (object.ReferenceEquals(null, pp.SetupObject))
            {
                throw new NullReferenceException();
            }

            var scaleUpper = (pp.SetupObject.Scale & 0xff00) >> 8;
            var scaleLower = pp.SetupObject.Scale & 0xff;
            double setupScale = scaleUpper + ((double)scaleLower / 256D);

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

            //// if (!(pp.SetupObject->flags & 0x1000)) // prop flag PROPFLAG_00001000 "Absolute Position"

            double modelSizeX = modelData.BboxMaxX - modelData.BboxMinX;
            double modelSizeY = modelData.BboxMaxY - modelData.BboxMinY;
            double modelSizeZ = modelData.BboxMaxZ - modelData.BboxMinZ;

            modelSizeX *= setupScale * xscale / levelScale;
            modelSizeY *= setupScale * yscale / levelScale;
            modelSizeZ *= setupScale * zscale / levelScale;

            double rotAngleRad = System.Math.Atan2(pp.Up.Z, pp.Up.X);
            rotAngleRad *= -1;

            if (pp.Look.Y > 0.9)
            {
                rotAngleRad = System.Math.Atan2(pp.Up.X, pp.Up.Z);
                rotAngleRad *= -1;
                rotAngleRad += System.Math.PI / 2;
            }

            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var result = new RuntimePropPosition()
            {
                Source = pp,
                RotationDegrees = rotAngle,
                ModelSize = new Coord3dd(modelSizeX, modelSizeY, modelSizeZ),
                HalfModelSize = new Coord3dd(modelSizeX / 2.0, modelSizeY / 2.0, modelSizeZ / 2.0),
                Origin = pos.Clone(),
            };

            return result;
        }

        /// <summary>
        /// For a given prop, gets the runtime position and bounding box size for drawing on some type of screen.
        /// </summary>
        /// <param name="pp">Prop point source.</param>
        /// <param name="levelScale">Level scale.</param>
        /// <returns>Character size and position information.</returns>
        /// <exception cref="NullReferenceException">
        /// <see cref="PointPosition.SetupObject"/> is required.
        /// </exception>
        public static RuntimePropPosition GetPropDefaultModelBbox_chr(PropPointPosition pp, double levelScale)
        {
            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            // I don't know.
            double scaleFactor = 7;

            double modelSizeX = 12 * scaleFactor;
            double modelSizeY = 5 * scaleFactor;
            double modelSizeZ = 5 * scaleFactor;
            double halfw = modelSizeX / 2;
            double halfh = modelSizeZ / 2;

            double translateX = pos.X - halfw;
            double translateY = pos.Z - halfh;

            //// chr ??
            double rotAngleRad = System.Math.Atan2(pp.Look.X, pp.Look.Z);
            //// rotAngleRad *= -1; // but not for chr ??
            double rotAngle = rotAngleRad * 180 / System.Math.PI;

            var result = new RuntimePropPosition()
            {
                Source = pp,
                RotationDegrees = rotAngle,
                ModelSize = new Coord3dd(modelSizeX, modelSizeY, modelSizeZ),
                HalfModelSize = new Coord3dd(modelSizeX / 2.0, modelSizeY / 2.0, modelSizeZ / 2.0),
                Origin = pos.Clone(),
            };

            return result;
        }

        /// <summary>
        /// For a given prop, gets the runtime position and bounding box size for drawing on some type of screen.
        /// </summary>
        /// <param name="pp">Prop point source.</param>
        /// <param name="levelScale">Level scale.</param>
        /// <returns>Prop size and position information.</returns>
        /// <exception cref="NullReferenceException">
        /// <see cref="PointPosition.SetupObject"/> is required.
        /// </exception>
        public static RuntimePropPosition GetPropDefaultModelBbox_prop(PropPointPosition pp, double levelScale)
        {
            if (object.ReferenceEquals(null, pp.SetupObject))
            {
                throw new NullReferenceException($"{nameof(pp.SetupObject)} is null");
            }

            // guard does not implement SetupObjectGenericBase
            double setupScale = 1.0;

            bool toXPresetBounds = false;
            bool toYPresetBounds = false;
            bool toZPresetBounds = false;

            if (pp.SetupObject is SetupObjectGenericBase)
            {
                var standardObject = (SetupObjectGenericBase)pp.SetupObject;

                var scaleUpper = (pp.SetupObject.Scale & 0xff00) >> 8;
                var scaleLower = pp.SetupObject.Scale & 0xff;
                setupScale = scaleUpper + ((double)scaleLower / 256D);

                toXPresetBounds = (standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag.PropFlag1_XToPresetBounds) > 0;
                toYPresetBounds = (standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag.PropFlag1_YToPresetBounds) > 0;
                toZPresetBounds = (standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag.PropFlag1_ZToPresetBounds) > 0;
            }

            bool boundToPad3dDimensions = false;
            bool has3dBoundBox = !object.ReferenceEquals(null, pp.Bbox);

            double modelSizeX = 0;
            double modelSizeY = 0;
            double modelSizeZ = 0;

            var modelData = Getools.Lib.Game.Asset.Model.ModelDataResolver.GetModelDataFromPropId(pp.Prop);
            double modelScale = 1.0;

            Coord3dd pos = pp.Origin.Clone().Scale(1.0 / levelScale);

            //// if ((standardObject.Flags1 & Getools.Lib.Game.Flags.PropFlag1_AbsolutePosition) > 0)

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
                ////

                modelSizeX *= setupScale * xscale;
                modelSizeY *= setupScale * yscale;
                modelSizeZ *= setupScale * zscale;
            }
            else
            {
                modelScale = Getools.Lib.Game.Asset.Model.ModelDataResolver.GetModelScaleFromPropId(pp.Prop);

                if (has3dBoundBox && toXPresetBounds)
                {
                    modelSizeX = pp.Bbox!.MaxX - pp.Bbox.MinX;
                    modelSizeX *= setupScale / levelScale;
                }
                else
                {
                    modelSizeX = modelData.BboxMaxX - modelData.BboxMinX;
                    modelSizeX *= setupScale * modelScale;
                }

                if (has3dBoundBox && toYPresetBounds)
                {
                    modelSizeY = pp.Bbox!.MaxY - pp.Bbox.MinY;
                    modelSizeY *= setupScale / levelScale;
                }
                else
                {
                    modelSizeY = modelData.BboxMaxY - modelData.BboxMinY;
                    modelSizeY *= setupScale * modelScale;
                }

                if (has3dBoundBox && toZPresetBounds)
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

            var result = new RuntimePropPosition()
            {
                Source = pp,
                RotationDegrees = rotAngle,
                ModelSize = new Coord3dd(modelSizeX, modelSizeY, modelSizeZ),
                HalfModelSize = new Coord3dd(modelSizeX / 2.0, modelSizeY / 2.0, modelSizeZ / 2.0),
                Origin = pos.Clone(),
            };

            return result;
        }

        /// <summary>
        /// Gets the pad or pad3d bounding box.
        /// </summary>
        /// <param name="rp">Pad point source.</param>
        /// <param name="levelScale">Level scale.</param>
        /// <returns>Pad size and position information.</returns>
        public static RuntimePosition GetPadBbox(PointPosition rp, double levelScale)
        {
            double scaleFactor = 1 / levelScale;

            Coord3dd pos = rp.Origin.Clone().Scale(scaleFactor);

            // If this is a pad3d, need to calculate orientation, then translate by the
            // 3d bounds.
            if (rp.Bbox != null && rp.Up.Y > 0)
            {
                double angle = -1.0 * System.Math.Atan2(rp.Look.X, rp.Look.Z);
                var cos = System.Math.Cos(angle);
                var sin = System.Math.Sin(angle);

                // Find the center point of the x and z bounds.
                double bbx = (rp.Bbox.MinX + rp.Bbox.MaxX) / 2;
                double bbz = (rp.Bbox.MinZ + rp.Bbox.MaxZ) / 2;

                // Rotate offset by the angle described by Look
                double xoffset = (bbx * cos) - (bbz * sin);
                double zoffset = (bbx * sin) + (bbz * cos);

                pos.X += xoffset * scaleFactor;
                pos.Z += zoffset * scaleFactor;
            }

            // this is arbitrary ...
            double modelSizeX = 36;
            double modelSizeY = 36;
            double modelSizeZ = 36;
            double half = modelSizeX / 2;

            var result = new RuntimePosition()
            {
                RotationDegrees = 0,
                ModelSize = new Coord3dd(modelSizeX, modelSizeY, modelSizeZ),
                HalfModelSize = new Coord3dd(half, half, half),
                Origin = pos.Clone(),
            };

            return result;
        }
    }
}
