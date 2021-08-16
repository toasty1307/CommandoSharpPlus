using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.Providers
{
    public class GuildSettingsHelper : SettingsProvider
    {
        public CommandoClient Client { get; set; }
        public DiscordGuild Guild { get; set; }

        public GuildSettingsHelper(CommandoClient client, DiscordGuild guild)
        {
            Client = client;
            Guild = guild;
        }

        public override T Get<T>(DiscordGuild guild, string key, T defVal)
        {
            if (Client.Provider is null) throw new NullReferenceException("No Provider hmm");
            return Client.Provider.Get(Guild, key, defVal);
        }

        public override Task<T> Set<T>(DiscordGuild guild, string key, T val)
        {
            if (Client.Provider is null) throw new NullReferenceException("No Provider hmm");
            return Client.Provider.Set(Guild, key, val);
        }

        public override Task<object> Remove(DiscordGuild guild, string key)
        {
            if (Client.Provider is null) throw new NullReferenceException("No Provider hmm");
            return Client.Provider.Remove(Guild, key);
        }

        public override Task Clear(DiscordGuild guild)
        {
            if (Client.Provider is null) throw new NullReferenceException("No Provider hmm");
            return Client.Provider.Clear(Guild);
        }
    }
}