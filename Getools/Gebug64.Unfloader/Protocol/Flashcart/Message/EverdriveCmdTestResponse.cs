using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart.Message
{
    public class EverdriveCmdTestResponse : EverdriveCmd
    {
        public const string Command_Test_Response = "cmdr";
        public static byte[] CommandBytes = System.Text.Encoding.ASCII.GetBytes(Command_Test_Response + ZeroPad12);

        public EverdriveCmdTestResponse()
            : base(CommandBytes)
        { }
    }
}
