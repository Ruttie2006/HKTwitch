using System;
using HollowTwitch.Clients;
using HollowTwitch.Entities;

namespace HollowTwitch
{
    public interface IClient : IDisposable
    {
        public event Action<IMessage> ReceivedChatMessage;
        public ClientType Type { get; }

		public event Action<string> ClientErrored;

		public void StartReceive();
    }
}
