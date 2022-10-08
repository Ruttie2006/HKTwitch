using System.Collections.Generic;

namespace HollowTwitch.Extensibility
{
    /// <summary>
    /// The default <see cref="ICooldownConfig"/> implementation.
    /// </summary>
    public class CooldownConfig : ICooldownConfig
    {
        /// <inheritdoc/>
        public Dictionary<string, double> Cooldowns { get; set; }
    }
}
