using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart.Message
{
    public class EverdriveCmdTestSend : EverdriveCmd
    {
        public const string Command_Test_Send = "cmdt";
        public static byte[] CommandBytes = System.Text.Encoding.ASCII.GetBytes(Command_Test_Send + ZeroPad12);

        public EverdriveCmdTestSend()
            : base(CommandBytes)
        { }
    }
}
