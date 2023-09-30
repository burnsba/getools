using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    /// <summary>
    /// Describes physical flashcart device.
    /// </summary>
    public interface IFlashcart : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether there is an active connection to the flashcart.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the public collection of packets that have been received from the flashcart.
        /// </summary>
        ConcurrentQueue<IFlashcartPacket> ReadPackets { get; }

        /// <summary>
        /// Gets a value indicating the amount of time since any data was received
        /// from the flashcart device.
        /// </summary>
        TimeSpan SinceDataReceived { get; }

        /// <summary>
        /// Gets a value indicating the amount of time since a well formed <see cref="IFlashcartPacket"/>
        /// was received from the flashcart device.
        /// </summary>
        TimeSpan SinceFlashcartPacketReceived { get; }

        /// <summary>
        /// Connects the flashcart to the given port.
        /// </summary>
        /// <param name="port">Serial port to connect to.</param>
        void Connect(string port);

        /// <summary>
        /// Gracefully disconnect the flashcart from the
        /// associated serial port.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends binary data to flashcart to load and boot/run.
        /// </summary>
        /// <param name="filedata">ROM contents. It is assumed this is already in correct endieness.</param>
        /// <param name="token">Optional cancellation token.</param>
        void SendRom(byte[] filedata, CancellationToken? token = null);

        /// <summary>
        /// Public method to send raw data to the flashcart. This will be wrapped in <see cref="IFlashcartPacket"/>.
        /// </summary>
        /// <param name="data">Data to send.</param>
        void Send(byte[] data);

        /// <summary>
        /// Sends a packet to the attached flashcart.
        /// </summary>
        /// <param name="packet">Packet to send.</param>
        void Send(IFlashcartPacket packet);

        /// <summary>
        /// Executes flashcart specific test to determine whether the connection is currently
        /// in the flashcart menu.
        /// </summary>
        /// <returns>True if a valid flashcart level response is received, false otherwise.</returns>
        /// <remarks>
        /// Used to test connection level.
        /// </remarks>
        bool TestInMenu();
    }
}
