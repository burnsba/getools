using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Flashcart.Message
{
    /// <summary>
    /// Everdrive "test" command ~ ping.
    /// Direction: <see cref="ParameterUseDirection.PcToConsole"/>.
    /// </summary>
    public class EverdriveCmdTestSend : EverdriveCmd
    {
        /// <summary>
        /// The Everdrive command word.
        /// </summary>
        public const string Command_Test_Send = "cmdt";

        /// <summary>
        /// Gets the entire Everdrive command packet for <see cref="EverdriveCmdTestSend"/>.
        /// </summary>
        public static byte[] CommandBytes = System.Text.Encoding.ASCII.GetBytes(Command_Test_Send + ZeroPad12);

        /// <summary>
        /// Initializes a new instance of the <see cref="EverdriveCmdTestSend"/> class.
        /// </summary>
        public EverdriveCmdTestSend()
            : base(CommandBytes)
        {
        }
    }
}
