using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public class AiVariableCommandDescription : IAiVariableCommandDescription
    {
        public string DecompName { get; set; }

        public byte CommandId { get; set; }

        public int CommandLengthBytes { get; set; }

        public byte[] CommandData { get; set; }
    }
}
