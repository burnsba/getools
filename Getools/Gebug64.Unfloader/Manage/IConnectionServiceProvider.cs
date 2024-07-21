using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Dto;
using Gebug64.Unfloader.Protocol.Unfloader;

namespace Gebug64.Unfloader.Manage
{
    /// <summary>
    /// Interface for core communication provider.
    /// </summary>
    public interface IConnectionServiceProvider
    {
        /// <summary>
        /// Gets or sets a value to control whether the manager
        /// will accept incoming messages. The background worker thread
        /// will continue to run even when <see cref="ManagerActive"/> is false, but
        /// no parsing or publishing will occur.
        /// </summary>
        bool ManagerActive { get; set; }

        /// <summary>
        /// Gets a value indicating whether the background worker
        /// thread is running.
        /// </summary>
        bool IsShutdown { get; }

        /// <summary>
        /// Gets a value indicating the amount of time since any data was received
        /// from the flashcart device.
        /// </summary>
        TimeSpan SinceDataReceived { get; }

        /// <summary>
        /// Gets a value indicating the amount of time since a well formed <see cref="IGebugMessage"/>
        /// was received from the flashcart device (romhack).
        /// </summary>
        TimeSpan SinceRomMessageReceived { get; }

        /// <summary>
        /// Sends a message to the attached flashcart.
        /// </summary>
        /// <param name="msg">Message to send.</param>
        void SendMessage(IGebugMessage msg);

        /// <summary>
        /// Sends a message to the attached flashcart.
        /// </summary>
        /// <param name="packet">Packet to send.</param>
        void SendMessage(IUnfloaderPacket packet);

        /// <summary>
        /// Sends a ROM to the flashcart to load and boot/run.
        /// </summary>
        /// <param name="path">Path on disk of file to send.</param>
        /// <param name="token">Optional cancellation token.</param>
        void SendRom(string path, CancellationToken? token = null);

        /// <summary>
        /// Starts the background worker thread.
        /// Connects the flashcart to the given port.
        /// </summary>
        /// <param name="port">Serial port to connect to.</param>
        void Start(string port);

        /// <summary>
        /// Attempts to stop the background worker thread
        /// and gracefully disconnect the flashcart from the
        /// associated serial port.
        /// </summary>
        void Stop();

        /// <summary>
        /// Subscribes to the <see cref="IGebugMessage"/> message bus to be notified of messages from console.
        /// </summary>
        /// <param name="callback">Callback to execute when a message is received.</param>
        /// <param name="listenCount">Number of times to execute callback. Subscription is automatically dropped after this many matching events. A value of zero will listen forever.</param>
        /// <param name="filter">Optional filter. If set, only messages matching the filter will notify the subscriber.</param>
        /// <returns>Subscription id.</returns>
        Guid Subscribe(Action<IGebugMessage> callback, int listenCount = 0, Func<IGebugMessage, bool>? filter = null);

        /// <summary>
        /// Stops executing callbacks for the given id, on the <see cref="IGebugMessage"/> message bus.
        /// </summary>
        /// <param name="id">Id to unsubscribe.</param>
        void GebugUnsubscribe(Guid id);

        /// <summary>
        /// Subscribes to the <see cref="IUnfloaderPacket"/> message bus to be notified of messages from console.
        /// </summary>
        /// <param name="callback">Callback to execute when a message is received.</param>
        /// <param name="listenCount">Number of times to execute callback. Subscription is automatically dropped after this many matching events. A value of zero will listen forever.</param>
        /// <param name="filter">Optional filter. If set, only messages matching the filter will notify the subscriber.</param>
        /// <returns>Subscription id.</returns>
        Guid Subscribe(Action<IUnfloaderPacket> callback, int listenCount = 0, Func<IUnfloaderPacket, bool>? filter = null);

        /// <summary>
        /// Stops executing callbacks for the given id, on the <see cref="IUnfloaderPacket"/> message bus.
        /// </summary>
        /// <param name="id">Id to unsubscribe.</param>
        void UnfloaderUnsubscribe(Guid id);

        /// <summary>
        /// Executes flashcart specific test to determine whether the connection is currently
        /// in the flashcart menu.
        /// </summary>
        /// <returns>True if a valid flashcart level response is received, false otherwise.</returns>
        /// <remarks>
        /// Used to test connection level.
        /// </remarks>
        bool TestInMenu();

        /// <summary>
        /// Sends a gebug message (ping) to the attached flashcart and checks for a valid response.
        /// </summary>
        /// <returns>True if a valid <see cref="IGebugMessage"/> response is received, false otherwise.</returns>
        /// <remarks>
        /// Used to test connection level.
        /// </remarks>
        bool TestInRom();

        void AddLogExclusion(MessageCategoryCommand filter);
        void RemoveLogExclusion(MessageCategoryCommand filter);
    }
}