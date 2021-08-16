using CommandoSharpPlus.CommandStuff;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.Commands.Util
{
    public class Ping : Command
    {
        public override string Name => "ping";
        public override string GroupId => "Util";
        public override string MemberName => "ping";
        public override string Description => "Checks the bot's ping to the discord servers.";
        public override ThrottlingOptions ThrottlingOptions => new() { Usage = 5, Duration = 10};
        
        public override async void Run(DiscordMessage message, ArgumentCollector collector)
        {
            await message.RespondAsync(Client.Ping.ToString());
        }
    }
}