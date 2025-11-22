using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    /// <summary>
    /// Request setGrenadeChance(x).
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Misc, Command = (byte)GebugCmdMisc.SetGrenadeChance)]
    public class GebugMiscGrenadeChanceMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugMiscGrenadeChanceMessage"/> class.
        /// </summary>
        public GebugMiscGrenadeChanceMessage()
          : base()
        {
        }

        /// <summary>
        /// Send config option to console.
        /// 0 = use system default
        /// 1 = grenade roll always fails
        /// 2 = grenade roll always succeeds
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 1, UseDirection = ParameterUseDirection.PcToConsole)]
        public byte Option { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            string grenadeText = string.Empty;
            if (Option == 1)
            {
                grenadeText = "0%";
            }
            else if (Option == 2)
            {
                grenadeText = "100%";
            }
            else
            {
                grenadeText = "default";
            }

            return $"{Category} {DebugCommand} {grenadeText}";
        }
    }
}
