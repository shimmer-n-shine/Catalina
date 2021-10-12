
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalina.Database;
using Microsoft.EntityFrameworkCore;
using Discord;
using Fergun.Interactive;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using NLog;
using System.Threading;

namespace Catalina.Discord
{
    public class Discord
    {
        public static DiscordSocketClient discord;
        public static List<IGuildChannel?> commandChannels;
        public static InteractiveService interactivity;
        public static CommandService commandService;
        public static async Task SetupClient()
        {
            var logger = LogManager.GetCurrentClassLogger();
            
            discord = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.All , //remove all later
                //Token = Environment.GetEnvironmentVariable(AppProperties.DiscordToken),
                //TokenType = TokenType.Bot,
                AlwaysDownloadUsers = true,
            });

            commandService = new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false,
                LogLevel = LogSeverity.Debug,
                IgnoreExtraArgs = false,
            });

            await commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), null);

            interactivity = new InteractiveService(discord as BaseSocketClient, TimeSpan.FromSeconds(30));



            discord.UserJoined += Events.Discord_GuildMemberAdded;
            discord.MessageDeleted += Events.Discord_MessageDeleted;
            discord.MessageReceived += Events.Discord_MessageCreated;
            discord.ReactionAdded += Events.Discord_ReactionAdded;
            discord.ReactionRemoved += Events.Discord_ReactionRemoved;
            discord.ReactionsCleared += Events.Discord_ReactionsCleared;
            discord.Ready += Events.Discord_Ready;

            discord.Log += Events.Discord_Log;

            await discord.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable(AppProperties.DiscordToken));

            await discord.StartAsync();
        }
        public static void UpdateChannels()
        {
            new Thread(() =>
            {
                using var database = new DatabaseContextFactory().CreateDbContext();
                commandChannels = new List<IGuildChannel>();
                var guildProperties = database.GuildProperties.AsNoTracking();

                if (guildProperties.All(g => string.IsNullOrEmpty(g.CommandChannelsSerialised)))
                {
                    return;
                }
                foreach (var guild in guildProperties.Where(g => !string.IsNullOrEmpty(g.CommandChannelsSerialised)))
                {
                    foreach (var channel in guild.CommandChannels)
                    {
                        var discordGuild = discord.GetGuild(guild.ID);
                        try
                        {
                            commandChannels.Add(discordGuild.GetTextChannel(channel));
                        }
                        catch (Exception e)
                        {
                            LogManager.GetCurrentClassLogger().Info(e, "Error when getting channel id " + channel);
                        }
                    }

                }
            }).Start();
        }
    }
}