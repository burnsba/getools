
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
    /// The device manager is the runtime object used to manage communication
    /// with a device. This will start a worker thread to send and receive messages.
    /// The worker thread is responsible for sending and receiving messages.
    /// </summary>
    public interface IDeviceManager : IDisposable
    {
        /// <summary>
        /// Gets the underlying device.
        /// </summary>
        IFlashcart? Flashcart { get; }

        /// <summary>
        /// Gets the <see cref="Flashcart"/> <see cref="IFlashcart.SinceDataReceived"/>.
        /// </summary>
        TimeSpan SinceDataReceived { get; }

        /// <summary>
        /// Gets the <see cref="Flashcart"/> <see cref="IFlashcart.SinceRomMessageReceived"/>.
        /// </summary>
        TimeSpan SinceRomMessageReceived { get; }

        /// <summary>
        /// Queues a message to send to the device.
        /// </summary>
        /// <param name="message">Message to send.</param>
        void EnqueueMessage(IGebugMessage message);

        /// <summary>
        /// Gets a value indicating whether the manager thread is running or not.
        /// </summary>
        bool IsShutdown { get; }

        /// <summary>
        /// Initialize runtime object.
        /// </summary>
        /// <param name="portName">Serial port to communicate with device.</param>
        void Init(string portName);

        /// <summary>
        /// Starts the manager thread to allow communcation.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the manager thread, which stops communication with the device.
        /// </summary>
        void Stop();

        /// <summary>
        /// Send a ROM to the device.
        /// </summary>
        /// <param name="filedata">Byte array containing file data in the correct format.</param>
        /// <param name="token">Cancellation token.</param>
        void SendRom(string path, Nullable<CancellationToken> token = null);

        /// <summary>
        /// Tests whether the device is connected and currently in "Menu" mode.
        /// </summary>
        /// <returns>True if the device is in the default OS (without loading a ROM).</returns>
        bool TestFlashcartConnected();

        /// <summary>
        /// Tests whether the device is connected and currently running a gebug ROM.
        /// </summary>
        /// <returns>True if so, false otherwise.</returns>
        bool TestRomConnected();

        /// <summary>
        /// Adds a callback for messages received from the device.
        /// </summary>
        /// <param name="callback">Callback action to perform.</param>
        /// <param name="listenCount">Number of messages to listen for. A value of zero will listen indefinitely.</param>
        /// <param name="filter">Optional filter to apply to incoming messages. Only messages that
        /// pass the filter will count against <paramref name="listenCount"/>.</param>
        /// <returns>Id of new subscription.</returns>
        Guid Subscribe(Action<IGebugMessage> callback, int listenCount = 0, Func<IGebugMessage, bool>? filter = null);

        /// <summary>
        /// Cancels an active subscription by id.
        /// </summary>
        /// <param name="id">Id of subscription to cancel.</param>
        void Unsubscribe(Guid id);
    }
}
