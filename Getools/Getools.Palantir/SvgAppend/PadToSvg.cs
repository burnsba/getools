using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Extensions;
using Getools.Lib.Game;
using Getools.Lib.Game.Engine;
using Getools.Palantir.Render;
using SvgLib;

namespace Getools.Palantir.SvgAppend
{
    /// <summary>
    /// Helper class to draw pad onto SVG.
    /// </summary>
    internal static class PadToSvg
    {
        // three decimal places
        private const string StandardDoubleToStringFormat = "0.###";

        /// <summary>
        /// Appends pad to SVG.
        /// </summary>
        /// <param name="appendTo">Base SVG container to add item to.</param>
        /// <param name="rp">Object information for item to be added to SVG.</param>
        /// <param name="levelScale">Stage scale factor.</param>
        /// <returns>New item that was appended.</returns>
        internal static SvgContainer? PadToSvgAppend(SvgGroup appendTo, PointPosition rp, double levelScale)
        {
            // TODO: REMOVE

            //double scaleFactor = 1 / levelScale;

            //Coord3dd pos = rp.Origin.Clone().Scale(scaleFactor);

            //// If this is a pad3d, need to calculate orientation, then translate by the
            //// 3d bounds.
            //if (rp.Bbox != null && rp.Up.Y > 0)
            //{
            //    double angle = -1.0 * System.Math.Atan2(rp.Look.X, rp.Look.Z);
            //    var cos = System.Math.Cos(angle);
            //    var sin = System.Math.Sin(angle);

            //    // Find the center point of the x and z bounds.
            //    double bbx = (rp.Bbox.MinX + rp.Bbox.MaxX) / 2;
            //    double bbz = (rp.Bbox.MinZ + rp.Bbox.MaxZ) / 2;

            //    // Rotate offset by the angle described by Look
            //    double xoffset = (bbx * cos) - (bbz * sin);
            //    double zoffset = (bbx * sin) + (bbz * cos);

            //    pos.X += xoffset * scaleFactor;
            //    pos.Z += zoffset * scaleFactor;
            //}

            //double modelSizeX = 36;
            //double modelSizeZ = 36;
            //double halfw = modelSizeX / 2;
            //double halfh = modelSizeZ / 2;

            //double translateX = pos.X - halfw;
            //double translateY = pos.Z - halfh;

            var stagePosition = Getools.Lib.Game.Engine.World.GetPadBbox(rp, levelScale);

            double translateX = stagePosition.Origin.X - stagePosition.HalfModelSize.X;
            double translateY = stagePosition.Origin.Z - stagePosition.HalfModelSize.Z;

            var container = appendTo.AddGroup();

            container.AddClass("svg-logical-item");

            container.Transform = $"translate({Format.DoubleToStringFormat(translateX, StandardDoubleToStringFormat)}, {Format.DoubleToStringFormat(translateY, StandardDoubleToStringFormat)})";

            var rect = container.AddRect();

            rect.X = 0;
            rect.Y = 0;
            rect.SetWidth(stagePosition.ModelSize.X, StandardDoubleToStringFormat);
            rect.SetHeight(stagePosition.ModelSize.Z, StandardDoubleToStringFormat);

            return container;
        }
    }
}
