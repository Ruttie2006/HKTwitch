using System;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Entities.Contexts;

namespace HollowTwitch.Precondition
{
    public class OwnerOnlyAttribute : PreconditionAttribute
    {
        public override bool Check(ICommandContext ctx)
        {
            return string.Equals(ctx.Message.User.Name, TwitchMod.Instance.Config.Twitch.Channel, StringComparison.OrdinalIgnoreCase);
        }
    }
}