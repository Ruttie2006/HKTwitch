using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using HollowTwitch.Entities;
using HollowTwitch.Entities.Twitch;

namespace HollowTwitch.Clients
{
    public class TwitchClient : IClient
    {
        public static readonly string[] MessageSeparators = ["\r\n"];
        private TcpClient _client;
        public StreamReader Output;
		public StreamWriter Input;

        internal readonly GlobalConfig.TwitchConfig _config;

        public ClientType Type => ClientType.Twitch;

#nullable enable
        public event Action<TwitchMessage>? ReceivedChatMessage;
        event Action<IMessage> IClient.ReceivedChatMessage { add => ReceivedChatMessage += value; remove => ReceivedChatMessage -= value; }
#nullable restore
        public event Action<string> RawPayload;

        public event Action<string> ClientErrored;

        public TwitchClient(GlobalConfig.TwitchConfig config)
        {
            _config = config;
            ConnectAndAuthenticate(config);
            RawPayload += ProcessMessage;
        }

        private void ConnectAndAuthenticate(GlobalConfig.TwitchConfig config)
        {
            _client = new TcpClient("irc.twitch.tv", 6667);

            Output = new StreamReader(_client.GetStream());
            Input = new StreamWriter(_client.GetStream())
            {
                AutoFlush = true
            };

            if (!_client.Connected)
            {
                Reconnect(10000);
                return;
            }
                
            SendMessage($"PASS oauth:{config.Token}");
            SendMessage($"NICK {config.Username}");
            SendMessage($"JOIN #{config.Channel}");
        }

        private void Reconnect(int delay)
        {
            ClientErrored?.Invoke("Reconnecting........");
            Dispose();
            Thread.Sleep(delay);
            ConnectAndAuthenticate(_config);
        }

        private void ProcessMessage(string message)
        {
            if (message == null)
                return;

            if (message.Contains("PING"))
            {
                SendMessage("PONG :tmi.twitch.tv");
                Console.WriteLine("sent pong!");
            }
            else
            {
                // Can't be bothered to deal with tags yet, so we'll skip them (hopefully)
                if (message[0] == '@')
                    TwitchMod.Instance.LogWarn("Somehow, tag values were received in a message. This should not be possible, and currently this mod does not support them.");

				var pos = message.IndexOf(':');
                string userName = message.Substring(pos, message.IndexOf("!") - pos);
                pos += userName.Length;
                string userMail = message.Substring(pos, message.IndexOf(' '));
                var user = new TwitchUser()
                {
                    Name = userName,
                    Email = userMail
                };

                var cmd = message.Substring(pos, message.Substring(pos).IndexOf(' '));
                pos += cmd.Length;

                switch (cmd)
                {
                    case "PRIVMSG":
                        var channel = message.Substring(pos, message.Substring(pos).IndexOf(' '));
                        var privMsg = new TwitchMessage()
                        {
                            Raw = message,
                            Content = message.Substring(pos),
                            User = user,
                            ChannelId = channel.Substring(1)
                        };
                        ReceivedChatMessage?.Invoke(privMsg);
						break;
                    default:
						TwitchMod.Instance.LogDebug($"Received unknown message: \'{message}\'.");
                        break;
				}
            }
        }

        public void StartReceive()
        {
            while (true)
            {
                try
                {
                    if (!_client.Connected)
                    {
                        Dispose();
                        ConnectAndAuthenticate(_config);
                    }

                    string raw = Output.ReadLine();
					// "When the Twitch IRC server sends a message, it may contain a single message or multiple messages"
					var messages = raw.Split(MessageSeparators, StringSplitOptions.RemoveEmptyEntries);
					// "[The Twitch IRC server] may also send a message multiple times if it doesn’t think the bot received it"
                    // I can't be bothered to deal with that, so I won't.

					foreach (var message in messages.Distinct())
                        RawPayload?.Invoke(message);
                }
                catch (Exception e)
                {
                    ClientErrored?.Invoke("Error occured trying to read stream: " + e);
                    Reconnect(5000);
                }
               
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void SendMessage(string message) => Input.WriteLine(message);

        public void Dispose()
        {
            Input.Dispose();
            Output.Dispose();
            _client.Close();
        }
    }
}