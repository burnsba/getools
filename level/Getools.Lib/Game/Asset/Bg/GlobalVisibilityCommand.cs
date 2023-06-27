using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Bg
{
    public class GlobalVisibilityCommand
    {
        public uint Command { get; set; }

        /// <summary>
        /// When loading a binary file, this will be the index of the roomdata seen so far (0,1,2,...).
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
