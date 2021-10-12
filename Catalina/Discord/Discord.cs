using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalina.Database;
using Microsoft.EntityFrameworkCore;

namespace Catalina.Discord
{
    public class Discord
    {
        static SerilogLoggerFactory logFactory;
        public static DiscordClient discord;
        public static List<DiscordChannel?> commandChannels;
        public static async Task SetupClient()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console()
                .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
                .CreateLogger();
            logFactory = new SerilogLoggerFactory();

            discord = new DiscordClient(new DiscordConfiguration()
            {
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                Intents = DiscordIntents.All,
                Token = Environment.GetEnvironmentVariable(AppProperties.DiscordToken),
                TokenType = TokenType.Bot,
                AlwaysCacheMembers = true,
                LoggerFactory = logFactory
            });
            discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { Environment.GetEnvironmentVariable(AppProperties.BotPrefix) },
                CaseSensitive = false,
            });

            //commands.RegisterCommands<CoreModule>();

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            //discord.GuildMemberAdded += Events.Discord_GuildMemberAdded;
            //discord.MessageDeleted += Events.Discord_MessageDeleted;
            //discord.MessageReactionAdded += Events.Discord_ReactionAdded;
            //discord.MessageReactionRemoved += Events.Discord_ReactionRemoved;
            //discord.MessageReactionsCleared += Events.Discord_ReactionsCleared;

            var discordActivity = new DiscordActivity
            {
                ActivityType = ActivityType.ListeningTo,
                Name = "The beat of your heart..."
            };

            await discord.ConnectAsync(discordActivity);
            await UpdateChannels();
        }
        public static async Task UpdateChannels()
        {
            using var database = new DatabaseContextFactory().CreateDbContext();

            commandChannels = new List<DiscordChannel>();
            var guildProperties = database.GuildProperties.AsNoTracking();
            if (guildProperties.All(g => string.IsNullOrEmpty(g.CommandChannelsSerialised)))
            {
                return;
            }
            foreach (var channels in guildProperties.Where(g => !string.IsNullOrEmpty(g.CommandChannelsSerialised)).Select(g => g.CommandChannels))
            {
                foreach (var channel in channels)
                {
                    try
                    {
                        commandChannels.Add(await discord.GetChannelAsync(channel));
                    }
                    catch (Exception e)
                    {
                        Log.Information(e.GetType() + " error when getting channel id " + channel);
                    }
                }

            }

        }
    }
}