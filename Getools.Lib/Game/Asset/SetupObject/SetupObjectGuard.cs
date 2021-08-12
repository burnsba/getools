using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectGuard : SetupObjectBase, ISetupObject
    {
        public SetupObjectGuard()
            : base(Propdef.Guard)
        {
        }

        public ushort ObjectId { get; set; }
        public ushort Preset { get; set; }
        public ushort BodyId { get; set; }
        public ushort ActionPathAssignment { get; set; }
        public uint PresetToTrigger { get; set; }
        public ushort Unknown10 { get; set; }
        public ushort Health { get; set; }
        public ushort ReactionTime { get; set; }
        public ushort Head { get; set; }
        public uint PointerRuntimeData { get; set; }
    }
}
