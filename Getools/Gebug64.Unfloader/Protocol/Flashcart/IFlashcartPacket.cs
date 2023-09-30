using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    /// <summary>
    /// Interface to define flashcart packet.
    /// Flashcart packet is the lowest level of communication between PC and gebug romhack.
    /// </summary>
    public interface IFlashcartPacket : IEncapsulate
    {
        /// <summary>
        /// Sets the inner content of the packet, without any header/tail protocol data.
        /// </summary>
        /// <param name="body">Body data.</param>
        void SetContent(byte[] body);

        /// <summary>
        /// Gets the inner content of the packet, without any header/tail protocol data.
        /// </summary>
        /// <returns>Data.</returns>
        byte[] GetInnerPacket();

        /// <summary>
        /// Gets the entire packet, including header/tail protocol data.
        /// </summary>
        /// <returns>Data.</returns>
        byte[] GetOuterPacket();
    }
}
