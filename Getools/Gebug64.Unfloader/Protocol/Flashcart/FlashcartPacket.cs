using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    /// <summary>
    /// Abstract flashcart packet.
    /// Flashcart packet is the lowest level of communication between PC and gebug romhack.
    /// </summary>
    public abstract class FlashcartPacket : IFlashcartPacket
    {
        /// <summary>
        /// Inner packet (body) data without header/tail protocol data.
        /// </summary>
        protected byte[]? _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashcartPacket"/> class.
        /// </summary>
        public FlashcartPacket()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashcartPacket"/> class.
        /// </summary>
        /// <param name="data">Inner packet (body) data without header/tail protocol data.</param>
        public FlashcartPacket(byte[] data)
        {
            _data = data;
        }

        /// <inheritdoc />
        public Type? InnerType { get; set; }

        /// <inheritdoc />
        public object? InnerData { get; set; }

        /// <inheritdoc />
        public byte[] GetInnerPacket()
        {
            if (object.ReferenceEquals(null, _data))
            {
                throw new NullReferenceException($"Body content not set");
            }

            return _data;
        }

        /// <inheritdoc />
        public virtual byte[] GetOuterPacket()
        {
            if (object.ReferenceEquals(null, _data))
            {
                throw new NullReferenceException($"Body content not set");
            }

            return _data;
        }

        /// <inheritdoc />
        public virtual void SetContent(byte[] body)
        {
            _data = body;
        }
    }
}
