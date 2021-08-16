using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.Providers
{
    public abstract class SettingsProvider
    {
        public virtual Task Init(CommandoClient client) =>
            throw new NotImplementedException("Override this method bruh");
        public virtual Task Destroy() =>
            throw new NotImplementedException("Override this method bruh");
        public abstract T Get<T>(DiscordGuild guild, string key, T defVal);
        public abstract Task<T> Set<T>(DiscordGuild guild, string key, T val);
        public abstract Task<object> Remove(DiscordGuild guild, string key);
        public abstract Task Clear(DiscordGuild guild);
    }
}