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
    [ConsoleDescription("BadGebug4", 2)]
    public class BadGebug4 : FakeConsoleBase
    {
        public BadGebug4(ISerialPort serialPort, IFakeFlashcart flashcart)
            : base(serialPort, flashcart)
        {
        }

        protected override byte[] MutatePacket(GebugPacket packet)
        {
            var byteList = packet.ToByteArray().ToList();

            // add a few bytes on the end
            byteList.Add(0);
            byteList.Add(1);
            byteList.Add(2);

            return byteList.ToArray();
        }
    }
}
