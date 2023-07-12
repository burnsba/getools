using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;
using Getools.Lib.Extensions;
using Getools.Palantir.Render;
using SvgLib;

namespace Getools.Palantir.SvgAppend
{
    internal static class PadToSvg
    {
        // three decimal places
        private const string StandardDoubleToStringFormat = "0.###";

        internal static SvgContainer? PadToSvgAppend(SvgGroup appendTo, RenderPosition rp, double levelScale)
        {
            double scaleFactor = 1 / levelScale;

            Coord3dd pos = rp.Origin.Clone().Scale(scaleFactor);

            double modelSizeX = 32;
            double modelSizeZ = 32;
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
