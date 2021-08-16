using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using CommandoSharpPlus.CommandStuff;
using DSharpPlus;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.Types
{
    public class CategoryChannel : ArgumentType
    {
        public CategoryChannel(CommandoClient client) : base(client, ArgumentTypes.CategoryChannel) { }

        public override bool Validate(string val, DiscordMessage message, Argument arg, DiscordMessage currentMessage)
        {
            var match = Regex.Matches(val, "[0-9]+");

            if (!string.IsNullOrWhiteSpace(match[0].Value))
            {
                try
                { 
                    var channels = Client.Guilds.Values.SelectMany(x => x.Channels.Values).ToList();
                    var channel = channels.First(x => x.Type == ChannelType.Category && x.Id.ToString() == match[0].Value);
                    if (EqualityComparer<DiscordChannel>.Default.Equals(channel, default)) return false; 
                    if (arg.Info.OneOf is not null && arg.Info.OneOf.Contains(channel.Name)) return false;
                    return true;
                }
                catch { return false; }
            }

            if (message.Channel.Guild is null) return false;

            var search = val.ToLower();
            var searchChannels = message.Channel.Guild.Channels.Values.ToList()
                .Where(x => x.Type == ChannelType.Category && x.Name.ToLower().Contains(search)).ToList();
            switch (searchChannels.Count)
            {
                case 0:
                    return false;
                case 1:
                    return !(arg.Info.OneOf is not null && !arg.Info.OneOf.ToList().Contains(searchChannels.First().Id.ToString()));
            }

            var exactChannels = searchChannels.Where(x => x.Name.ToLower() == search).ToList();
            switch (exactChannels.Count)
            {
                case 1:
                    return !(arg.Info.OneOf is not null && !arg.Info.OneOf.ToList().Contains(exactChannels.First().Id.ToString()));
                case > 0:
                    break;
            }
            throw new ArgumentException("many channels with same name, so no");
        }

        public override object Parse(string val, DiscordMessage message, Argument arg, DiscordMessage currentMessage)
        {
            var match = Regex.Matches(val, "[0-9]+");
            if (!string.IsNullOrEmpty(match[0].Value))
                return Client.Guilds.Values.SelectMany(x => x.Channels.Values).Where(x => x.Id == Convert.ToUInt64(match[0].Value));
            if (message.Channel.Guild is null) return false;
            var search = val.ToLower();
            var channels = message.Channel.Guild.Channels.Values.Where(x =>
                x.Type == ChannelType.Category && x.Name.ToLower().Contains(search)).ToList();
            switch (channels.Count)
            {
                case 0:
                    return null;
                case 1:
                    return channels.First();
                default:
                {
                    var exactChannels = channels.Where(x => x.Type == ChannelType.Category && x.Name.ToLower() == search)
                        .ToList();
                    return exactChannels.Count == 1 ? exactChannels.First() : null;
                }
            }
        }
    }
}