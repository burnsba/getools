using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart.Message
{
    public class EverdriveCmdWriteRom : EverdriveCmd
    {
        public const string Command_WriteRom = "cmdW";
        public static byte[] CommandBytes = System.Text.Encoding.ASCII.GetBytes(Command_WriteRom + ZeroPad12);

        public EverdriveCmdWriteRom()
            : base(CommandBytes)
        { }
    }
}
