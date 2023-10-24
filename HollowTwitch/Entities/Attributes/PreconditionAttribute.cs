using System;
using HollowTwitch.Entities.Contexts;

namespace HollowTwitch.Entities.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public abstract class PreconditionAttribute : Attribute
    {            
        public abstract bool Check(ICommandContext ctx);

        public virtual void Use() {}
    }
}