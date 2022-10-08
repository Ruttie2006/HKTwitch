using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Extensibility
{
    /// <summary>
    /// An <see cref="ExtensionMod{TConfig}"/> that uses <see cref="CooldownConfig"/> as config.
    /// </summary>
    public abstract class ExtensionMod : ExtensionMod<CooldownConfig>
    {
    }
}
