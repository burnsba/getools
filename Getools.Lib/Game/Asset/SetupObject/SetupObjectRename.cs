using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectRename : SetupObjectBase, ISetupObject
    {
        public SetupObjectRename()
            : base(Propdef.Rename)
        {
        }

        public uint ObjectOffset { get; set; }
        public uint InventoryId { get; set; }
        public uint Text1 { get; set; }
        public uint Text2 { get; set; }
        public uint Text3 { get; set; }
        public uint Text4 { get; set; }
        public uint Text5 { get; set; }
        public uint Unknown20 { get; set; }
        public uint Unknown24 { get; set; }
    }
}
