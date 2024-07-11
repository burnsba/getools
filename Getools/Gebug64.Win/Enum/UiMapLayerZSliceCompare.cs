using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Enum
{
    /// <summary>
    /// WPF helper enum, to explain how map objects in a layer should be compared to the Z min/max range.
    /// </summary>
    public enum UiMapLayerZSliceCompare
    {
        /// <summary>
        /// Compare to the origin point of the object.
        /// </summary>
        OriginPoint,

        /// <summary>
        /// Compare to min/max of the bounding box of the object.
        /// </summary>
        Bbox,
    }
}
