using System;
using System.Collections.Generic;
using CommandoSharpPlus.Utils;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.CommandStuff
{
    public class CommandGroup
    {
        public CommandoClient Client { get; set; }
        public List<Command> Commands { get; set; } = new();
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Guarded { get; }
        public bool GlobalEnabled { get; set; } = true;

        public CommandGroup(CommandoClient client, string id, string name, bool guarded = false)
        {
            Client = client;
            Id = id;
            Name = name;
            Guarded = guarded;
        }

        public void SetEnabledIn(DiscordGuild guild, bool enabled)
        {
            if (Guarded) throw new Exception("This group is Guarded");
            if (guild is null)
            {
                GlobalEnabled = enabled;
                CommandoEvents.InvokeGroupStatusChange(null, this, enabled);
                return;
            }
            guild.SetGroupEnabled(this, enabled);
        }

        public bool IsEnabledIn(DiscordGuild guild)
        {
            if (Guarded) return true;
            return guild?.IsGroupEnabled(this) ?? GlobalEnabled;
        }
    }
}