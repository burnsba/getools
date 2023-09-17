using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.UsbPacket;

namespace Gebug64.Unfloader.Message
{
    public interface IGebugMessage
    {
        Guid Id { get; }

        DateTime InstantiateTime { get; }

        CommunicationSource Source { get; }

        IPacket GetUsbPacket();

        void SetUsbPacket(IPacket packet);

        bool PacketDataSet { get; }

        /// <summary>
        /// Message data to byte array, without any protocol data.
        /// </summary>
        /// <returns></returns>
        byte[] ToSendData();
    }
}
