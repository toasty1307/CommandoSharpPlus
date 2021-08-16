using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandoSharpPlus.CommandStuff;
using DSharpPlus;
using DSharpPlus.Entities;

namespace CommandoSharpPlus.Utils
{
    public static class MessageExtensions
    {
        public static CommandoClient Client { get; set; }
        public static Dictionary<ulong, MessageSettings> MessageSettingsMap { get; set; } = new();
        public static DiscordMessage InitCommand(this DiscordMessage message, Command command, string argString,
            MatchCollection matches)
        {
            if (!MessageSettingsMap.ContainsKey(message.Id)) MessageSettingsMap.Add(message.Id, new MessageSettings());
            var @this = MessageSettingsMap[message.Id];
            @this.IsCommand = true;
            @this.Command = command;
            @this.ArgString = argString;
            @this.PatternMatches = matches;
            return message;
        }

        public static string Usage(this DiscordMessage message, string argString, string prefix, DiscordUser user) =>
            MessageSettingsMap[message.Id].Command.Usage(argString, prefix, user);
        public static string Usage(this DiscordMessage message, string argString, string prefix) => 
            Usage(message, argString, prefix, Client.CurrentUser);
        public static string Usage(this DiscordMessage message, string argString) => 
            Usage(message, argString, message.Channel.Type == ChannelType.Private ? Client.CommandPrefix : message.Channel.Guild.GetCommandPrefix(), Client.CurrentUser);
        public static string Usage(this DiscordMessage message, string argString, DiscordUser user) =>
            Usage(message, argString, message.Channel.Type == ChannelType.Private ? Client.CommandPrefix : message.Channel.Guild.GetCommandPrefix(), user);

        public static string AnyUsage(this DiscordMessage message, Command command, string prefix, DiscordUser user) =>
            Command.Usage(command, prefix, user);
        public static string AnyUsage(this DiscordMessage message, Command command, string prefix) =>
            AnyUsage(message, command, prefix, Client.CurrentUser);

        public static string[] ParseArgs(this DiscordMessage message)
        {
            var @this = MessageSettingsMap[message.Id];
            switch (@this.Command.ArgsType)
            {
                case ArgsType.Single:
                    return new []{Regex.Replace(@this.ArgString.Trim(), @this.Command.ArgsSingleQuotes ? new Regex("/^(\"|')([^]*)\\1$/g$").ToString() : new Regex("/^(\")([^]*)\"$/g").ToString(), "$2")};
                case ArgsType.Multiple:
                    return ParseArgs(@this.ArgString, @this.Command.ArgsCount, @this.Command.ArgsSingleQuotes);
                default:
                    throw new ArgumentOutOfRangeException(nameof(ArgsType));
            }
        }

        public static string[] ParseArgs(string argsString, int argsCount, bool allowSingleQuote = true)
        {
            var argsStringModified = RemoveSmartQuotes(argsString, allowSingleQuote);
            var regex = new Regex(allowSingleQuote ? "/\\s*(?:(\"|\')([^]*?)\\1|(\\S+))\\s*/g" : "/\\s*(?:(\")([^]*?)\"|(\\S+))\\s*/g");
            var result = new List<string>();
            Match match = null;
            argsCount = argsCount == 0 ? argsStringModified.Length : argsCount;
            while(--argsCount != 0 && (match = regex.Match(argsStringModified)).Success) result.Add(match.Value);
            if (match is null || regex.ToString().Length >= argsStringModified.Length) return result.ToArray();
            var regex2 = allowSingleQuote ?  "/^(\"|\')([^]*)\\1$/g" : "/^(\")([^]*)\"$/g";
            result.Add(Regex.Replace(argsStringModified.Substring(regex.ToString().Length), regex2, "$2"));
            return result.ToArray();
        }

        public static async Task<DiscordMessage[]> Run(this DiscordMessage message)
        {
            throw new NotImplementedException("ffs make the run method");
        }

