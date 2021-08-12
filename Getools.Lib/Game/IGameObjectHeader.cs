using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game
{
    public interface IGameObjectHeader : IGameObject
    {
        UInt16 Scale { get; set; }

        byte Hidden2Raw { get; set; }

        byte TypeRaw { get; set; }
    }
}
