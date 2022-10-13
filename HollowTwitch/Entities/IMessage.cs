using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Entities
{
	public interface IMessage
	{
		public IUser User { get; }
		public string Content { get; }
		public string Raw { get; }
	}

	public interface IMessage<T> : IMessage where T: IUser
	{
		public new T User { get; }
	}
}
