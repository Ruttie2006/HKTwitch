using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Entities.Twitch
{
	public class TwitchUser : IUser
	{
		public string Name { get; set; }
		public string Email { get; set; }
	}
}
