using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    public class EverdrivePacket : FlashcartPacket
    {
        public EverdrivePacket()
        { }

        public EverdrivePacket(byte[] data)
            : base(data)
        {
        }

        public override byte[] GetOuterPacket()
        {
            if (object.ReferenceEquals(null, _data))
            {
                throw new NullReferenceException($"Body content not set");
            }

            var toSend = new List<byte>
            {
                (byte)'D',
                (byte)'M',
                (byte)'A',
                (byte)'@'
            };

            toSend.AddRange(_data);

            toSend.Add((byte)'C');
            toSend.Add((byte)'M');
            toSend.Add((byte)'P');
            toSend.Add((byte)'H');

            return toSend.ToArray();
        }
    }
}
