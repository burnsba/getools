using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Compiler.Map
{
    public interface IMapParser
    {
        List<MapDetail> ParseMapFile(string path, Func<string, bool>? segmentFilter = null, Func<UInt32, bool>? addressFilter = null);
    }
}
