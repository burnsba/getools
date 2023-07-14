using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public class AiFixedCommandDescription : IAiFixedCommandDescription
    {
        public AiFixedCommandDescription()
        { }

        public string DecompName { get; set; }
        public byte CommandId { get; set; }
        public int CommandLengthBytes { get; set; }
        public int NumberParameters { get; set; }
        public List<IAiParameter> CommandParameters { get; set; }
    }
}
