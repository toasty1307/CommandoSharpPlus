using System;
using System.Collections.Generic;
using CommandoSharpPlus.CommandStuff;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.Types
{
    public class Bool : ArgumentType
    {
        public List<string> Truthy { get; set; }
        public List<string> Falsy { get; set; }

        public Bool(CommandoClient client) : base(client, ArgumentTypes.Bool)
        {
            Truthy = new List<string>(new [] {"true", "t", "yes", "y", "on", "enable", "enabled", "1", "+"});
            Falsy = new List<string>(new []{"false", "f", "no", "n", "off", "disable", "disabled", "0", "-"});
        }

        public override bool Validate(string val, DiscordMessage message, Argument _, DiscordMessage __)
        {
            var lower = val.ToLower();
            return Truthy.Contains(lower) || Falsy.Contains(val);
        }

        public override object Parse(string val, DiscordMessage message, Argument arg, DiscordMessage currentMessage)
        {
            var lower = val.ToLower();
            if (Truthy.Contains(lower)) return true;
            if (Falsy.Contains(lower)) return false;
            throw new ArgumentOutOfRangeException("ok do you really need more names of true and false?");
        }
    }
}