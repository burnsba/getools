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
        protected IPacket? _usbPacket;

        private readonly Guid _id = Guid.NewGuid();

        public Guid Id => _id;

        public CommunicationSource Source { get; set; }

        public DateTime InstantiateTime { get; init; } = DateTime.Now;

        public bool PacketDataSet { get; private set; }

        public GebugMessageBase()
        {
            PacketDataSet = false;
        }

        public GebugMessageBase(IPacket usbPacket)
        {
            _usbPacket = usbPacket;
            PacketDataSet = true;
        }

        public GebugMessageBase(GebugMessageBase copy)
        {
            _usbPacket = copy._usbPacket;
            PacketDataSet = !object.ReferenceEquals(null, _usbPacket);
            InstantiateTime = copy.InstantiateTime;
            Source = copy.Source;
            // don't copy id.
        }

        public virtual IPacket GetUsbPacket()
        {
            if (object.ReferenceEquals(null, _usbPacket))
            {
                throw new NullReferenceException($"{nameof(GetUsbPacket)} : {nameof(_usbPacket)}");
            }

            return _usbPacket;
        }

        public virtual void SetUsbPacket(IPacket packet)
        {
            _usbPacket = packet;
            PacketDataSet = true;
        }

        public virtual byte[] ToSendData()
        {
            // this can throw null reference exception
            return GetUsbPacket().GetInnerData();
        }
    }
}
