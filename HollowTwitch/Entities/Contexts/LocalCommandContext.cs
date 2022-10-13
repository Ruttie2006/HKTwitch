using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HollowTwitch.Clients;
using HollowTwitch.Entities.BiliBili;
using HollowTwitch.Entities.Local;

namespace HollowTwitch.Entities.Contexts
{
	public class LocalCommandContext : BaseCommandContext
	{
		public new LocalClient Client { get; set; }
		public new LocalMessage Message { get; set; }

		public LocalCommandContext(LocalClient client, LocalMessage message) : base()
		{
			Client = client;
			Message = message;
		}
	}
}
