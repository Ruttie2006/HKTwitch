using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HollowTwitch.Extensibility
{
    /// <summary>
    /// A config interface used to store cooldowns.
    /// </summary>
    public interface ICooldownConfig
    {
        /// <summary>
        /// The cooldowns this config stores.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, double> Cooldowns { get; set; }
    }
}
