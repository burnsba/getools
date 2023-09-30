using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;
using Getools.Lib;

namespace Gebug64.Unfloader.Protocol.Flashcart.Message
{
    /// <summary>
    /// Sends the command to begin uploading a ROM to Everdrive.
    /// Will need to be followed with <see cref="EverdriveCmdPifboot"/> to begin executing.
    /// Direction: <see cref="ParameterUseDirection.PcToConsole"/>.
    /// </summary>
    public class EverdriveCmdWriteRom : EverdriveCmd
    {
        /// <summary>
        /// Everdrive expects incoming ROM to be written to this address.
        /// </summary>
        private const int WriteRomTargetAddress = 0x10000000;

        /// <summary>
        /// The Everdrive command word.
        /// </summary>
        public const string Command_WriteRom = "cmdW";

        /// <summary>
        /// Gets the entire Everdrive command packet for <see cref="EverdriveCmdWriteRom"/>.
        /// </summary>
        public static byte[] CommandBytes = System.Text.Encoding.ASCII.GetBytes(Command_WriteRom + ZeroPad12);

        /// <summary>
        /// Initializes a new instance of the <see cref="EverdriveCmdWriteRom"/> class.
        /// </summary>
        /// <param name="bytesToSend">Total number of bytes to send. Will be converted into total number of 512 bytes chunks.</param>
        public EverdriveCmdWriteRom(int bytesToSend)
        {
            _data = (byte[])CommandBytes.Clone();

            // Second command parameter is the target address. This is constant.
            BitUtility.Insert32Big(_data, 4, WriteRomTargetAddress);

            var chunks = bytesToSend / 512;

            // Third command parameter is the number of 512 byte chunks that will be written.
            BitUtility.Insert32Big(_data, 8, chunks);
        }
    }
}
