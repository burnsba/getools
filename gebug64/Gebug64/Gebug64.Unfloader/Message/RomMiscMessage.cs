using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message.CommandParameter;
using Gebug64.Unfloader.Message.MessageType;
using Gebug64.Unfloader.UsbPacket;

namespace Gebug64.Unfloader.Message
{
    public class RomMiscMessage : RomMessage
    {
        private GebugCmdMisc _command;

        public GebugCmdMisc Command
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

        public RomMiscMessage()
            : base(GebugMessageCategory.Misc, 0)
        {
        }

        public RomMiscMessage(GebugCmdMisc command)
            : base(GebugMessageCategory.Misc, (int)command)
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

        static internal void Unwrap(RomMessage self, GebugCmdMisc command, byte[] data)
        {
            switch (command)
            {
                case GebugCmdMisc.OsTime:
                {
                    var time = BitUtility.Read32Big(data, 0);
                    self.Parameters.Add(new U32Parameter(time));
                }
                break;
            }
        }

        public override string ToString()
        {
            return $"{Category} {Command}";
        }
    }
}
