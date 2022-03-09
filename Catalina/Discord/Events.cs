using Discord;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Catalina.Database;
using Discord.Commands;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Discord.Interactions;

namespace Catalina.Discord
{
    class Events
    {

        internal static async Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task MessageCreated(SocketMessage arg)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();

        }

        internal static async Task ReactionsCleared(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task GuildMemberAdded(SocketGuildUser user)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task MessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task InteractionCreated(SocketInteraction socketInteraction)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();

            var context = new SocketInteractionContext(Discord.discord, socketInteraction);
            await TickGuild(context);
            await Discord.interactionService.ExecuteCommandAsync(context, null);
        }

        internal static async Task GuildPingAsync(ulong guildID)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task LeftGuild(SocketGuild arg)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task JoinedGuild(SocketGuild arg)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static Task Discord_Log(LogMessage msg)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    if (msg.Exception is not null)
                        logger.Fatal(msg.Exception, $"{msg.Message}");
                    else
                        logger.Fatal($"{msg.Message}");
                    break;
                case LogSeverity.Debug:
                    if (msg.Exception is not null)
                        logger.Debug(msg.Exception, $"{msg.Source}");
                    else
                        logger.Debug($"{msg.Message}");
                    break;
                case LogSeverity.Error:
                    if (msg.Exception is not null)
                        logger.Error(msg.Exception, $"{msg.Message}");
                    else
                        logger.Error($"{msg.Message}");
                    break;
                case LogSeverity.Info:
                    if (msg.Exception is not null)
                        logger.Info(msg.Exception, $"{msg.Message}");
                    else
                        logger.Info($"{msg.Message}");
                    break;
                case LogSeverity.Verbose:
                    if (msg.Exception is not null)
                        logger.Debug(msg.Exception, $"{msg.Message}");
                    else
                        logger.Debug($"{msg.Message}");
                    break;
                case LogSeverity.Warning:
                    if (msg.Exception is not null)
                        logger.Warn(msg.Exception, $"{msg.Message}");
                    else
                        logger.Warn($"{msg.Message}");
                    break;
            };

            return Task.CompletedTask;
        }

        internal static async Task Ready()
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(AppProperties.DeveloperGuildID))) 
            {
                if (ulong.TryParse(Environment.GetEnvironmentVariable(AppProperties.DeveloperGuildID), out ulong guildID)) 
                {
                    await Discord.interactionService.RegisterCommandsToGuildAsync(guildID);
                }
            }
            
            await Discord.discord.SetGameAsync(type: ActivityType.Watching, name: "Jerma985.");
            NLog.LogManager.GetCurrentClassLogger().Info("Discord Ready!");
        }

        internal static async Task TickGuild(IInteractionContext context)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();

            if (database.GuildProperties.Find(context.Guild.Id) == null)
            {
                database.GuildProperties.Add(new Database.Models.GuildProperty { ID = context.Guild.Id });

                await database.SaveChangesAsync();
            }
        }
    }
}