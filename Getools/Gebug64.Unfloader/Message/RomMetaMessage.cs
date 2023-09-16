using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message.CommandParameter;
using Gebug64.Unfloader.Message.MessageType;
using Gebug64.Unfloader.UsbPacket;
using Microsoft.VisualBasic;

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

        static internal void Unwrap(RomMessage self, GebugCmdMeta command, byte[] data)
        {
            switch (command)
            {
                case GebugCmdMeta.Version:
                    {
                        int offset = 2; // skip category and command
                        int val;

                        val = BitUtility.Read32Big(data, offset);
                        self.Parameters.Add(new S32Parameter(val));
                        offset += 4;

                        val = BitUtility.Read32Big(data, offset);
                        self.Parameters.Add(new S32Parameter(val));
                        offset += 4;

                        val = BitUtility.Read32Big(data, offset);
                        self.Parameters.Add(new S32Parameter(val));
                        offset += 4;

                        val = BitUtility.Read32Big(data, offset);
                        self.Parameters.Add(new S32Parameter(val));
                        offset += 4;
                    }
                    break;
            }
        }

        public Version? GetVersion()
        {
            if (Command == GebugCmdMeta.Version && Parameters.Count > 3)
            {
                return new Version(
                    Parameters[0].GetValueIntOrDefault(),
                    Parameters[1].GetValueIntOrDefault(),
                    Parameters[2].GetValueIntOrDefault(),
                    Parameters[3].GetValueIntOrDefault());
            }

            return null;
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
            switch (Command)
            {
                case GebugCmdMeta.Version:
                    {
                        var version = GetVersion();
                        if (version != null)
                        {
                            return $"{Category} {Command} {version}";
                        }
                    }
                    break;
            }

            return $"{Category} {Command}";
        }
    }
}
