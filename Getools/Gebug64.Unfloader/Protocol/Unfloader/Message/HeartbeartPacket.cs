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
    /// UNFLoader heartbeat/version/ping packet.
    /// </summary>
    public class HeartbeartPacket : UnfloaderPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeartbeartPacket"/> class.
        /// </summary>
        /// <param name="data">Inner packet (body) data without header/tail protocol data.</param>
        public HeartbeartPacket(byte[] data)
            : base(UnfloaderMessageType.HeartBeat, data)
        {
        }

        /// <summary>
        /// UNFLoader USB library protocol version.
        /// 2 bytes.
        /// </summary>
        public int UsbProtocolVersion
        {
            get
            {
                if (object.ReferenceEquals(null, _data) || _data.Count() < 2)
                {
                    return 0;
                }

                int val = _data[0] << 8;
                val |= _data[1];

                return val;
            }
        }

        /// <summary>
        /// UNFLoader heartbeat packet version.
        /// 2 bytes.
        /// </summary>
        public int HeartbeatVersion
        {
            get
            {
                if (object.ReferenceEquals(null, _data) || _data.Count() < 4)
                {
                    return 0;
                }

                int val = _data[2] << 8;
                val |= _data[3];

                return val;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"size={Size}, type={MessageType}, protocol={UsbProtocolVersion}, heartbeat={HeartbeatVersion}";
        }
    }
}
