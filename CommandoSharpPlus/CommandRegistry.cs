using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using CommandoSharpPlus.CommandStuff;
using CommandoSharpPlus.Types;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace CommandoSharpPlus
{
    public class CommandRegistry
    {
        public CommandoClient Client { get; set; }
        public List<Command> Commands { get; set; } = new();
        public List<CommandGroup> Groups { get; private set; } = new();
        public Command UnknownCommand { get; set; }
        public List<ArgumentType> Types { get; set; }

        public CommandRegistry(CommandoClient client)
        {
            Client = client;
        }

        public CommandRegistry RegisterGroup(CommandGroup group)
        {
            if (Groups.Contains(group))
            {
                Client.Logger.LogWarning("Group: {0} is already registered", group.Name);
                return this;
            }
            
            Groups.Add(group);
            CommandoEvents.InvokeGroupRegister(group);
            Client.Logger.LogDebug("Registered group: {0}", group.Id);
            return this;
        }

        public CommandRegistry RegisterGroups(CommandGroup[] groups)
        {
            foreach (var commandGroup in groups)
                RegisterGroup(commandGroup);
            return this;
        }

        public CommandRegistry RegisterCommand(Command command)
        {
            if (Commands.Any(x =>
                x.Name == command.Name || x.Aliases.ToList().Contains(command.Name) ||
                x.Aliases.ToList().Intersect(command.Aliases).Any()))
                throw new Exception(
                    $"Command with same name/alias {command.Name}/({string.Join("/", command.Aliases)}) already exists");
            if (Groups.All(x => x.Id != command.GroupId))
                throw new Exception($"Group {command.GroupId} is not registered");
            var group = Groups.First(x => x.Id == command.GroupId);
            if (group.Commands.Any(x => x.MemberName == command.MemberName))
                throw new Exception(
                    $"A command with the member name {command.MemberName} is already registered in {group.Id}");
            if (command.Unknown && UnknownCommand is not null)
                throw new Exception("An unknown command is already registered");
            command.Group = group;
            group.Commands.Add(command);
            Commands.Add(command);
            if (command.Unknown) UnknownCommand = command;
            CommandoEvents.InvokeCommandRegister(command);
            Client.Logger.LogDebug("Registered command {0}:{1}", group.Id, command.MemberName);
            return this;
        }

        public CommandRegistry RegisterCommands(Command[] commands)
        {
            foreach (var command in commands)
                RegisterCommand(command);
            return this;
        }

        public CommandRegistry RegisterCommandsIn(Assembly assembly)
        {
            var allTypes = assembly.GetTypes().ToList();
            var allTypesButCommands = allTypes.Where(x => x.IsSubclassOf(typeof(Command)));
            var allCommandObjects = allTypesButCommands.Select(x => (Command)Activator.CreateInstance(x)).ToArray();
            RegisterCommands(allCommandObjects);
            return this;
        }

        public CommandRegistry RegisterType(ArgumentType type)
        {
            if (Types.Contains(type)) throw new Exception($"The Argument type {type.Type} already exists");
            Types.Add(type);
            // TODO type register event
            return this;
        }

        public CommandRegistry RegisterTypes(ArgumentType[] types)
        {
            foreach (var argumentType in types)
                RegisterType(argumentType);
            return this;
        }

        public CommandRegistry RegisterTypesIn(Assembly assembly)
        {
            return RegisterTypes(assembly.GetTypes().ToList().Where(x => x.IsSubclassOf(typeof(ArgumentType)))
                .Select(x => x.GetConstructor(new[] {typeof(CommandoClient)})!.Invoke(new object[] {Client}))
                .ToArray() as ArgumentType[]);
        }

        public CommandRegistry RegisterDefaults()
        {
            RegisterGroups(new CommandGroup[]
            {
                new(Client, "Commands", "Commands", true),
                new(Client, "Util", "Utility")
            });
            RegisterCommandsIn(GetType().Assembly);
            // RegisterTypesIn(GetType().Assembly);
            return this;
        }

        public CommandRegistry UnregisterCommand(Command command)
        {
            if (!Commands.Contains(command)) return this;
            Commands.Remove(command);
            command.Group.Commands.Remove(command);
            if (UnknownCommand == command) UnknownCommand = null;
            // TODO unregister command event
            return this;
        }

        public Command[] FindCommands(string searchString = null, bool exact = false, DiscordMessage message = null)
        {
            if (searchString is null)
                return message is not null ? Commands.Where(x => x.IsUsable(message)).ToArray() : Commands.ToArray();
            var lcSearch = searchString.ToLower();
            var matchedCommands = Commands.Where(exact
                ? x => x.Name == lcSearch || x.Aliases.Any(al => al == lcSearch) ||
                       $"{x.GroupId}:{x.MemberName}" == lcSearch
                : x => x.Name.Contains(lcSearch) || x.Aliases.Any(al => al.Contains(lcSearch)) ||
                       $"{x.GroupId}:{x.MemberName}" == lcSearch);
            if (exact) return matchedCommands.ToArray();

            var enumerable = matchedCommands as Command[] ?? matchedCommands.ToArray();
            foreach (var command in enumerable)
                if (command.Name == lcSearch ||
                    command.Aliases is not null && command.Aliases.Any(ali => ali == lcSearch))
                    return new[] {command};
            return enumerable;
        }
    }
}