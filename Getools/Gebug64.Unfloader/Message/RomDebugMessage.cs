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
    public class RomDebugMessage : RomMessage
    {
        private GebugCmdDebug _command;

        public GebugCmdDebug Command
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

        public RomDebugMessage()
            : base (GebugMessageCategory.Debug, 0)
        {
        }

        public RomDebugMessage(GebugCmdDebug command)
            : base (GebugMessageCategory.Debug, (int)command)
        {
            Command = command;
        }

        public override string ToString()
        {
            switch (Command)
            {
                case GebugCmdDebug.ShowDebugMenu:
                    {
                        if (Parameters.Count > 0)
                        {
                            var pvalue = Parameters[0].GetValueIntOrDefault();
                            return $"{Category} {Command} {(pvalue > 0 ? "on" : "off")}";
                        }
                    }
                    break;

                case GebugCmdDebug.DebugMenuProcessor:
                    {
                        if (Parameters.Count > 0)
                        {
                            var pvalue = Parameters[0].GetValueIntOrDefault();
                            var typedValue = DebugMenuCommandX.ValidCommands.FirstOrDefault(x => x.Id == pvalue);
                            if (typedValue != null && typedValue.Id > 0 && pvalue > 0)
                            {
                                return $"{Category} {Command} {typedValue.Name}";
                            }
                        }
                    }
                    break;
            }

            return $"{Category} {Command}";
        }
    }
}
