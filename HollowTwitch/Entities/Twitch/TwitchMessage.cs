using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Entities.Twitch
{
	public class TwitchMessage : IMessage<TwitchUser>
	{
		public TwitchUser User { get; set; }
		IUser IMessage.User { get => User; }
		public string Content { get; set; }
		public string ChannelId { get; set; }
		public string Raw { get; set; }
	}
}
