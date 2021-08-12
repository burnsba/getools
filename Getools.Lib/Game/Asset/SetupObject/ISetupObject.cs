using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public interface ISetupObject : IGameObjectHeader
    {
        Propdef Type { get; set; }
    }
}
