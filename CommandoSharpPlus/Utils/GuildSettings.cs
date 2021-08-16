using System.Collections.Generic;
using CommandoSharpPlus.CommandStuff;

namespace CommandoSharpPlus.Utils
{
    public class GuildSettings
    {
        public string Prefix { get; set; }
        public Dictionary<Command, bool> CommandStatuses { get; set; }
        public Dictionary<CommandGroup, bool> GroupStatuses { get; set; }

        public object this[string key]
        {
            get => GetType().GetProperty(key)!.GetGetMethod()!.Invoke(this, null);
            set => GetType().GetProperty(key)!.GetSetMethod()!.Invoke(this, new [] {value});
        }
    }
}