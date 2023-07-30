# Game

Core C# code to define game objects. This matches original c type definitions

The namespace is further broken down:

- Asset: Matches original game asset definitions.
- Asset/Bg: bg file and runtime data structs
- Asset/Intro: Setup into section and runtime data structs
- [Asset/Model](Asset/Model/README.md): Preliminary model data support.
- Asset/Prop:
- Asset/Setup: "everything else" setup sections and runtime data structs
- [Asset/Setup/Ai](Asset/Setup/Ai/README.md): Methods for reading/parsing AI command bytes.
- Asset/SetupObject: Support for all the various types of "setup object" and runtime data structs
- Asset/Stan: stan file and runtime data structs (including beta)
- Enums: Original game enums
- Flags: Original game constants

See also

- [Asset/Setup/data_readme.md](Asset/Setup/data_readme.md): Technical information on setup data types
- [Asset/Setup/data_readme_propdef.md](Asset/Setup/data_readme_propdef.md): Technical information on PROPDEF
