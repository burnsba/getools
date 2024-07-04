using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Extensions;

namespace Getools.Lib.Game.Engine
{
    public static class World
    {
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
    }
}
