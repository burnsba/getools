using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    public interface IFlashcartPacket : IEncapsulate
    {
        void SetContent(byte[] body);

        byte[] GetInnerPacket();
        byte[] GetOuterPacket();
    }
}
