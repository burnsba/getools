# Getools.Lib

The core code is contained in this library project.

The project is split into the following namespaces:

- Antlr: Preliminary support to parse an existing .c file and load data definitions into well defined C# objects in memory. Currently only supports stan (and beta stan) files.
- Architecture: Low level CPU definition code
- BinPack: Used to convert C# objects into a binary blob as used by the game engine. This is a pseudo assembler.
- Converters: Top level entry point to convert a game object to another format, e.g., json to binary.
- Error: Exception definitions.
- Extensions: Class extension methods.
- Formatters: Used to format/display type information.
- [Game](Game\README.md): Core C# code to define game objects. This matches original c type definitions.
- Kaitai: [Kaitai struct](https://kaitai.io/) definitions and C# parsers for binary data formats.
- Math: Various math libraries; direct ports of original game math functions.
