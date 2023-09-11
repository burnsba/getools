using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.UsbPacket;

namespace Gebug64.Unfloader.Message
{
    public abstract class GebugMessageBase : IGebugMessage
    {
        protected Packet? _usbPacket;

        private readonly Guid _id = Guid.NewGuid();

        public Guid Id => _id;

        public CommunicationSource Source { get; set; }

        public DateTime InstantiateTime { get; set; }

        public GebugMessageBase()
        {
        }

        public GebugMessageBase(Packet usbPacket)
        {
            _usbPacket = usbPacket;
        }

        public virtual Packet GetUsbPacket()
        {
            if (object.ReferenceEquals(null, _usbPacket))
            {
                throw new NullReferenceException();
            }

            return _usbPacket;
        }
    }
}
