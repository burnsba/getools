using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public interface IAiParameter : IAiCmacro, IAiByteConvertable
    {
        string ParameterName { get; init; }
        int ByteLength { get; init; }
        int ByteValue { get; init; }
    }
}
