using System;
using DSharpPlus;

namespace CommandoSharpPlus
{
    public class CommandoConfiguration : DiscordConfiguration
    {
        public string CommandPrefix { get; set; } = "!";
        public int CommandEditableDuration { get; set; } = 30;
        public bool NonCommandEditable { get; set; } = true;
        public ulong[] Owners { get; set; } = Array.Empty<ulong>();
        public string Invite { get; set; }
    }
}