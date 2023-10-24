using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Entities.Contexts
{
    public interface IRespondableContext : ICommandContext
    {
        public void SendMessage(string message);
    }
}
