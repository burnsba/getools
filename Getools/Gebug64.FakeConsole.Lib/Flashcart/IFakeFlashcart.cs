using Gebug64.Unfloader.Protocol;
using Gebug64.Unfloader.Protocol.Flashcart;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gebug64.FakeConsole.Lib.Flashcart
{
    public interface IFakeFlashcart
    {
        FlashcartPacketParseResult TryParse(List<byte> data);

        IEncapsulate PackageToSend(byte[] data);
    }
}
