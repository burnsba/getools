using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectSetGuardAttribute : SetupObjectBase, ISetupObject
    {
        public SetupObjectSetGuardAttribute()
            : base(Propdef.Guard)
        {
        }

        public uint GuardId { get; set; }
        public int Attribute { get; set; }
    }
}
