using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using System.Transactions;
using CommandoSharpPlus.CommandStuff;
using CommandoSharpPlus.Utils;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CommandoSharpPlus.Providers
{
    public class SQLiteProvider : SettingsProvider
    {
        public SqliteConnection Connection { get; set; }
        public CommandoClient Client { get; set; }
        public static Dictionary<ulong, GuildSettings> Settings { get; set; } = new();
        public SqliteCommand InsertOrReplaceCommand { get; set; }
        public SqliteCommand DeleteCommand { get; set; }
        public CommandPrefixChange CommandPrefixChange;
        public CommandStatusChange CommandStatusChange;
        public GroupStatusChange GroupStatusChange;
        public CommandRegister CommandRegister;
        public GroupRegister GroupRegister;
        public Action<object, GuildCreateEventArgs> GuildCreated;

        public SQLiteProvider(SqliteConnection connection)
        {
            Connection = connection;
            Client = null;
            InsertOrReplaceCommand = null;
            DeleteCommand = null;
        }

        public override async Task Init(CommandoClient client)
        {
            Client = client;
            var createTableCommand = Connection.CreateCommand();
            createTableCommand.CommandText = "CREATE TABLE IF NOT EXISTS Settings (Guild INTEGER PRIMARY KEY, Settings TEXT)";
            await createTableCommand.PrepareAsync();
            await createTableCommand.ExecuteNonQueryAsync();
            var loadSettingsCommand = Connection.CreateCommand();
            loadSettingsCommand.CommandText = "SELECT CAST(Guild as TEXT) as Guild, Settings FROM Settings";
            await loadSettingsCommand.PrepareAsync();
            var reader = await loadSettingsCommand.ExecuteReaderAsync();
            while (reader.Read())
            {
                ulong guild;
                GuildSettings settings;
                try
                {
                    guild = (ulong)reader.GetInt64(0);
                    settings = JsonConvert.DeserializeObject<GuildSettings>(reader.GetString(1));
                }
                catch
                {
                    Client.Logger.LogWarning("Unable To load Settings for guilds");
                    continue;
                }
                Settings.Add(guild, settings);
                if (guild == 0 && !Client.Guilds.ContainsKey(guild)) continue;
                SetUpGuild(guild, settings);
            }

            InsertOrReplaceCommand = Connection.CreateCommand();
            InsertOrReplaceCommand.CommandText = "INSERT OR REPLACE INTO Settings VALUES(GUILD, SETTINGS)";
            
            CommandPrefixChange = (guild, prefix) => Set(guild, "Prefix", prefix);
            CommandStatusChange = (guild, command, enabled) => Set(guild, $"Command-{command.Name}", enabled);
            GroupStatusChange = (guild, group, enabled) => Set(guild, $"Group-{group.Name}", enabled);
            GroupRegister = group =>
            {
                foreach (var (guild, settings) in Settings)
                {
                    if (guild != 0 && !Client.Guilds.ContainsKey(guild)) continue;
                    SetupGuildGroup(Client.Guilds[guild], group, settings);
                }
            };
            CommandRegister = command =>
            {
                foreach (var (guild, settings) in Settings)
                {
                    if (guild != 0 && !Client.Guilds.ContainsKey(guild)) continue;
                    SetupGuildCommand(Client.Guilds[guild], command, settings);
                }
            };
            GuildCreated = (_, args) =>
            {
                if (!Settings.ContainsKey(args.Guild.Id)) return;
                var settings = Settings[args.Guild.Id];
                SetUpGuild(args.Guild.Id, settings);
            };

            DeleteCommand = Connection.CreateCommand();
            DeleteCommand.CommandText = "DELETE FROM Settings WHERE Guild = GUILD";

            CommandoEvents.CommandPrefixChange += CommandPrefixChange;
            CommandoEvents.CommandStatusChange += CommandStatusChange;
            CommandoEvents.GroupStatusChange += GroupStatusChange;
            CommandoEvents.CommandRegister += CommandRegister;
            CommandoEvents.GroupRegister += GroupRegister;
        }

        private void SetupGuildCommand(DiscordGuild clientGuild, Command command, GuildSettings settings)
        {
            if (clientGuild is not null)
            {
                var guildSettings = clientGuild.GetSettings();
                guildSettings.CommandStatuses[command] = settings.CommandStatuses[command];
            }
            else
            {
                command.GlobalEnabled = settings.CommandStatuses[command];
            }
        }

        private void SetupGuildGroup(DiscordGuild clientGuild, CommandGroup group, GuildSettings settings)
        {
            if (clientGuild is not null)
            {
                var guildSettings = clientGuild.GetSettings();
                guildSettings.GroupStatuses[group] = settings.GroupStatuses[group];
            }
            else
            {
                group.GlobalEnabled = settings.GroupStatuses[group];
            }
        }

        private void SetUpGuild(ulong guildId, GuildSettings settings)
        {
            var guild = Client.Guilds.ContainsKey(guildId) ? Client.Guilds[guildId] : null;
            if (settings.Prefix is not null && !string.IsNullOrWhiteSpace(settings.Prefix))
            {
                if (guild is not null) guild.SetCommandPrefix(settings.Prefix);
                else Client.Options.CommandPrefix = settings.Prefix;
            }

            foreach (var command in Client.Registry.Commands) SetupGuildCommand(guild, command, settings);
            foreach (var group in Client.Registry.Groups) SetupGuildGroup(guild, group, settings);
        }

        public override Task Destroy()
        {
            CommandoEvents.CommandPrefixChange -= CommandPrefixChange;
            CommandoEvents.CommandStatusChange -= CommandStatusChange;
            CommandoEvents.GroupStatusChange -= GroupStatusChange;
            CommandoEvents.CommandRegister -= CommandRegister;
            CommandoEvents.GroupRegister -= GroupRegister;
            return Task.CompletedTask;
        }

        public override T Get<T>(DiscordGuild guild, string key, T defVal)
        {
            if (!Settings.ContainsKey(guild.Id)) return defVal;
            var settings = Settings[guild.Id];
            return settings is T settingsT ? settingsT : default;
        }

        public override Task<T> Set<T>(DiscordGuild guild, string key, T val)
        {
            GuildSettings settings;
            if (!Settings.ContainsKey(guild.Id))
            {
                settings = new GuildSettings();
                Settings.Add(guild.Id, settings);
            }
            else
                settings = Settings[guild.Id];
            
            settings[key] = val;
            InsertOrReplaceCommand.Parameters.AddWithValue("GUILD", guild.Id);
            InsertOrReplaceCommand.Parameters.AddWithValue("SETTINGS", JsonConvert.SerializeObject(settings));
            InsertOrReplaceCommand.ExecuteNonQuery();
            InsertOrReplaceCommand.CommandText = "INSERT OR REPLACE INTO Settings VALUES(GUILD, SETTINGS)";
            return new Task<T>(() => val);
        }

        public override Task<object> Remove(DiscordGuild guild, string key)
        {
            GuildSettings settings;
            if (!Settings.ContainsKey(guild.Id) || (settings = Settings[guild.Id]) == null) return null;
            var val = settings[key];
            InsertOrReplaceCommand.Parameters.AddWithValue("GUILD", guild.Id);
            InsertOrReplaceCommand.Parameters.AddWithValue("SETTINGS", JsonConvert.SerializeObject(settings));
            InsertOrReplaceCommand.ExecuteNonQuery();
            InsertOrReplaceCommand.CommandText = "INSERT OR REPLACE INTO Settings VALUES(GUILD, SETTINGS)";
            return new Task<object>(() => val);
        }

        public override Task Clear(DiscordGuild guild)
        {
            if (!Settings.ContainsKey(guild.Id)) return Task.CompletedTask;
            Settings.Remove(guild.Id);
            DeleteCommand.Parameters.AddWithValue("GUILD", guild.Id);
            DeleteCommand.ExecuteNonQuery();
            DeleteCommand.CommandText = "DELETE FROM Settings WHERE Guild = GUILD";
            return Task.CompletedTask;
        }
    }
}