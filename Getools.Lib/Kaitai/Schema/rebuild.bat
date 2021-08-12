
set comp="c:\Program Files (x86)\kaitai-struct-compiler\bin\kaitai-struct-compiler.bat"

for %%f in (*.ksy) do (
    %comp% --target csharp --dotnet-namespace Getools.Lib.Kaitai.Gen %%~nf.ksy
)