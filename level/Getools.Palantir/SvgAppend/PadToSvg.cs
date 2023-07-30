using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;
using Getools.Lib.Extensions;
using Getools.Palantir.Render;
using SvgLib;
using System.Reflection.Metadata;

namespace Getools.Palantir.SvgAppend
{
    internal static class PadToSvg
    {
        // three decimal places
        private const string StandardDoubleToStringFormat = "0.###";

        internal static SvgContainer? PadToSvgAppend(SvgGroup appendTo, RenderPosition rp, double levelScale)
        {
            double scaleFactor = 1 / levelScale;

            if (rp.PadId == 0x27a3)
            {
                var a = 0;
            }

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
                double xoffset = bbx * cos - bbz * sin;
                double zoffset = bbx * sin + bbz * cos;

                pos.X += xoffset * scaleFactor;
                pos.Z += zoffset * scaleFactor;
            }

            double modelSizeX = 36;
            double modelSizeZ = 36;
            double halfw = modelSizeX / 2;
            double halfh = modelSizeZ / 2;

            double translateX = pos.X - halfw;
            double translateY = pos.Z - halfh;

            var container = appendTo.AddGroup();

            container.AddClass("svg-logical-item");

            container.Transform = $"translate({Format.DoubleToStringFormat(translateX, StandardDoubleToStringFormat)}, {Format.DoubleToStringFormat(translateY, StandardDoubleToStringFormat)})";

            var rect = container.AddRect();

            rect.X = 0;
            rect.Y = 0;
            rect.SetWidth(modelSizeX, StandardDoubleToStringFormat);
            rect.SetHeight(modelSizeZ, StandardDoubleToStringFormat);

            return container;
        }

    }
}
