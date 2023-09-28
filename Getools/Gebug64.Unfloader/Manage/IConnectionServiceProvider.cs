using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Unfloader;

namespace Gebug64.Unfloader.Manage
{
    public interface IConnectionServiceProvider
    {
        bool ManagerActive { get; set; }
        bool IsShutdown { get; }
        TimeSpan SinceDataReceived { get; }
        TimeSpan SinceRomMessageReceived { get; }

        void SendMessage(IGebugMessage msg);
        void SendMessage(IUnfloaderPacket packet);
        void SendRom(string path, Nullable<CancellationToken> token = null);

        void Start(string port);
        void Stop();

        Guid Subscribe(Action<IGebugMessage> callback, int listenCount = 0, Func<IGebugMessage, bool>? filter = null);
        void GebugUnsubscribe(Guid id);

        Guid Subscribe(Action<IUnfloaderPacket> callback, int listenCount = 0, Func<IUnfloaderPacket, bool>? filter = null);
        void UnfloaderUnsubscribe(Guid id);

        bool TestInMenu();
        bool TestInRom();
    }
}