using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using HollowTwitch.Clients;
using HollowTwitch.Entities;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Entities.BiliBili;
using HollowTwitch.Entities.Contexts;
using HollowTwitch.Entities.Local;
using HollowTwitch.Entities.Twitch;
using HollowTwitch.Extensibility;
using HollowTwitch.Precondition;
using HutongGames.PlayMaker;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace HollowTwitch
{
    // Scuffed command processor thingy, needs a lot of work
    public class CommandProcessor
    {
        public const char Separator = ' ';

        public List<Command> Commands { get; init; } = [];
        public Dictionary<Type, IArgumentParser> ArgumentParsers { get; init; } = [];
        
        private MonoBehaviour _coroutineRunner;

        internal void Initialize()
        {
            var go = new GameObject();

            UObject.DontDestroyOnLoad(go);

            _coroutineRunner = go.AddComponent<NonBouncer>();
        }

        public void AddTypeParser<T>(T parser, Type t) where T : IArgumentParser
        {
            ArgumentParsers.Add(t, parser);
        }

        public void Execute(IClient client, IMessage message, ReadOnlyCollection<string> blacklist, bool ignoreChecks = false)
        {
            var trimmedContent = message.Content.Substring(TwitchMod.Instance.Config.Prefix.Length);
            string[] pieces = trimmedContent.Split(Separator);
			IEnumerable<string> args = pieces.Skip(1);

			IOrderedEnumerable<Command> found = Commands
                                                .Where(x => x.Name.Equals(pieces[0],
													TwitchMod.Instance.Config.CaseSensitive
                                                    ? StringComparison.InvariantCulture
                                                    : StringComparison.InvariantCultureIgnoreCase))
                                                .OrderByDescending(x => x.Priority);

			ICommandContext ctx = client.Type switch
			{
				ClientType.Twitch => new TwitchCommandContext(client as TwitchClient, message as TwitchMessage)
				{
					Arguments = args.ToArray(),
					CommandName = pieces[0],
				},
				ClientType.Bilibili => new BiliBiliCommandContext(client as BiliBiliClient, message as BiliMessage)
				{
					Arguments = args.ToArray(),
					CommandName = pieces[0]
				},
				ClientType.Local => new LocalCommandContext(client as LocalClient, message as LocalMessage)
				{
					Arguments = args.ToArray(),
					CommandName = pieces[0]
				},
				_ => new BaseCommandContext(message, pieces[0], args.ToArray())
			};

			foreach (Command c in found)
            {
                bool allGood = !blacklist.Contains(c.Name, StringComparer.OrdinalIgnoreCase);

				foreach (PreconditionAttribute p in c.Preconditions)
                {
                    if (p.Check(ctx)) continue;

                    allGood = false;

                    if (c.Preconditions.FirstOrDefault() is CooldownAttribute cooldown)
                    {
                        Logger.Log
                        (
                            $"The coodown for command {c.Name} failed. "
                            + $"The cooldown has {cooldown.MaxUses - cooldown.Uses} and will reset in {cooldown.ResetTime - DateTimeOffset.Now}"
                        );
                    }
                }

                allGood |= ignoreChecks;

                if (!allGood)
                    continue;

                if (!BuildArguments(args, c, out object[] parsed))
                    continue;

                foreach (PreconditionAttribute precond in c.Preconditions)
                {
                    precond.Use();
                }

                try
                {
                    Logger.LogDebug($"Built arguments for command {message.Content}.");

                    IEnumerator RunCommand()
                    {
                        /*
                         * We have to wait a frame in order to make Unity itself call
                         * the MoveNext on the IEnumerator
                         *
                         * This forces it to run on the main thread, so Unity doesn't break.
                         */
                        yield return null;


                        c.ClassInstance.SetContext(ctx);
                        if (c.MethodInfo.ReturnType == typeof(IEnumerator))
                        {
                            yield return c.MethodInfo.Invoke(c.ClassInstance, parsed) as IEnumerator;
                        }
                        else
                        {
                            c.MethodInfo.Invoke(c.ClassInstance, parsed);
                        }
                    }

                    _coroutineRunner.StartCoroutine(RunCommand());

                }
                catch (Exception e)
                {
                    Logger.LogError(e.ToString());
                }
            }
        }

        private bool BuildArguments(IEnumerable<string> args, Command command, out object[] parsed)
        {
            parsed = null;

            // Avoid multiple enumerations when indexing
            string[] enumerated = args.ToArray();
            
            ParameterInfo[] parameters = command.Parameters;

            bool hasRemainder = parameters.Length != 0 && parameters[parameters.Length - 1].GetCustomAttributes(typeof(RemainingTextAttribute), false).Any();
            
            if (enumerated.Length < parameters.Length && !hasRemainder)
                return false;
            
            var built = new List<object>();

            for (int i = 0; i < parameters.Length; i++)
            {
                string toParse = enumerated[i];
                if (i == parameters.Length - 1)
                {
                    if (hasRemainder)
                    {
                        toParse = string.Join(Separator.ToString(), enumerated.Skip(i).Take(enumerated.Length).ToArray());
                    }
                }
                
                object p = ParseParameter(toParse, parameters[i].ParameterType);

                if (p is null)
                    return false;

                if (parameters[i].GetCustomAttributes(typeof(EnsureParameterAttribute), false).FirstOrDefault() is EnsureParameterAttribute epa)
                    p = epa.Ensure(p);

                built.Add(p);
            }

            parsed = built.ToArray();

            return true;
        }

        private object ParseParameter(string arg, Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);

            try
            {
                return converter.ConvertFromString(arg);
            }
            catch
            {
                try
                {
                    return ArgumentParsers[type].Parse(arg);
                }
                catch
                {
                    return null;
                }
            }
        }

#nullable enable
        public void RegisterCooldowns(ICooldownConfig config, IEnumerable<Command>? ownedCommands = null)
        {
            config.Cooldowns ??= [];
            // No cooldowns configured, let's populate the dictionary.
            if (config.Cooldowns.Count == 0)
            {
                foreach (Command c in TwitchMod.Instance.Processor.Commands)
                {
                    if (ownedCommands != null && !ownedCommands.Contains(c))
                        continue;

                    CooldownAttribute cd = c.Preconditions.OfType<CooldownAttribute>().FirstOrDefault();

                    if (cd == null)
                        continue;

                    config.Cooldowns[c.Name] = (int)cd.Cooldown.TotalSeconds;
                }

                return;
            }

            foreach (Command c in TwitchMod.Instance.Processor.Commands)
            {
                if (ownedCommands != null && !ownedCommands.Contains(c))
                    continue;

                if (!config.Cooldowns.TryGetValue(c.Name, out double time))
                    continue;

                CooldownAttribute cd = c.Preconditions.OfType<CooldownAttribute>().First();

                cd.Cooldown = TimeSpan.FromSeconds(time);
            }
        }
#nullable restore

        public List<Command> RegisterCommands(Assembly asm)
        {
            var cmds = new List<Command>();
            foreach (var type in asm.DefinedTypes.Where(x => x.IsSubclassOf(typeof(CommandBase))))
                cmds.AddRange(RegisterCommands(type));
            return cmds;
        }

        public List<Command> RegisterCommands<T>() where T : CommandBase =>
            RegisterCommands(typeof(T));

        public List<Command> RegisterCommands(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var instance = (CommandBase)Activator.CreateInstance(type);
            var cmdList = new List<Command>();

            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttributes<HKCommandAttribute>(false).FirstOrDefault() is not { } attr)
                    continue;

                var cmd = new Command(attr.Name, method, instance);

                Commands.Add(cmd);
                cmdList.Add(cmd);

                Logger.Log($"Added command: {attr.Name}");
            }
            return cmdList;
        }
    }
}