using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using HollowTwitch.Clients;
using HollowTwitch.Entities;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Precondition;
using JetBrains.Annotations;
using Modding;
using UnityEngine;

namespace HollowTwitch
{
    [UsedImplicitly]
    public class TwitchMod : Mod, ITogglableMod, IGlobalSettings<GlobalConfig>
    {
        private IClient _client;
        
        private Thread _currentThread;

        public GlobalConfig Config = new();

        public CommandProcessor Processor { get; private set; }

        public static TwitchMod Instance;
        
        public void OnLoadGlobal(GlobalConfig s) => Config = s;

        public GlobalConfig OnSaveGlobal() => Config;

        public static readonly Version Version = new(2, 5, 0, 0);
        public override string GetVersion() =>
            "R." + Version.ToString(4);

        public TwitchMod() : base("HollowTwitch")
        {
            Instance = this;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            ModHooks.ApplicationQuitHook += OnQuit;
            ModHooks.FinishedLoadingModsHook += OnFinishLoad;
            ReceiveCommands();
        }

        private void OnFinishLoad()
        {
            GenerateHelpInfo();
        }

        private void ReceiveCommands()
        {
            Processor = new CommandProcessor();
            Processor.Initialize();

            if (Config.TwitchToken is null)
            {
                Logger.Log("Token not found, relaunch the game with the fields in settings populated.");
                return;
            }

            _client = Config.Client switch 
            {
                ClientType.Twitch => new TwitchClient(Config),
                
                ClientType.Bilibili => new BiliBiliClient(Config),
                
                ClientType.Local => new LocalClient(Config),
                
                _ => throw new InvalidOperationException($"Enum member {Config.Client} does not exist!") 
            };

            _client.ChatMessageReceived += OnMessageReceived;

            _client.ClientErrored += s => Log($"An error occured while receiving messages.\nError: {s}");

            _currentThread = new Thread(_client.StartReceive)
            {
                IsBackground = true
            };
            _currentThread.Start();

            Log("Started receiving");
        }

        private void OnQuit()
        {
            _client.Dispose();
            _currentThread.Abort();
        }

        private void OnMessageReceived(bool twitch, string user, string message)
        {
            Log($"Twitch chat: [{user}: {message}]");

            string trimmed = message.Trim();
            int index = trimmed.IndexOf(Config.Prefix);

            if (index != 0) return;

            string command = trimmed.Substring(Config.Prefix.Length).Trim();

            if (Config.BannedUsers.Contains(user, StringComparer.OrdinalIgnoreCase))
                return;

            Processor.ExecuteTwitch((TwitchClient)_client, user, command, Config.BlacklistedCommands.AsReadOnly(), Config.AdminUsers.Contains(user, StringComparer.OrdinalIgnoreCase));
        }

        private void GenerateHelpInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Twitch Mod Command List.\n");

            foreach (Command command in Processor.commands)
            {
                string name = command.Name;
                sb.AppendLine($"Command: {name}");

                object[]           attributes = command.MethodInfo.GetCustomAttributes(false);
                string             args       = string.Join(" ", command.Parameters.Select(x => $"[{x.Name}]").ToArray());
                CooldownAttribute  cooldown   = attributes.OfType<CooldownAttribute>().FirstOrDefault();
                SummaryAttribute   summary    = attributes.OfType<SummaryAttribute>().FirstOrDefault();
                
                sb.AppendLine($"Usage: {Config.Prefix}{name} {args}");
                sb.AppendLine($"Cooldown: {(cooldown is null ? "This command has no cooldown" : $"{cooldown.MaxUses} use(s) per {cooldown.Cooldown.ToString().TrimEnd('0')}.")}");
                sb.AppendLine($"Summary:\n{(summary?.Summary ?? "No summary provided.")}\n");
            }

            var path = new FileInfo(Assembly.GetExecutingAssembly().Location);

            File.WriteAllText(Path.Combine(path.Directory!.FullName, "CommandList.txt"), sb.ToString());
        }

        public void Unload() => OnQuit();
    }
}