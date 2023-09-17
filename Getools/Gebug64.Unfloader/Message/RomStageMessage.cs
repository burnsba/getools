using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message.CommandParameter;
using Gebug64.Unfloader.Message.MessageType;
using Gebug64.Unfloader.UsbPacket;
using Getools.Lib.Game.EnumModel;

namespace Gebug64.Unfloader.Message
{
    public class RomStageMessage : RomMessage
    {
        private GebugCmdStage _command;

        public GebugCmdStage Command
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

        public RomStageMessage()
            : base (GebugMessageCategory.Stage, 0)
        {
        }

        public RomStageMessage(GebugCmdStage command)
            : base (GebugMessageCategory.Stage, (int)command)
        {
            Command = command;
        }

        public override string ToString()
        {
            if (Command == GebugCmdStage.SetStage && Parameters.Count > 0)
            {
                var pvalue = Parameters[0].GetValueIntOrDefault();
                var value = LevelIdX.SinglePlayerStages.FirstOrDefault(x => x.Id == pvalue);

                if (value != null && value.Id > 0 && pvalue > 0)
                {
                    return $"{Category} {Command} {value.Name}";
                }
                else
                {
                    return $"{Category} {Command} unknown";
                }
            }
            else
            {
                return $"{Category} {Command}";
            }
        }
    }
}
