using HollowTwitch.Entities.Attributes;
using HollowTwitch.Entities.Contexts;
using System;

namespace HollowTwitch.Precondition
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AdminOnlyAttribute : PreconditionAttribute
    {
        public override bool Check(ICommandContext ctx)
        {
            return TwitchMod.Instance.Config.AdminUsers.Contains(ctx.Message.User.Name);
        }
    }
}
