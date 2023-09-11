using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message.MessageType;
using Gebug64.Unfloader.UsbPacket;

namespace Gebug64.Unfloader.Message
{
    public class RomMetaMessage : RomMessage
    {
        private GebugCmdMeta _command;

        public GebugCmdMeta Command
        {
            get
            {
                return _command;
            }

            set
            {
                _command = value;
                RawCommand = (int)value;
            }
        }

        public RomMetaMessage()
            : base (GebugMessageCategory.Meta, 0)
        {
        }

        public RomMetaMessage(GebugCmdMeta command)
            : base (GebugMessageCategory.Meta, (int)command)
        {
            Command = command;
        }

        public override Packet GetUsbPacket()
        {
            if (object.ReferenceEquals(null, _usbPacket))
            {
                _usbPacket = new Packet(PacketType.Binary, ToSendData());
            }

            return _usbPacket;
        }

        public override string ToString()
        {
            return $"{Category} {Command}";
        }
    }
}
