using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Flashcart.Message
{
    /// <summary>
    /// Everdrive command to boot the ROM that was just sent.
    /// Direction: <see cref="ParameterUseDirection.PcToConsole"/>.
    /// </summary>
    public class EverdriveCmdPifboot : EverdriveCmd
    {
        /// <summary>
        /// The Everdrive command word.
        /// </summary>
        public const string Command_Pifboot = "cmds";

        /// <summary>
        /// Gets the entire Everdrive command packet for <see cref="EverdriveCmdPifboot"/>.
        /// </summary>
        public static byte[] CommandBytes = System.Text.Encoding.ASCII.GetBytes(Command_Pifboot + ZeroPad12);

        /// <summary>
        /// Initializes a new instance of the <see cref="EverdriveCmdPifboot"/> class.
        /// </summary>
        public EverdriveCmdPifboot()
            : base(CommandBytes)
        {
        }
    }
}
