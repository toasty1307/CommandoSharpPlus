using System;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.CommandStuff
{
    public abstract class Command
    {
        public static CommandoClient Client { get; set; }
        public abstract string Name { get; }
        public virtual string[] Aliases { get; } = Array.Empty<string>();
        public virtual bool AutoAliases { get; } = true;
        public abstract string GroupId { get; }
        public abstract string MemberName { get; }
        public abstract string Description { get; }
        public virtual string Format { get; } = null;
        public virtual string Details { get; } = null;
        public virtual string[] Examples { get; } = null;
        public virtual bool GuildOnly { get; } = false;
        public virtual bool OwnerOnly { get; } = false;
        public virtual Permissions[] ClientPermissions { get; } = null;
        public virtual Permissions[] UserPermissions { get; } = null;
        public virtual bool Nsfw { get; } = false;
        public virtual ThrottlingOptions ThrottlingOptions { get; } = null;
        public virtual bool DefaultHandling { get; } = true;
        public virtual ArgumentInfo[] Args { get; }
        public virtual int ArgsPromptLimit { get; } = int.MaxValue;
        public virtual ArgsType ArgsType { get; } = ArgsType.Single;
        public virtual int ArgsCount { get; } = 0;
        public virtual bool ArgsSingleQuotes { get; } = true;
        public virtual Regex[] Patterns { get; }
        public virtual bool Guarded { get; } = false;
        public virtual bool Hidden { get; } = false;
        public virtual bool Unknown { get; } = false;
        public virtual bool GlobalEnabled { get; set; } = true;

        public CommandGroup Group { get; set; }

        public abstract void Run(DiscordMessage message, ArgumentCollector collector);

        public override int GetHashCode() => Name.GetHashCode();

        public bool IsEnabledIn(DiscordGuild guild) => true; // TODO maek

        public string Usage(string argString, string prefix, DiscordUser user)
        {
            throw new NotImplementedException();
        }

        public static string Usage(Command command, string prefix, DiscordUser user)
        {
            throw new NotImplementedException();
        }

        public bool IsUsable(DiscordMessage message)
        {
            throw new NotImplementedException();
        }

        public static string Usage(Command command, string prefix) =>
            Usage(command, prefix, Client.CurrentUser);

        public static implicit operator bool(Command command) => command is not null;
    }

    public class ThrottlingOptions
    {
        public int Usage;
        public int Duration;
    }
}