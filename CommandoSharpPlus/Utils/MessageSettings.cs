using System.Collections.Generic;
using System.Text.RegularExpressions;
using CommandoSharpPlus.CommandStuff;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.Utils
{
    public class MessageSettings
    {
        public bool IsCommand { get; set; } = false;
        public Command Command { get; set; } = null;
        public string ArgString { get; set; } = null;
        public MatchCollection PatternMatches { get; set; } = null;
        public Dictionary<ulong, DiscordMessage[]> Responses { get; set; }  = null;
        public Dictionary<ulong, int> ResponsePosition { get; set; } = null;
    }
}