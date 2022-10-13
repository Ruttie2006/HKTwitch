using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Entities.Contexts
{
	public interface ICommandContext
	{
		public IMessage Message { get; }
		public IClient Client { get; }
		public string CommandName { get; }
		public string[] Arguments { get; }
	}
}
