using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Enums
{
    /// <summary>
    /// Reserverd values for ai script parameters.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "<Justification>")]
    public enum ChrNum
    {
        /* chr numbers start at 0. */

        CHR_BOND_CINEMA = -8,
        CHR_CLONE = -7,
        CHR_SEE_SHOT = -6,
        CHR_SEE_DIE = -5,
        CHR_PRESET = -4,
        CHR_SELF = -3,
    }
}
