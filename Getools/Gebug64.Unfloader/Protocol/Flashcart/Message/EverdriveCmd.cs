using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart.Message
{
    public abstract class EverdriveCmd : EverdrivePacket
    {
        protected const string ZeroPad12 = "\0\0\0\0\0\0\0\0\0\0\0\0";

        protected EverdriveCmd(byte[] data)
            : base(data)
        {
        }

        protected EverdriveCmd()
        { }

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
                    //&& data[3] == ??
                    && data[4] == 0)
                {
                    return true;
                }
            }

            return false;
        }

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
                    // Need to read the `chunks` parameter from the input bytes ...
                    throw new NotSupportedException();

                    //var result = new EverdriveCmdWriteRom();
                    //result._data = data;
                    //return result;
                }
                else if (data[3] == EverdriveCmdPifboot.CommandBytes[3])
                {
                    var result = new EverdriveCmdPifboot();
                    result._data = data;
                    return result;
                }
            }

            return null;
        }

        public override void SetContent(byte[] body)
        {
            // don't allow public set content calls
            throw new NotSupportedException("System commands should not explicitly set body content");
        }
    }
}
