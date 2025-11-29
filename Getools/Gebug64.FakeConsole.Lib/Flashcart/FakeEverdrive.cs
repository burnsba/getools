using Gebug64.Unfloader.Protocol;
using Gebug64.Unfloader.Protocol.Flashcart;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gebug64.FakeConsole.Lib.Flashcart
{
    public class FakeEverdrive : IFakeFlashcart
    {
        public FlashcartPacketParseResult TryParse(List<byte> data)
        {
            return EverdrivePacket.TryParse(data);
        }

        public IEncapsulate PackageToSend(byte[] data)
        {
            var flashcartPacket = new EverdrivePacket(data);
            return flashcartPacket;
        }
    }
}
