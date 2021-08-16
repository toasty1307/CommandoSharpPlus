using System;
using System.Threading.Tasks;
using DSharpPlus;

namespace VanillaDSharpPlusTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new DiscordClient(new DiscordConfiguration
                {Token = "ODc0ODk4MjkyNzcxODA3Mjgy.YRNqhw.XdSsu4kjpo1L2c-Bq59yjQMga4o", Intents = DiscordIntents.All});
            await client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}