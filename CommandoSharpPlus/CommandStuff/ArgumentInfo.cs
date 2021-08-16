using System;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.CommandStuff
{
    public class ArgumentInfo
    {
        public string Key { get; set; }
        public string Label { get; set;}
        public string Prompt { get; set; }
        public string Error { get; set; }
        public ArgumentTypes Type { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public string[] OneOf { get; set; } = null;
        public object Default { get; set; }
        public bool Infinite { get; set; }
        public Func<DiscordMessage, bool> Validate { get; set; }
        public Func<string, DiscordMessage, DiscordMessage, bool> IsEmpty { get; set; }
        public Func<string, DiscordMessage, DiscordMessage, bool> Parse { get; set; }
        public int Wait { get; set; } = 30;
    }

    public enum ArgumentTypes
    {
        Bool,
        CategoryChannel,
        Channel,
        Command,
        CustomEmoji,
        DefaultEmoji,
        Float,
        Group,
        Integer,
        Member,
        Message,
        Role,
        String,
        TextChannel,
        Union,
        User,
        VoiceChannel
    }

    public enum ArgsType
    {
        Single,
        Multiple
    }
}