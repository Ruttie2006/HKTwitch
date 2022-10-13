using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HollowTwitch.Clients;
using HollowTwitch.Entities.BiliBili;
using HollowTwitch.Entities.Twitch;

namespace HollowTwitch.Entities.Contexts
{
    public class BiliBiliCommandContext : BaseCommandContext
	{
		public new BiliBiliClient Client { get; set; }
		public new BiliMessage Message { get; set; }

		public BiliBiliCommandContext(BiliBiliClient client, BiliMessage message) : base()
        {
			Client = client;
			Message = message;
        }
    }
}
