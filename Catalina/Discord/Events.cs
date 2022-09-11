using Discord;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Catalina.Database;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Discord.Interactions;
using System.Linq;

namespace Catalina.Discord
{
    class Events
    {
        

        internal static async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot || reaction.User.Value.Discriminator == "0000") return;

            using var database = new DatabaseContextFactory().CreateDbContext();

            var guild = (channel.Value as IGuildChannel).Guild;
            if (database.GuildProperties.Any(g => g.ID == guild.Id))
            {
                Database.Models.GuildProperty guildProperty = null;
                try
                {
                    //guildProperty = database.GuildProperties.Include(g => g.StarboardEmoji).First(g => g.ID == guild.Id);
                    guildProperty = database.GuildProperties.First(g => g.ID == guild.Id);
                } 
                catch
                {

                }
                

                var emoji = await Database.Models.Emoji.ParseAsync(reaction.Emote, guild);

                if (emoji.NameOrID == guildProperty.Starboard.Emoji.NameOrID)
                {
                    await Starboard.ProcessVote(guildProperty, await message.GetOrDownloadAsync(), reaction.User.Value);
                }
            }
            

        }

        internal static async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            await using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task MessageCreated(SocketMessage arg)
        {
            await using var database = new DatabaseContextFactory().CreateDbContext();

        }

        internal static async Task ReactionsCleared(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            await using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task GuildMemberAdded(SocketGuildUser user)
        {
            await using var database = new DatabaseContextFactory().CreateDbContext();

            //var guildProperty = database.GuildProperties.Include(g => g.Roles).FirstOrDefault(g => g.ID == user.Guild.Id);
            var guildProperty = database.GuildProperties.FirstOrDefault(g => g.ID == user.Guild.Id);
            if (guildProperty is null) return;

            try
            {
                await user.AddRolesAsync(guildProperty.Roles.Where(r => r.IsAutomaticallyAdded).Select(r => r.ID));
            }
            catch
            {
                NLog.LogManager.GetCurrentClassLogger().Error("Could not add automatic roles to user");
            }
            

        }

        internal static async Task MessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            await using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task InteractionCreated(SocketInteraction socketInteraction)
        {
            var context = new SocketInteractionContext(Discord.DiscordClient, socketInteraction);
            await TickGuild(context);
            await Discord.InteractionService.ExecuteCommandAsync(context, null);
        }

        internal static async Task GuildPingAsync(ulong guildID)
        {
            await using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task LeftGuild(SocketGuild arg)
        {
            await using var database = new DatabaseContextFactory().CreateDbContext();
        }

        internal static async Task JoinedGuild(SocketGuild arg)
        {
            await using var database = new DatabaseContextFactory().CreateDbContext();
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
                #if DEBUG
                if (ulong.TryParse(Environment.GetEnvironmentVariable(AppProperties.DeveloperGuildID), out ulong guildID)) 
                {
                    await Discord.InteractionService.RegisterCommandsToGuildAsync(guildID);
                }
                #else
                await Discord.InteractionService.RegisterCommandsGloballyAsync();
                #endif
            }
            await Discord.DiscordClient.SetGameAsync(type: ActivityType.Watching, name: "Jerma985.");
            NLog.LogManager.GetCurrentClassLogger().Info("Discord Ready!");
        }

        internal static async Task TickGuild(IInteractionContext context)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();

            if (database.GuildProperties.Find(context.Guild.Id) == null)
            {
                var guildProperty = new Database.Models.GuildProperty { ID = context.Guild.Id, Starboard = new Database.Models.Starboard { } };
                database.GuildProperties.Add(guildProperty);

                await database.SaveChangesAsync();

                guildProperty.Starboard.SetOrCreateEmoji(database.Emojis.AsNoTracking().FirstOrDefault(e => e.NameOrID == ":star:"), database);

                await database.SaveChangesAsync();
                
            }
        }
    }
}