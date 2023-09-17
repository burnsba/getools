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

        static internal void ParseParameters(RomMiscMessage self, GebugCmdMisc command, byte[] data, int offset)
        {
            switch (command)
            {
                case GebugCmdMisc.OsTime:
                {
                    int val;

                    val = BitUtility.Read32Big(data, offset);
                    self.Parameters.Add(new U32Parameter(val));
                    offset += 4;
                }
                break;
            }
        }

        public override string ToString()
        {
            switch (Command)
            {
                case GebugCmdMisc.OsTime:
                    {
                        if (Parameters.Count > 0)
                        {
                            return $"{Category} {Command} {Parameters[0]}";
                        }
                    }
                    break;
            }

            return $"{Category} {Command}";
        }
    }
}
