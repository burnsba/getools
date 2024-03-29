﻿using System.Globalization;
using System.Xml;

namespace SvgLib
{
    public sealed class SvgRect : SvgBasicShape
    {
        private SvgRect(XmlElement element)
            : base(element)
        {
        }

        internal static SvgRect Create(XmlElement parent)
        {
            var element = parent.OwnerDocument.CreateElement("rect");
            parent.AppendChild(element);
            return new SvgRect(element);
        }

        public double X
        {
            get => Element.GetAttribute("x", SvgDefaults.Attributes.Position.X);
            set => Element.SetAttribute("x", value);
        }

        public double Y
        {
            get => Element.GetAttribute("y", SvgDefaults.Attributes.Position.Y);
            set => Element.SetAttribute("y", value);
        }

        public double Width
        {
            get => Element.GetAttribute("width", SvgDefaults.Attributes.Size.Width);
            set => Element.SetAttribute("width", value);
        }

        public double Height
        {
            get => Element.GetAttribute("height", SvgDefaults.Attributes.Size.Height);
            set => Element.SetAttribute("height", value);
        }

        public void SetX(double d, string format)
        {
            Element.SetAttribute("x", d.ToString(format, CultureInfo.InvariantCulture));
        }

        public void SetY(double d, string format)
        {
            Element.SetAttribute("y", d.ToString(format, CultureInfo.InvariantCulture));
        }

        public void SetWidth(double d, string format)
        {
            Element.SetAttribute("width", d.ToString(format, CultureInfo.InvariantCulture));
        }

        public void SetHeight(double d, string format)
        {
            Element.SetAttribute("height", d.ToString(format, CultureInfo.InvariantCulture));
        }
    }
}
