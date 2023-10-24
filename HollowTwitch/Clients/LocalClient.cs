using System;
using System.Net.Sockets;
using System.Text;
using HollowTwitch.Entities;
using HollowTwitch.Entities.Local;

namespace HollowTwitch.Clients
{
    /// <summary>
    /// This is just for local testing
    /// You should make a server on your Machine and start it.
    /// The client will try to connect to your local Server and receive messages.
    /// </summary>
    public class LocalClient : IClient
    {
        private const string LocalUser = "LocalUser";

        public ClientType Type => ClientType.Local;

        public event Action<LocalMessage> ReceivedChatMessage;
		event Action<IMessage> IClient.ReceivedChatMessage { add => ReceivedChatMessage += value; remove => ReceivedChatMessage -= value; }
		public event Action<string> ClientErrored;

        private static TcpClient _client;
        private static byte[] receiveBuf;
        private static NetworkStream stream;

        private readonly int Port;
        
        public LocalClient(GlobalConfig config, int port = 26955)
        {
            Port = port;
            
            config.AdminUsers.Add(LocalUser);
        }

        public void Dispose()
        {
            stream.Dispose();
            _client.Close();
        }

        public void StartReceive()
        {
            Connect("127.0.0.1", Port);
        }

        private void Connect(string host, int port)
        {
            _client = new TcpClient
            {
                ReceiveBufferSize = 4096,
                SendBufferSize = 4096
            };

            receiveBuf = new byte[4096];
            
            Logger.Log("Connecting...");
            
            _client.BeginConnect(host, port, ConnectCallback, _client);
        }
        
        private void ConnectCallback(IAsyncResult result)
        {
            var client = (TcpClient)result.AsyncState;
            client.EndConnect(result);

            if (!client.Connected)
            {
                Logger.LogError("Connection failed.");
                
                return;
            }

            Logger.Log("Connection Successful. Waiting for messages.");

            stream = client.GetStream();

            stream.BeginRead(receiveBuf, 0, 4096, RecvCallback, stream);
        }
        
        private void RecvCallback(IAsyncResult result)
        {
            var stream = (NetworkStream)result.AsyncState;
            int byte_len = stream.EndRead(result);
            
            if (byte_len <= 0)
            {
                Logger.LogError("Received length < 0!");
                
                ClientErrored?.Invoke("Invalid length!");
                    
                return;
            }
            
            var data = new byte[byte_len];
            
            Array.Copy(receiveBuf, data, byte_len);

            string message = Encoding.UTF8.GetString(data);
            
            Logger.Log($"Received message: {LocalUser}: {message}");

            var msg = new LocalMessage()
            {
                Content = message,
                User = new LocalUser() { Name = LocalUser },
            };

			ReceivedChatMessage?.Invoke(msg);
            
            stream.BeginRead(receiveBuf, 0, 4096, RecvCallback, null);
        }
    }
}
