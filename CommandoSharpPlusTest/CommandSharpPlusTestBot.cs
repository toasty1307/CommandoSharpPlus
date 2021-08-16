using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandoSharpPlus;
using CommandoSharpPlus.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using SQLitePCL;

namespace CommandoSharpPlusTest
{
    public class CommandSharpPlusTestBot
    {
        public CommandoClient Client { get; set; }
        
        public CommandSharpPlusTestBot()
        {
            var config = GetConfig("Config.json");
            var commandoConfig = new CommandoConfiguration
            {
                Token = config.Token,
                CommandPrefix = config.CommandPrefix,
                Intents = DiscordIntents.All,
                LoggerFactory = new LoggerFactory(),
            };
            Client = new CommandoClient(commandoConfig);
            Client.Registry.RegisterDefaults();
            Client.Ready += async (_, _) => await Client.UpdateStatusAsync(new DiscordActivity("drinking milk", ActivityType.Playing),
                UserStatus.Idle);
        }

        public async Task Run()
        {
            try
            {
                await Client.ConnectAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Config GetConfig(string fileName)
        {
            var completePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            var reader = new StreamReader(completePath);
            var completeString = reader.ReadToEnd();
            var config = JsonConvert.DeserializeObject<Config>(completeString);
            return config;
        }
    }
}