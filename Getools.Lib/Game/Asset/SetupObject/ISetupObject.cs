using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Interface for object/prop definitions used in setup.
    /// </summary>
    public interface ISetupObject : IGameObjectHeader, IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// Gets or sets object type.
        /// </summary>
        PropDef Type { get; set; }
    }
}
