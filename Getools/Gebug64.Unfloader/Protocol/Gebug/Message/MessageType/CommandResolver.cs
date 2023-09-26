using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    public class CommandResolver
    {
        public static string ResolveCommand(GebugMessageCategory category, int command)
        {
            switch (category)
            {
                case GebugMessageCategory.Cheat: return ((GebugCmdCheat)command).ToString();
                case GebugMessageCategory.Debug: return ((GebugCmdDebug)command).ToString();
                case GebugMessageCategory.Meta: return ((GebugCmdMeta)command).ToString();
                case GebugMessageCategory.Misc: return ((GebugCmdMisc)command).ToString();
                case GebugMessageCategory.Stage: return ((GebugCmdStage)command).ToString();
                case GebugMessageCategory.Vi: return ((GebugCmdVi)command).ToString();
            }

            throw new NotImplementedException();
        }
    }
}
