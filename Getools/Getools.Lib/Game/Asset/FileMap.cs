using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset
{
    public class FileMap
    {
        /// <summary>
        /// Directory prefix, relative to source location of the type of asset.
        /// </summary>
        public string Dir { get; set; }

        /// <summary>
        /// Base filename without extension and without directory prefix.
        /// </summary>
        public string Filename { get; set; }
    }
}
