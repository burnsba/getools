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
    internal class TextPacket : UnfloaderPacket
    {
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
                    _data = null;
                }
                else
                {
                    _data = Encoding.ASCII.GetBytes(value);
                }
            }
        }

        public TextPacket(byte[] data)
            : base(UnfloaderMessageType.Text, data)
        { }

        public override string ToString()
        {
            return $"size={Size}, type={MessageType}, text={Content}";
        }
    }
}
