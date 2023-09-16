using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message.MessageType;
using Gebug64.Unfloader.UsbPacket;

namespace Gebug64.Unfloader.Message
{
    public class RomAckMessage : RomMessage
    {
        public RomMessage Reply { get; set; }

        public GebugMessageCategory AckCategory { get; set; }
        //public int AckCommand { get; set; }

        public RomAckMessage()
            : base (GebugMessageCategory.Ack, 0)
        {
        }

        public override Packet GetUsbPacket()
        {
            if (object.ReferenceEquals(null, _usbPacket))
            {
                _usbPacket = new Packet(PacketType.Binary, ToSendData());
            }

            return _usbPacket;
        }

        public void Unwrap(byte[] data)
        {
            var category = (GebugMessageCategory)data[0];
            int command = (int)data[1];
            switch (category)
            {
                case GebugMessageCategory.Meta:
                    {
                        Reply = new RomMetaMessage((GebugCmdMeta)command);
                        RomMetaMessage.Unwrap(Reply, (GebugCmdMeta)command, data);
                    }
                    break;

                case GebugMessageCategory.Misc:
                    {
                        Reply = new RomMiscMessage((GebugCmdMisc)command);
                        RomMiscMessage.Unwrap(Reply, (GebugCmdMisc)command, data);
                    }
                    break;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (!object.ReferenceEquals(null, Reply))
            {
                var replyString = Reply.ToString();

                sb.Append($"{Category} {replyString}");
            }
            else
            {
                sb.Append($"{Category} {AckCategory}");
            }

            return sb.ToString();
        }
    }
}
