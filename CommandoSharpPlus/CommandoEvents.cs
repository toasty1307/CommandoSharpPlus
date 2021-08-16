using System.Runtime.InteropServices;
using CommandoSharpPlus.CommandStuff;
using DSharpPlus.Entities;

namespace CommandoSharpPlus
{
    public delegate void CommandPrefixChange(DiscordGuild guild, string prefix);

    public delegate void CommandStatusChange(DiscordGuild guild, Command command, bool enabled);

    public delegate void GroupStatusChange(DiscordGuild guild, CommandGroup group, bool enabled);
    public delegate void CommandRegister(Command command);
    public delegate void GroupRegister(CommandGroup group);

    public static class CommandoEvents
    {
        public static event CommandPrefixChange CommandPrefixChange;
        public static event CommandStatusChange CommandStatusChange;
        public static event GroupStatusChange GroupStatusChange;
        public static event CommandRegister CommandRegister;
        public static event GroupRegister GroupRegister;

        public static void InvokeCommandPrefixChange(DiscordGuild guild, string prefix) =>
            CommandPrefixChange?.Invoke(guild, prefix);
        public static void InvokeCommandStatusChange(DiscordGuild guild, Command command, bool enabled) =>
            CommandStatusChange?.Invoke(guild, command, enabled);
        public static void InvokeGroupStatusChange(DiscordGuild guild, CommandGroup group, bool enabled) =>
            GroupStatusChange?.Invoke(guild, group, enabled);
        public static void InvokeCommandRegister(Command command) =>
            CommandRegister?.Invoke(command);
        public static void InvokeGroupRegister(CommandGroup group) =>
            GroupRegister?.Invoke(group);

    } 
}