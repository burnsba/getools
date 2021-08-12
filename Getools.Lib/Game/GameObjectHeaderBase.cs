using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game
{
    public abstract class GameObjectHeaderBase : GameObjectBase
    {
        public UInt16 Scale { get; set; }

        public byte Hidden2Raw { get; set; }

        public byte TypeRaw { get; set; }
    }
}
