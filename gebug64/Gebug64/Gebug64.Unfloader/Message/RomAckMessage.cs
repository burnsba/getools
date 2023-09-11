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
        public GebugMessageCategory AckCategory { get; set; }
        public int AckCommand { get; set; }

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
            switch (AckCategory)
            {
                case GebugMessageCategory.Misc:
                    {
                        RomMiscMessage.Unwrap(this, (GebugCmdMisc)AckCommand, data);
                    }
                    break;
            }
        }

        public override string ToString()
        {
            var parameters = Parameters.Take(3);

            var sb = new StringBuilder();

            var commandName = ResolveCommand(AckCategory, AckCommand);

            sb.Append($"{Category} {AckCategory} {commandName}");

            if (parameters.Any())
            {
                sb.Append(" ");
                sb.Append(string.Join(", ", parameters.Select(x => x.ToString())));
            }

            if (Parameters.Count > 3)
            {
                sb.Append(" ...");
            }

            return sb.ToString();
        }
    }
}
