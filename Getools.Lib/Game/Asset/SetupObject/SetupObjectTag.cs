using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectTag : SetupObjectBase, ISetupObject
    {
        public SetupObjectTag()
            : base(Propdef.Tag)
        {
        }

        public ushort TagId { get; set; }
        public ushort Value { get; set; }
        public uint Unknown_04 { get; set; }
        public uint Unknown_08 { get; set; }
    }
}
