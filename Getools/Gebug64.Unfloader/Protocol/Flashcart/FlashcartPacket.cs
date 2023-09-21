using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    public abstract class FlashcartPacket : IFlashcartPacket
    {
        protected byte[] _data;

        public FlashcartPacket()
        { }

        public FlashcartPacket(byte[] data)
        {
            _data = data;
        }

        public byte[] GetInnerPacket()
        {
            if (object.ReferenceEquals(null, _data))
            {
                throw new NullReferenceException($"Body content not set");
            }

            return _data;
        }

        public virtual byte[] GetOuterPacket()
        {
            if (object.ReferenceEquals(null, _data))
            {
                throw new NullReferenceException($"Body content not set");
            }

            return _data;
        }

        public void SetContent(byte[] body)
        {
            _data = body;
        }
    }
}
