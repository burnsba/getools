using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart.Message
{
    /// <summary>
    /// Base implemtentation to describe Everdrive command packet.
    /// All commands are 16 bytes long, 4 words each, and share a common syntax.
    /// </summary>
    public abstract class EverdriveCmd : EverdrivePacket
    {
        /// <summary>
        /// Most commands only care about the first word. This is just a helper
        /// value to seed the rest of the command.
        /// </summary>
        protected const string ZeroPad12 = "\0\0\0\0\0\0\0\0\0\0\0\0";

        /// <summary>
        /// Initializes a new instance of the <see cref="EverdriveCmd"/> class.
        /// </summary>
        /// <param name="data">Inner packet (body) data without header/tail protocol data.</param>
        protected EverdriveCmd(byte[] data)
            : base(data)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EverdriveCmd"/> class.
        /// </summary>
        protected EverdriveCmd()
        {
        }

        /// <summary>
        /// Check whether the byte data begins the Everdrive command protocol.
        /// </summary>
        /// <param name="data">Data to check.</param>
        /// <returns>True if command syntax is detected, false otherwise.</returns>
        public static bool IsSystemCommand(byte[] data)
        {
            if (object.ReferenceEquals(null, data))
            {
                return false;
            }

            if (data.Length >= 5 && data.Length <= 16)
            {
                if (data[0] == 'c'
                    && data[1] == 'm'
                    && data[2] == 'd'
                    ////&& data[3] == ?? // index 3 is command specific value.
                    && data[4] == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to parse a collection of bytes as an Everdrive system command.
        /// </summary>
        /// <param name="data">Bytes to evaluate for system command.</param>
        /// <returns>Parse result.</returns>
        /// <exception cref="NotImplementedException">Throw if received write ROM command from console.</exception>
        public static EverdriveCmd? ParseSystemCommand(byte[] data)
        {
            if (!IsSystemCommand(data))
            {
                return null;
            }

            if (data.Length >= 4 && data.Length <= 16)
            {
                // because the IsSystemCommand check succeeded above, we know
                // that the first three bytes are { 'c', 'm', 'd' }
                if (data[3] == EverdriveCmdTestSend.CommandBytes[3])
                {
                    // Shouldn't receive this from console.
                    var result = new EverdriveCmdTestSend();
                    result._data = data;
                    return result;
                }
                else if (data[3] == EverdriveCmdTestResponse.CommandBytes[3])
                {
                    var result = new EverdriveCmdTestResponse();
                    result._data = data;
                    return result;
                }
                else if (data[3] == EverdriveCmdWriteRom.CommandBytes[3])
                {
                    // Shouldn't receive this from console.
                    // Need to read the `chunks` parameter from the input bytes ...
                    throw new NotImplementedException();

                    /***
                    //// Something like
                    //var result = new EverdriveCmdWriteRom();
                    //result._data = data;
                    //return result;
                    */
                }
                else if (data[3] == EverdriveCmdPifboot.CommandBytes[3])
                {
                    // Shouldn't receive this from console.
                    var result = new EverdriveCmdPifboot();
                    result._data = data;
                    return result;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public override void SetContent(byte[] body)
        {
            // don't allow public set content calls
            throw new NotSupportedException("System commands should not explicitly set body content");
        }

        /// <inheritdoc />
        public override byte[] GetOuterPacket()
        {
            // don't include header+tail for system commands.
            return _data!;
        }
    }
}
