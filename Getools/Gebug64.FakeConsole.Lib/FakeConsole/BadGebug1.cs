using Gebug64.FakeConsole.Lib.Flashcart;
using Gebug64.FakeConsole.Lib.SerialPort;
using Gebug64.Unfloader.Protocol;
using Gebug64.Unfloader.Protocol.Flashcart;
using Gebug64.Unfloader.Protocol.Flashcart.Message;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Unfloader;
using Gebug64.Unfloader.Protocol.Unfloader.Message;
using Gebug64.Unfloader.SerialPort;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Gebug64.FakeConsole.Lib.FakeConsole
{
    [ConsoleDescription("BadGebug1", 2)]
    public class BadGebug1 : FakeConsoleBase
    {
        public BadGebug1(ISerialPort serialPort, IFakeFlashcart flashcart)
            : base(serialPort, flashcart)
        {
        }

        protected override byte[] MutatePacket(GebugPacket packet)
        {
            var bytes = packet.ToByteArray();

            bytes[4] += 10; // increase size by 10 bytes

            return bytes;
        }
    }
}
