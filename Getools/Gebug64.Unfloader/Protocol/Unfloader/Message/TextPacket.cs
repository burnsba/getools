using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gebug64.Unfloader.Protocol.Unfloader.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Unfloader.Message
{
    /// <summary>
    /// UNFLoader text packet. This is the format used for `osSyncPrintf` and similar
    /// calls from the ROM, outside of gebug protocol spec.
    /// </summary>
    public class TextPacket : UnfloaderPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextPacket"/> class.
        /// </summary>
        /// <param name="data">Inner packet (body) data without header/tail protocol data.</param>
        public TextPacket(byte[] data)
            : base(UnfloaderMessageType.Text, data)
        {
        }

        /// <summary>
        /// Gets or sets string content of the packet.
        /// </summary>
        public string? Content
        {
            get
            {
                if (!object.ReferenceEquals(null, _data))
                {
                    return Encoding.ASCII.GetString(_data);
                }

                return null;
            }

            set
            {
                if (value == null)
                {
                    _data = new byte[0];
                }
                else
                {
                    //// TODO: need to restrict this to less than the size of the romhack USB buffer ...
                    _data = Encoding.ASCII.GetBytes(value);
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"size={Size}, type={MessageType}, text={Content}";
        }
    }
}
