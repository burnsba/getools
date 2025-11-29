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
    [ConsoleDescription("BadGebug3", 2)]
    public class BadGebug3 : FakeConsoleBase
    {
        public BadGebug3(ISerialPort serialPort, IFakeFlashcart flashcart)
            : base(serialPort, flashcart)
        {
        }

        protected override byte[] MutatePacket(GebugPacket packet)
        {
            // truncate to only 10 bytes in length (packet size is 12 minumum)
            var bytes = packet.ToByteArray().ToList().Take(10).ToArray();

            return bytes;
        }
    }
}
