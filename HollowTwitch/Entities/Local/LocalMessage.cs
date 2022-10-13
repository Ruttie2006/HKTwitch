using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Entities.Local
{
	public class LocalMessage : IMessage<LocalUser>
	{
		public LocalUser User { get; set; }
		IUser IMessage.User { get => User; }
		public string Content { get; set; }
		public string Raw { get => Content; }
	}
}
