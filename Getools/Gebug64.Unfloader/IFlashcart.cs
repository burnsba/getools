using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message;

namespace Gebug64.Unfloader
{
    /// <summary>
    /// Interface to define physical device.
    /// The device should syncrhonously accept messages, and then asynchronously queue
    /// responses as received. This object implmentation should handle translating
    /// high level messages into device specific messages.
    /// </summary>
    public interface IFlashcart : IDisposable
    {
        /// <summary>
        /// Gets the time since the last data was received from the device.
        /// </summary>
        TimeSpan SinceDataReceived { get; }

        /// <summary>
        /// Gets the time since the last complete message was received from the device.
        /// </summary>
        TimeSpan SinceRomMessageReceived { get; }

        /// <summary>
        /// Gets messages received from the device. Messages from console are expected to be <see cref="PendingGebugMessage"/>.
        /// </summary>
        ConcurrentQueue<IGebugMessage> MessagesFromConsole { get; }

        /// <summary>
        /// Initialize runtime object.
        /// </summary>
        /// <param name="portName">Serial port to communicate with device.</param>
        void Init(string portName);

        /// <summary>
        /// Send a high level message to the device.
        /// </summary>
        /// <param name="message">Message to send.</param>
        void Send(IGebugMessage message);

        /// <summary>
        /// Disconnect from the device.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Send a ROM to the device, boot and start running it.
        /// </summary>
        /// <param name="filedata">Byte array containing file data in the correct format.</param>
        /// <param name="token">Cancellation token.</param>
        void SendRom(byte[] filedata, Nullable<CancellationToken> token = null);
    }
}
