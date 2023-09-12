using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public interface IAiCommandDescription
    {
        string DecompName { get; set; }

        byte CommandId { get; set; }

        int CommandLengthBytes { get; set; }
    }
}
