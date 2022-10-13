using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Entities.BiliBili
{
	public class BiliMessage : IMessage<BiliUser>
	{
		public BiliUser User { get; set; }
		IUser IMessage.User { get => User; }
		public string Content { get; set; }
		public string Time { get; set; }
		public string Raw { get; set; }
	}
}
