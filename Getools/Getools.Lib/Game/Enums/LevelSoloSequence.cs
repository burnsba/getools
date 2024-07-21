using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Enums
{
    /// <summary>
    /// typedef enum LEVEL_SOLO_SEQUENCE.
    ///
    /// No credits stage.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "<Justification>")]
    public enum LevelSoloSequence
    {
        DefaultUnknown = -1,

        Dam = 0,
        Facility,
        Runway,
        Surface1,
        Bunker1,
        Silo,
        Frigate,
        Surface2,
        Bunker2,
        Statue,
        Archives,
        Streets,
        Depot,
        Train,
        Jungle,
        Control,
        Caverns,
        Cradle,
        Aztec,
        Egypt,

        Max = 20,
    }
}
