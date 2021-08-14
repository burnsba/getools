using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Base class for setup object definitions.
    /// </summary>
    public abstract class SetupObjectBase : GameObjectHeaderBase, ISetupObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectBase"/> class.
        /// </summary>
        /// <param name="type">Type of object.</param>
        public SetupObjectBase(PropDef type)
        {
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectBase"/> class.
        /// Should only be used for serialization purposes.
        /// </summary>
        internal SetupObjectBase()
        {
        }

        /// <inheritdoc/>
        public PropDef Type
        {
            get
            {
                return (PropDef)TypeRaw;
            }

            set
            {
                TypeRaw = (byte)value;
            }
        }
    }
}
