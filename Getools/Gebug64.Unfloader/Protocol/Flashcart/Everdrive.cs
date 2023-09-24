using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Flashcart.Message;
using Gebug64.Unfloader.Protocol.Parse;
using Gebug64.Unfloader.Protocol.Unfloader;
using Gebug64.Unfloader.Protocol.Unfloader.Message;
using Gebug64.Unfloader.SerialPort;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    public class Everdrive : Flashcart
    {
        private const int WriteRomTargetAddress = 0x10000000;

        private const string Command_Test_Send = "cmdt";
        private const string Command_Test_Response = "cmdr";
        private const string Command_WriteRom = "cmdW";
        private const string Command_PifBoot_Send = "cmds";

        public Everdrive(SerialPortProvider portProvider)
          : base(portProvider)
        {
        }

        protected override FlashcartPacketParseResult TryParse(List<byte> data)
        {
            return EverdrivePacket.TryParse(data);
        }

        protected override IFlashcartPacket MakePacket(byte[] data)
        {
            return new EverdrivePacket(data);
        }
    }
}
