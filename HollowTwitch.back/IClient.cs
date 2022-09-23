using System;

namespace HollowTwitch
{
    public interface IClient : IDisposable
    {
        event Action<bool, string, string> ChatMessageReceived;

        event Action<string> ClientErrored;

        void StartReceive();
    }
}
