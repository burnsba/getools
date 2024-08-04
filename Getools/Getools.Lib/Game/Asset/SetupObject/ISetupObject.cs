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

        /// <summary>
        /// Index into the setup section (padlist, propdeflist, ailist, etc).
        /// </summary>
        int SetupIndex { get; set; }

        /// <summary>
        /// Index in subsection for setup section that has subtypes (guard index, prop index, etc).
        /// </summary>
        int SetupSectionTypeIndex { get; set; }
    }
}
