using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HollowTwitch.Clients;
using HollowTwitch.Entities.Twitch;

namespace HollowTwitch.Entities.Contexts
{
    public class TwitchCommandContext : BaseCommandContext
    {
        public new TwitchClient Client { get; set; }
        public StreamWriter Input { get; set; }
        public new TwitchMessage Message { get; set; }

		public TwitchCommandContext(TwitchClient client, TwitchMessage msg) : base()
		{
            Client = client;
			Input = client.Input;
            Message = msg;
		}

		public TwitchCommandContext(TwitchClient client, TwitchMessage msg, IMessage message, string commandName, string[] arguments) : base(message, commandName, arguments)
		{
			Client = client;
			Input = client.Input;
			Message = msg;
		}

        public void SendMessage(string msg)
        {
			Input.WriteLine($"PRIVMSG #{Message.ChannelId} : [BOT] {msg}");
		}
    }
}
