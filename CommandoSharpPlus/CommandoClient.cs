using System;
using System.Threading.Tasks;
using CommandoSharpPlus.Providers;
using CommandoSharpPlus.Utils;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CommandoSharpPlus
{
    public class CommandoClient : DiscordClient
    {
        public CommandRegistry Registry { get; set; }
        public CommandDispatcher Dispatcher { get; set; }
        public SettingsProvider Provider { get; set; }
        public GuildSettingsHelper Settings { get; set; }
        public CommandoConfiguration Options { get; set; }

        public string CommandPrefix
        {
            get => _commandPrefix ?? Options.CommandPrefix;
            set
            {
                _commandPrefix = value;
                CommandoEvents.InvokeCommandPrefixChange(null, value);
            }
        }
        private string _commandPrefix;
        
        public CommandoClient(CommandoConfiguration config) : base(config)
        {
            Options = config;
            Registry = new CommandRegistry(this);
            Dispatcher = new CommandDispatcher(this, Registry);
            Provider = null;
            Settings = new GuildSettingsHelper(this, null);
            _commandPrefix = null;
            MessageCreated += (_, args) =>
            {
                Dispatcher.HandleMessage(args.Message);
                return Task.CompletedTask;
            };
            MessageUpdated += (_, args) =>
            {
                Dispatcher.HandleMessage(args.MessageBefore, args.Message);
                return Task.CompletedTask;
            };
            if (Options.Owners.Length > 0)
            {
                Ready += (sender, _) =>
                {
                    Logger.LogDebug("Bot Logged In");
                    foreach (var owner in config.Owners)
                    {
                        try { sender.GetUserAsync(owner); }
                        catch { Logger.LogError("Unable To fetch Owner {0}", owner); }
                    }
                    return Task.CompletedTask;
                };
            }

            MessageExtensions.Client = this;
            GuildExtensions.Client = this;
        }
    }
}