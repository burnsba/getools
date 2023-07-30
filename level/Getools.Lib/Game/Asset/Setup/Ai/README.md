# AI

In C#, AI commands are split into two types, a variable length command and a fixed length command. These implement interfaces `IAiVariableCommandDescription` and `IAiFixedCommandDescription` respectively.

The `AiCommandBuilder` class is used to parse raw bytes as AI commands and convert into strongly typed objects.

The initial `bondaicommands.h` file was parsed to generate the names of commands, and the names of each parameter. The parameter names were mostly consistent enough to assume type information from each parameter. This allowed the declaration of every single existing command in the partial class `AiCommandBuilder`.

Individual commands (with parameters) are combined into one `AiCommandBlock`. This corresponds to an AI script as used in the game; this might be assigned to a guard for instance.

There are a few "global" AI command blocks. These are stored as raw binary in `GlobalAiScript`, and parsed on demand.
