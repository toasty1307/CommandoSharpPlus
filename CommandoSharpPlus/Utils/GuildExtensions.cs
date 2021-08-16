using System.Collections.Generic;
using CommandoSharpPlus.CommandStuff;
using CommandoSharpPlus.Providers;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.Utils
{
    public static class GuildExtensions
    {
        public static Dictionary<ulong, string> GuildsAndPrefixes = new();
        public static Dictionary<ulong, List<CommandGroup>> GuildsAndEnabledGroup = new();
        public static CommandoClient Client { get; set; }
        
        public static string GetCommandPrefix(this DiscordGuild guild) => GuildsAndPrefixes.ContainsKey(guild.Id)
            ? GuildsAndPrefixes[guild.Id]
            : Client.CommandPrefix;
        public static void SetCommandPrefix(this DiscordGuild guild, string prefix)
        {
            if (!GuildsAndPrefixes.ContainsKey(guild.Id))
                GuildsAndPrefixes.Add(guild.Id, prefix);
            else
                GuildsAndPrefixes[guild.Id] = prefix;
        }

        public static GuildSettings GetSettings(this DiscordGuild guild) =>
            SQLiteProvider.Settings.ContainsKey(guild.Id) ? SQLiteProvider.Settings[guild.Id] : default;
        
        public static void SetSettings(this DiscordGuild guild, GuildSettings settings)
        {
            if (!SQLiteProvider.Settings.ContainsKey(guild.Id))
                SQLiteProvider.Settings.Add(guild.Id, settings);
            else
                SQLiteProvider.Settings[guild.Id] = settings;
        }

        public static void SetGroupEnabled(this DiscordGuild guild, CommandGroup group, bool enabled)
        {
            GuildsAndEnabledGroup[guild.Id] ??= new List<CommandGroup>();
            if (GuildsAndEnabledGroup[guild.Id].Contains(group))
            {
                if (!enabled) GuildsAndEnabledGroup[guild.Id].Remove(group);
            }
            else
            {
                if (enabled) GuildsAndEnabledGroup[guild.Id].Add(group);
            }
        }

        public static bool IsGroupEnabled(this DiscordGuild guild, CommandGroup group) =>
            GuildsAndEnabledGroup[guild.Id].Contains(group);
    }
}