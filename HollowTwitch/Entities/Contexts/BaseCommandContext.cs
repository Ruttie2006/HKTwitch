using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Entities.Contexts
{
	public class BaseCommandContext : ICommandContext
	{
		public IMessage Message { get; init; }
		public IClient Client { get; }
		public string CommandName { get; init; }
		public string[] Arguments { get; init; }

		protected BaseCommandContext() { }

		public BaseCommandContext(IMessage message, string commandName, string[] arguments)
		{
			Message = message;
			CommandName = commandName;
			Arguments = arguments;
		}
	}
}
