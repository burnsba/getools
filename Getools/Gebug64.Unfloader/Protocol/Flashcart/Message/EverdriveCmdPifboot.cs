using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart.Message
{
    public class EverdriveCmdPifboot : EverdriveCmd
    {
        public const string Command_Pifboot = "cmds";
        public static byte[] CommandBytes = System.Text.Encoding.ASCII.GetBytes(Command_Pifboot + ZeroPad12);

        public EverdriveCmdPifboot()
            : base(CommandBytes)
        { }
    }
}
