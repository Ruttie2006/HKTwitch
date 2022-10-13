using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HollowTwitch.Entities;
using HollowTwitch.Entities.Contexts;
using HollowTwitch.Entities.Twitch;

namespace HollowTwitch
{
    public abstract class CommandBase
    {
        public IClient Client { get => Context.Client; }
		public ICommandContext Context { get; set; }

        public void SetContext(ICommandContext ctx)
        {
            Context = ctx;
        }

        public T GetContext<T>() where T : ICommandContext =>
            (T)Context;

        public virtual void Reply(string message)
        {
            if (Context is not TwitchCommandContext ctx)
                throw new InvalidOperationException("You cannot reply to messages not received via the twitch client!");
            ctx.SendMessage(message);
        }
    }
}
