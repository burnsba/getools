using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Compiler.Map
{
    /// <summary>
    /// Interface to describe a compiler map parser.
    /// </summary>
    public interface IMapParser
    {
        /// <summary>
        /// Parses a map file into a list of variables.
        /// </summary>
        /// <param name="path">Full path name to map file.</param>
        /// <param name="segmentFilter">Optional. If non-null, used to filter segments from the map file. When the method returns true the item is allowed.</param>
        /// <param name="addressFilter">Optional. If non-null, used to filter addresses from the map file. When the method returns true the item is allowed.</param>
        /// <returns>List of parsed variables with related information.</returns>
        List<MapDetail> ParseMapFile(string path, Func<string, bool>? segmentFilter = null, Func<UInt32, bool>? addressFilter = null);
    }
}
