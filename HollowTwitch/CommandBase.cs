using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch
{
    public abstract class CommandBase
    {
        public IClient Client { get; set; }
        public StreamWriter Input { get; set; }
        public string ChannelId { get; set; }

        public void SetContext(IClient client, StreamWriter input, string channel)
        {
            Client = client;
            Input = input;
            ChannelId = channel;
        }

        public virtual void SendMessage(string message)
        {
            Input.WriteLine($"PRIVMSG #{ChannelId} : [BOT] {message}");
        }
    }
}
