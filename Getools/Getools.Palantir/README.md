# Getools.Palantir

Library to read one or more game data sources and visualize the (2d) data.

The project is split into the following namespaces:

- ImageRef: preliminary sketch of SVG files. The path information was copied into C# to use when building output SVG file.
- Render: Data objects used to define point/line definitions that will be displayed on output image.
- SvgAppend: Explains how to convert a game object into an SVG item.

The core library code is defined in `MapImageMaker`. This parses the various data sources (bg, setup, stan) and translates into the generic `Render` data objects that will be displayed.

The `SvgBuilder` class takes the generic `Render` objects and converts into an output SVG file.
