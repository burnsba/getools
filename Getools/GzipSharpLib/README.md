# GzipSharpLib

C# partial port of GNU gzip `inflate.c`, from version 1.2.4 release. This is the version used by Rare. The `inflate.c` was placed in the public domain, separate from GNU gzip.

This library can inflate regular gzip compressed data, as well as the 1172 format used by Rare. Compressing data is not currently supported.

Note that this is a port of the c code to C#, translating various memory calls and pointer references to a functionally equivalent C# implementation. No `unsafe` code or marshalled calls are used; all objects are fully managed by the C# runtime.

https://ftp.gnu.org/gnu/gzip/
