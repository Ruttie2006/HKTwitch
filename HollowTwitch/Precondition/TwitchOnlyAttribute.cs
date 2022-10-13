using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Entities.Contexts;

namespace HollowTwitch.Precondition
{
	public class TwitchOnlyAttribute : PreconditionAttribute
	{
		public override bool Check(ICommandContext ctx)
		{
			if (ctx is TwitchCommandContext)
				return true;
			return false;
		}
	}
}
