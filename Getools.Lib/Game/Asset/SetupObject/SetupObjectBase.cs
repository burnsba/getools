using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public abstract class SetupObjectBase : GameObjectHeaderBase, ISetupObject
    {
        public SetupObjectBase()
        {
        }

        public SetupObjectBase(Propdef type)
        {
            Type = type;
        }

        public Propdef Type
        {
            get
            {
                return (Propdef)TypeRaw;
            }

            set
            {
                TypeRaw = (byte)value;
            }
        }
    }
}
