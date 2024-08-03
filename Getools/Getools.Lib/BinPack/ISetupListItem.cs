using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// Indicates item is an element of an arry or list as found in binary setup file section.
    /// </summary>
    public interface ISetupListItem
    {
        /// <summary>
        /// Natural index of the item in the container list, as found in the binary.
        /// </summary>
        int ListIndex { get; set; }
    }
}
