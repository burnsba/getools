using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public interface IAiFixedCommand : IAiConcreteCommand
    {
        int NumberParameters { get; set; }

        List<IAiParameter> CommandParameters { get; set; }
    }
}
