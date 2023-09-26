using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib;

namespace Gebug64.Unfloader.Protocol.Flashcart.Message
{
    public class EverdriveCmdWriteRom : EverdriveCmd
    {
        private const int WriteRomTargetAddress = 0x10000000;

        public const string Command_WriteRom = "cmdW";
        public static byte[] CommandBytes = System.Text.Encoding.ASCII.GetBytes(Command_WriteRom + ZeroPad12);

        public EverdriveCmdWriteRom(int bytesToSend)
        {
            _data = (byte[])CommandBytes.Clone();

            BitUtility.Insert32Big(_data, 4, WriteRomTargetAddress);

            var chunks = bytesToSend / 512;

            BitUtility.Insert32Big(_data, 8, chunks);
        }
    }
}
