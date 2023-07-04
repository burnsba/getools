using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public interface IHasPreset
    {
        ushort Preset { get; set; }
    }
}
