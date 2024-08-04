using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;

namespace Gebug64.Win.Game
{
    /// <summary>
    /// Related / backer object that the <see cref="ViewModels.Map.MapObject"/> is drawing.
    /// </summary>
    public class GameObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameObject"/> class.
        /// </summary>
        public GameObject()
        {
        }

        /// <summary>
        /// Optional setup object propdef type.
        /// </summary>
        public PropDef? PropDefType { get; set; } = null;

        /// <summary>
        /// Associated "primary key" for the <see cref="Getools.Lib.Game.Enums.PropDef"/> type of object in the setup file.
        /// </summary>
        public int LayerInstanceId { get; set; } = -1;

        /// <summary>
        /// The index of the <see cref="Getools.Lib.Game.Enums.PropDef"/> type of object in the setup file.
        /// </summary>
        public int LayerIndexId { get; set; } = -1;
    }
}
