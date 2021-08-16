using System;
using CommandoSharpPlus.CommandStuff;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.Types
{
    public abstract class ArgumentType
    {
        public CommandoClient Client { get; set; }
        public ArgumentTypes Type { get; set; }

        public ArgumentType(CommandoClient client, ArgumentTypes type)
        {
            Client = client ?? throw new Exception("Client cannot be null");
            Type = type;
        }

        public abstract bool Validate(string val, DiscordMessage message, Argument arg, DiscordMessage currentMessage);
        public bool Validate(string val, DiscordMessage message, Argument arg) => Validate(val, message, arg, message);
        public abstract object Parse(string val, DiscordMessage message, Argument arg, DiscordMessage currentMessage);
        public object Parse(string val, DiscordMessage message, Argument arg) => Parse(val, message, arg, message);
        public T Parse<T>(string val, DiscordMessage message, Argument arg, DiscordMessage currentMessage) where T : class => Parse(val, message, arg, currentMessage) as T;
        public T Parse<T>(string val, DiscordMessage message, Argument arg) where T : class => Parse<T>(val, message, arg, message);
        public bool IsEmpty(object val, DiscordMessage message, Argument arg, DiscordMessage currentMessage)
        {
            if (val is null) return true;
            if (val.GetType().IsArray) return (val as object[])?.Length == 0;
            return true;
        }
        public bool IsEmpty(string val, DiscordMessage message, Argument arg) => IsEmpty(val, message, arg, message);
    }
}