using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message.MessageType;
using Gebug64.Unfloader.UsbPacket;

namespace Gebug64.Unfloader.Message
{
    public class RomMultiAckMessage : RomAckMessage
    {
        public List<RomAckMessage> Fragments { get; set; } = new List<RomAckMessage>();

        public RomMultiAckMessage()
            : base ()
        {
        }

        public void UnwrapFragments()
        {
            // Each packet from the console will have the RomAckMessage header,
            // but want to parse this on the PC side as one continuous packet.
            // Therefore, skip the header on all but the first packet.
            bool skip = false;

            var bytes = new List<IEnumerable<byte>>();
            var orderedFragrments = Fragments.OrderBy(x => x.PacketNumber);
            foreach (var f in orderedFragrments)
            {
                if (skip)
                {
                    if (f.FragmentBytes.Length > AckProtocolHeaderSize)
                    {
                        bytes.Add(f.FragmentBytes.Skip(AckProtocolHeaderSize));
                    }
                }
                else
                {
                    bytes.Add(f.FragmentBytes);
                    skip = true;
                }
            }

            var flatBytes = bytes.SelectMany(x => x).ToArray();

            Unwrap(flatBytes);
        }
    }
}
