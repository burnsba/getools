using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Engine
{
    /// <summary>
    /// Prop position in scaled units with bounding box and rotation.
    /// </summary>
    public class RuntimePropPosition : RuntimePosition
    {
        public PropPointPosition Source { get; set; }
    }
}
