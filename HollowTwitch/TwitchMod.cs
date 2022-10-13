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
		public static TwitchMod Instance;

		private Thread _currentThread;

		public IClient Client { get; private set; }
		public GlobalConfig Config { get; private set; } = new();
        public CommandProcessor Processor { get; private set; }
        
        public void OnLoadGlobal(GlobalConfig s) =>
            Config = s;

        public GlobalConfig OnSaveGlobal() =>
            Config;

        public static readonly Version Version = new(2, 5, 0, 0);
        public override string GetVersion() =>
            "R." + Version.ToString(4);

        public TwitchMod() : base("HollowTwitch")
        {
            Instance = this;
        }

        public override void Initialize()
        {
            ModHooks.ApplicationQuitHook += OnQuit;
            ModHooks.FinishedLoadingModsHook += OnFinishLoad;
            InitProcessor();
        }

        private void OnFinishLoad()
        {
            GenerateHelpInfo();
        }

        private void InitProcessor()
        {
            Processor = new CommandProcessor();
            Processor.Initialize();

            if (Config.TwitchToken is null)
            {
                LogError("Token not found, relaunch the game with the fields in settings populated.");
                return;
            }

            Client = Config.Client switch 
            {
                ClientType.Twitch => new TwitchClient(Config),
                
                ClientType.Bilibili => new BiliBiliClient(Config),
                
                ClientType.Local => new LocalClient(Config),
                
                _ => throw new InvalidOperationException($"Enum member {Config.Client} does not exist!") 
            };

            Client.ReceivedChatMessage += OnMessageReceived;

            Client.ClientErrored += s => LogError($"An error occured while receiving messages.\nError: {s}");

            _currentThread = new Thread(Client.StartReceive)
            {
                IsBackground = true
            };
            _currentThread.Start();

            LogDebug("Started receiving");
        }

        private void OnQuit()
        {
            Client.Dispose();
            _currentThread.Abort();
        }

        private void OnMessageReceived(IMessage message)
        {
            LogDebug($"Message received: [{message.User}: {message.Content}]");

            string trimmed = message.Content.Trim();
            int index = trimmed.IndexOf(Config.Prefix);

            if (index != 0)
                return;

            if (Config.BannedUsers.Contains(message.User.Name, StringComparer.OrdinalIgnoreCase))
                return;

            Processor.Execute(Client, message, Config.BlacklistedCommands.AsReadOnly(), Config.AdminUsers.Contains(message.User.Name, StringComparer.OrdinalIgnoreCase));
        }

        private void GenerateHelpInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Twitch Mod Command List.\n");

            foreach (Command command in Processor.Commands)
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