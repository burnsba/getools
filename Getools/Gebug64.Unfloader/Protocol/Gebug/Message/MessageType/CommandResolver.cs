using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Helper class to convert command information.
    /// </summary>
    public class CommandResolver
    {
        /// <summary>
        /// Converts category+command into friendly command string.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="command">Command in category.</param>
        /// <returns>Name of command.</returns>
        /// <exception cref="NotImplementedException">Throw if command is not found in category.</exception>
        public static string ResolveCommand(GebugMessageCategory category, int command)
        {
            switch (category)
            {
                case GebugMessageCategory.Cheat: return ((GebugCmdCheat)command).ToString();
                case GebugMessageCategory.Debug: return ((GebugCmdDebug)command).ToString();
                case GebugMessageCategory.Meta: return ((GebugCmdMeta)command).ToString();
                case GebugMessageCategory.Misc: return ((GebugCmdMisc)command).ToString();
                case GebugMessageCategory.Ramrom: return ((GebugCmdRamrom)command).ToString();
                case GebugMessageCategory.Stage: return ((GebugCmdStage)command).ToString();
                case GebugMessageCategory.Vi: return ((GebugCmdVi)command).ToString();
            }

            throw new NotImplementedException();
        }
    }
}
