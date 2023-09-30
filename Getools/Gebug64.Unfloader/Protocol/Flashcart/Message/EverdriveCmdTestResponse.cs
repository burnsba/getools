using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Flashcart.Message
{
    /// <summary>
    /// Everdrive response to <see cref="EverdriveCmdTestSend"/>.
    /// Direction: <see cref="ParameterUseDirection.ConsoleToPc"/>.
    /// </summary>
    public class EverdriveCmdTestResponse : EverdriveCmd
    {
        /// <summary>
        /// The Everdrive command word.
        /// </summary>
        public const string Command_Test_Response = "cmdr";

        /// <summary>
        /// Gets the entire Everdrive command packet for <see cref="EverdriveCmdTestResponse"/>.
        /// </summary>
        public static byte[] CommandBytes = System.Text.Encoding.ASCII.GetBytes(Command_Test_Response + ZeroPad12);

        /// <summary>
        /// Initializes a new instance of the <see cref="EverdriveCmdTestResponse"/> class.
        /// </summary>
        public EverdriveCmdTestResponse()
            : base(CommandBytes)
        {
        }
    }
}
