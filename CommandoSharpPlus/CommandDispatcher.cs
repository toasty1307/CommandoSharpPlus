using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using CommandoSharpPlus.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace CommandoSharpPlus
{
    public class CommandDispatcher
    {
        public CommandoClient Client { get; set; }
        public CommandRegistry Registry { get; set; }
        public List<Inhibitor> Inhibitors { get; set; }
        private Dictionary<ulong, DiscordMessage> _results { get; set; }
        private Dictionary<ulong, ulong> _awaiting { get; set; }
        private Dictionary<string, Regex> _commandPatterns { get; set; } = new();

        public CommandDispatcher(CommandoClient client, CommandRegistry registry)
        {
            Client = client;
            Registry = registry;
            Inhibitors = new List<Inhibitor>();
            _results = new Dictionary<ulong, DiscordMessage>();
            _awaiting = new Dictionary<ulong, ulong>();
        }

        public void HandleMessage(DiscordMessage message)
        {
            HandleMessage(message, null);
        }

        public async void HandleMessage(DiscordMessage message, DiscordMessage oldMessage)
        {
            if (!ShouldHandleMessage(message, oldMessage)) return;
            DiscordMessage cmdMsg = null, oldCmdMsg = null;
            if (oldMessage is not null)
            {
                oldCmdMsg = _results[oldMessage.Id];
                if (oldCmdMsg is null && !Client.Options.NonCommandEditable) return;
                cmdMsg = ParseMessage(message);
                if (cmdMsg is not null && oldCmdMsg is not null)
                {
                    MessageExtensions.MessageSettingsMap[cmdMsg.Id].Responses =
                        MessageExtensions.MessageSettingsMap[oldCmdMsg.Id].Responses;
                    MessageExtensions.MessageSettingsMap[cmdMsg.Id].ResponsePosition =
                        MessageExtensions.MessageSettingsMap[oldCmdMsg.Id].ResponsePosition;
                }
            }   
            else cmdMsg = ParseMessage(message);

            DiscordMessage[] responses = null;
            if (cmdMsg is not null)
            {
                var inhibit = Inhitbit(cmdMsg);
                if (inhibit is null)
                {
                    if (MessageExtensions.MessageSettingsMap[cmdMsg.Id].Command)
                    {
                        if (!MessageExtensions.MessageSettingsMap[cmdMsg.Id].Command.IsEnabledIn(message.Channel.Guild))
                        {
                            if (!MessageExtensions.MessageSettingsMap[cmdMsg.Id].Command.Unknown)
                                responses = new[]
                                {
                                    await message.RespondAsync(
                                        $"The `{MessageExtensions.MessageSettingsMap[cmdMsg.Id].Command.Name}` command is disabled")
                                };
                            else
                            {
                                // TODO Unknown Command
                                responses = null;
                            }
                        }
                        else if (oldMessage is null)
                            responses = await cmdMsg.Run();
                    }
                    else
                    {
                        // TODO Unknown Command Event
                        responses = null;
                    }
                }
                else
                    responses = new[] {inhibit.Response};
                cmdMsg.Finalize(/* responses */);
            }
            else if (oldCmdMsg is not null)
            {
                oldCmdMsg.Finalize( /* null */);
                if (!Client.Options.NonCommandEditable) _results.Remove(message.Id);
            }
            
            CacheCommandMessage(message, oldMessage, cmdMsg, responses);
        }

        public bool AddInhibitor(Inhibitor inhibitor)
        {
            if (Inhibitors.Contains(inhibitor)) return false;
            Inhibitors.Add(inhibitor);
            return true;
        }
        
        public bool RemoveInhibitor(Inhibitor inhibitor)
        {
            if (Inhibitors.Contains(inhibitor)) return false;
            Inhibitors.Remove(inhibitor);
            return true;
        }

        public bool ShouldHandleMessage(DiscordMessage message, DiscordMessage oldMessage)
        {
            if (message.Author.IsBot) return false;
            if (_awaiting.ContainsKey(message.Author.Id)) return false;
            if (oldMessage is null) return true;
            return oldMessage.Content != message.Content;
        }
        
        public bool ShouldHandleMessage(DiscordMessage message)
        {
            if (message.Author.IsBot) return false;
            return !_awaiting.ContainsKey(message.Author.Id);
        }

        public Inhibition Inhitbit(DiscordMessage message)
        {
            foreach (var inhitbitResult in Inhibitors.Select(inhibitor => inhibitor(message)))
            {
                if (inhitbitResult is not null)
                {
                    /* TODO CommandBlock Event */ 
                    return inhitbitResult;
                }
            }

            return null;
        }

        private void CacheCommandMessage(DiscordMessage message, DiscordMessage oldMessage, DiscordMessage commandMessage, DiscordMessage[] responses)
        {
            if (Client.Options.CommandEditableDuration <= 0) return;
            if (commandMessage is null && !Client.Options.NonCommandEditable) return;
            if (responses is null) return;
            if (!_results.ContainsKey(message.Id))
                _results.Add(message.Id, commandMessage);
            if (oldMessage is not null)
                Task.Delay(Client.Options.CommandEditableDuration).ContinueWith(_ => _results.Remove(message.Id));
            else
                _results.Remove(message.Id);
        }

        public DiscordMessage ParseMessage(DiscordMessage message)
        {
            foreach (var command in Registry.Commands)
            {
                if (command.Patterns is null) continue;
                foreach (var pattern in command.Patterns)
                {
                    var matches = pattern.Matches(message.Content);
                    return message.InitCommand(command, null, matches);
                }
            }
            
            var prefix = message.Channel.Guild is not null ? message.Channel.Guild.GetCommandPrefix() : Client.CommandPrefix;
            Client.Logger.LogInformation(Client.CommandPrefix);
            Client.Logger.LogInformation(message.Channel.Guild.GetCommandPrefix());
            if (_commandPatterns[prefix] is null) BuildCommandPattern(prefix);
            var cmdMsg = MatchDefault(message, _commandPatterns[prefix], 2);
            if (cmdMsg is null && message.Channel.Guild is null)
                cmdMsg = MatchDefault(message, new Regex("/^([^\\s]+)/i"), 1, true);
            return cmdMsg;
        }

        private Regex BuildCommandPattern(string prefix)
        {
            Regex pattern;
            if (prefix is not null)
            {
                var escapedPrefix = new Regex("/[|\\\\{}()[\\]^$+*?.]/g").Replace(prefix, "\\$&");
                pattern = new Regex(
                    "^(<@!?${this.client.user.id}>\\s+(?:${escapedPrefix}\\s*)?|${escapedPrefix}\\s*)([^\\s]+)");
            }
            else
                pattern = new Regex("(^<@!?${this.client.user.id}>\\s+)([^\\s]+)");

            _commandPatterns[prefix] = pattern;
            Client.Logger.LogDebug("Built Command Patterns for prefix \"{0}\":{1}" ,prefix, pattern);
            return pattern;
        }

        private DiscordMessage MatchDefault(DiscordMessage message, Regex pattern, int commandNameIndex = 1,
            bool prefixLess = false)
        {
            var matches = pattern.Matches(message.Content);
            var commands = Registry.FindCommands(matches[commandNameIndex].Value, true);
            if (commands.Length != 1 || !commands[0].DefaultHandling)
                return message.InitCommand(Registry.UnknownCommand, prefixLess ? message.Content : matches[1].Value, null);
            var argString =
                message.Content.Substring(matches[1].Value.Length + (matches.Count >= 3 ? matches[2].Value.Length : 0));
            return message.InitCommand(commands[0], argString, null);
        }
        
    }

    public delegate Inhibition Inhibitor(DiscordMessage message);
    
    public class Inhibition
    {
        public string Reason { get; set; }
        public DiscordMessage Response { get; set; }
    }
}