        public static async Task<DiscordMessage[]> Respond(this DiscordMessage message, ResponseOptions options)
        {
            var @this = MessageSettingsMap[message.Id];
            var shouldEdit = @this.Responses is not null && !options.FromEdit;
            if (options.Type == ResponseOptions.ResponseType.Reply && message.Channel.IsPrivate)
                options.Type = ResponseOptions.ResponseType.Plain;
            if (options.Type != ResponseOptions.ResponseType.Direct)
                if (message.Channel.Guild is not null && !message.Channel
                    .PermissionsFor(message.Channel.Guild.Members[message.Author.Id])
                    .HasPermission(Permissions.SendMessages))
                    options.Type = ResponseOptions.ResponseType.Direct;
            switch (options.Type)
            {
                case ResponseOptions.ResponseType.Plain:
                    if (!shouldEdit)
                        return new []{await message.Channel.SendMessageAsync(options.Content)};
                    return await message.EditCurrentResponse(message.ChannelId, options);
                case ResponseOptions.ResponseType.Reply:
                    if (!shouldEdit) return new[] {await message.RespondAsync(options.Options)};
                    return await message.EditCurrentResponse(message.ChannelId, options);
                case ResponseOptions.ResponseType.Direct:
                    if (@shouldEdit)
                        if (message.Channel.Guild is not null)
                            return new[]
                            {
                                await (await message.Channel.Guild.Members[message.Author.Id].CreateDmChannelAsync())
                                    .SendMessageAsync(options.Options)
                            };
                    break;
                case ResponseOptions.ResponseType.Code:
                    if (!shouldEdit) return new[] {await message.Channel.SendMessageAsync(options.Options)};
                    options.Options.Content = $"```{options.Lang}\n{options.Options.Content}\n```";
                    return await message.EditCurrentResponse(message.ChannelId, options);
                default:
                    throw new ArgumentOutOfRangeException();
            }
            throw new ArgumentOutOfRangeException();
        }

        private static Task<DiscordMessage[]> EditCurrentResponse(this DiscordMessage message, ulong messageChannelId, ResponseOptions options)
        {
            var @this = MessageSettingsMap[messageChannelId];
            if (!@this.Responses.ContainsKey(messageChannelId))
                @this.Responses.Add(messageChannelId, Array.Empty<DiscordMessage>());
            if (!@this.ResponsePosition.ContainsKey(messageChannelId))
                @this.ResponsePosition.Add(messageChannelId, -1);
            @this.ResponsePosition[messageChannelId]++;
            return message.EditResponse(new[]{@this.Responses[messageChannelId][@this.ResponsePosition[messageChannelId]]}, options);
        }

        public static async Task<DiscordMessage[]> Say(this DiscordMessage message, string content) =>
            await message.Respond(new ResponseOptions {Type = ResponseOptions.ResponseType.Plain, Content = content});
        public static async Task<DiscordMessage[]> Reply(this DiscordMessage message, string content) =>
            await message.Respond(new ResponseOptions {Type = ResponseOptions.ResponseType.Reply, Content = content});
        public static async Task<DiscordMessage[]> Direct(this DiscordMessage message, string content) =>
            await message.Respond(new ResponseOptions {Type = ResponseOptions.ResponseType.Direct, Content = content});
        public static async Task<DiscordMessage[]> Code(this DiscordMessage message, string lang, string content) =>
            await message.Respond(new ResponseOptions {Type = ResponseOptions.ResponseType.Code, Content = content});
        public static async Task<DiscordMessage[]> Embed(this DiscordMessage message, DiscordEmbed embed, DiscordMessageBuilder options, string content = "")
        {
            options.Embed = embed;
            options.Content = content;
            return await message.Respond(new ResponseOptions
                {Type = ResponseOptions.ResponseType.Plain, Content = content, Options = options});
        }
        public static async Task<DiscordMessage[]> ReplyEmbed(this DiscordMessage message, DiscordEmbed embed, DiscordMessageBuilder options, string content = "")
        {
            options.Embed = embed;
            options.Content = content;
            return await message.Respond(new ResponseOptions
                {Type = ResponseOptions.ResponseType.Reply, Content = content, Options = options});
        }

        private static async Task<DiscordMessage[]> EditResponse(this DiscordMessage message, DiscordMessage[] response,
            ResponseOptions options)
        {
            if (response is null)
                return await message.Respond(new ResponseOptions
                    {Options = options.Options, Content = options.Content, Type = options.Type, FromEdit = true});
            return new[]{await message.ModifyAsync(options.Content)}; 
        }
        
        public static void Finalize(this DiscordMessage message/*, DiscordMessage[][]*/)
        {
            MessageSettingsMap[message.Id].Responses = new Dictionary<ulong, DiscordMessage[]>();
            MessageSettingsMap[message.Id].ResponsePosition = new Dictionary<ulong, int>();
        }

        private static string RemoveSmartQuotes(string argsString, bool allowSingleQuote)
        {
            var replacementArgsString = argsString;
            var singleSmartQuote = new Regex("/[‘’]/g");
            var doubleSmartQuote = new Regex("/[“”]/g");
            if (allowSingleQuote) replacementArgsString = Regex.Replace(argsString, singleSmartQuote.ToString(), "\'");
            return Regex.Replace(replacementArgsString, doubleSmartQuote.ToString(), "\"");
        }
    }

    public class ResponseOptions
    {
        public ResponseType Type { get; set; }
        public string Content { get; set; }
        public DiscordMessageBuilder Options { get; set; }
        public string Lang { get; set; } = "";
        public bool FromEdit { get; set; }
        
        public enum ResponseType
        {
            Plain,
            Reply,
            Direct,
            Code
        } 
    }
}