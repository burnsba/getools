using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace SvgLib
{
    public sealed class SvgDocument : SvgContainer
    {
        private readonly XmlDocument _document;
        private readonly XmlElement _styleNode;

        private SvgDocument(XmlDocument document, XmlElement element, XmlElement styleNode)
            : base(element)
        {
            _document = document;
            _styleNode = styleNode;
        }

        public static SvgDocument Create()
        {
            var document = new XmlDocument();
            var rootElement = document.CreateElement("svg");
            document.AppendChild(rootElement);
            rootElement.SetAttribute("xmlns", "http://www.w3.org/2000/svg");

            var defs = document.CreateElement("defs");
            var styleNode = document.CreateElement("style");
            styleNode.SetAttribute("type", "text/css");

            defs.AppendChild(styleNode);
            rootElement.AppendChild(defs);

            var svg = new SvgDocument(document, rootElement, styleNode);

            return svg;
        }

        public void Save(Stream stream) => _document.Save(stream);

        public void SetStylesheet(string cssText)
        {
            XmlCDataSection cdata = _document.CreateCDataSection(cssText);
            _styleNode.InnerXml = cdata.OuterXml;
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

        public SvgViewBox ViewBox
        {
            get => Element.GetAttribute("viewBox", new SvgViewBox());
            set => Element.SetAttribute("viewBox", value.ToString());
        }
    }
}
