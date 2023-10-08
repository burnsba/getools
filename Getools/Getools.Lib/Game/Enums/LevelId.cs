using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Enums
{
    /// <summary>
    /// typedef enum LEVELID
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "<Justification>")]
    public enum LevelId
    {
        None = -1,
        DefaultUnknown = 0,

        Bunker1 = 9,

        Silo = 20,

        Statue = 22,
        Control,
        Archives,
        Train,
        Frigate,
        Bunker2,
        Aztec,
        Streets,
        Depot,
        Complex,
        Egypt,
        Dam,
        Facility,
        Runway,
        Surface,
        Jungle,
        Temple,
        Caverns,
        Citadel,
        Cradle,
        Sho,
        Surface2,
        Eld,
        Basement,
        Stack,
        Lue,
        Library,
        Rit,
        Caves,
        Ear,
        Lee,
        Lip,
        Cuba,
        Wax,
        Pam,
        Max,
        Title = 90,

        Bunker2_MP = Bunker2 + 400,
        Archives_MP = Archives + 400,
        Caverns_MP = Caverns + 400,
        Facility_MP = Facility + 400,
        Egypt_MP = Egypt + 400,
    }
}
