using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message.MessageType;
using Gebug64.Unfloader.UsbPacket;
using Getools.Lib.Game.EnumModel;
using Newtonsoft.Json.Linq;

namespace Gebug64.Unfloader.Message
{
    public class RomCheatMessage : RomMessage
    {
        private GebugCmdCheat _command;

        public GebugCmdCheat Command
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

        public RomCheatMessage()
            : base (GebugMessageCategory.Cheat, 0)
        {
        }

        public RomCheatMessage(GebugCmdCheat command)
            : base (GebugMessageCategory.Cheat, (int)command)
        {
            Command = command;
        }

        public override string ToString()
        {
            switch (Command)
            {
                case GebugCmdCheat.SetCheatStatus:
                    {
                        if (Parameters.Count > 1)
                        {
                            var p1value = Parameters[0].GetValueIntOrDefault();
                            var p2value = Parameters[1].GetValueIntOrDefault();

                            var typedValue = CheatIdX.All.FirstOrDefault(x => x.Id == p2value);

                            if (typedValue != null && typedValue.Id > 0 && p2value > 0)
                            {
                                return $"{Category} {Command} {(p1value > 0 ? "on" : "off")} {typedValue.Name}";
                            }
                        }
                    }
                    break;
            }

            return $"{Category} {Command}";
        }
    }
}
