using Modding;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HollowTwitch.Extensibility
{
    public abstract class ExtensionMod<TConfig> : Mod, IGlobalSettings<TConfig>
        where TConfig : ICooldownConfig, new()
    {
        readonly Assembly Asm;
        public Dictionary<string, Dictionary<string, GameObject>> Preloads;
        public TConfig Config;

        public abstract string ExtensionName { get; }

        public ExtensionMod() : base()
        {
            Asm = GetType().Assembly;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloads)
        {
            Preloads = preloads;
            if (Asm == null)
                LogError($"{nameof(Asm)} is null!");
            var Commands = TwitchMod.Instance.Processor.RegisterCommands(Asm);

            if (Commands == null)
                LogError($"{nameof(Commands)} is null!");
            Config ??= new();
            TwitchMod.Instance.Processor.RegisterCooldowns(Config, Commands);
            base.Initialize();
        }

        public new string GetName() { return "HollowTwitch." + ExtensionName; }

        public void OnLoadGlobal(TConfig s) =>
            Config = s;

        public TConfig OnSaveGlobal() =>
            Config;
    }
}
