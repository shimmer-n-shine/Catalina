using Discord;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Catalina.Database;
using Microsoft.EntityFrameworkCore;
using Discord.Interactions;
using System.Linq;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;

namespace Catalina.Discord;

    public static class Events
    {
        public static ServiceProvider Services;
        internal static async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot || reaction.User.Value.IsWebhook) return;
            using var database = Services.GetRequiredService<DatabaseContext>();

            var guild = (channel.Value as IGuildChannel).Guild;
            if (database.GuildProperties.Any(g => g.ID == guild.Id))
            {
            Database.Models.Guild guildProperty = null;
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
            throw new NotImplementedException("its ok D.NET will catch these");
        }

        internal static async Task MessageCreated(SocketMessage arg)
        {
            throw new NotImplementedException("its ok D.NET will catch these");

        }

        internal static async Task ReactionsCleared(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            throw new NotImplementedException("its ok D.NET will catch these");
            //its ok D.NET will catch these
        }

        internal static async Task GuildMemberAdded(SocketGuildUser user)
        {
            using var database = Services.GetRequiredService<DatabaseContext>();

            var guildProperty = database.GuildProperties.Include(g => g.Roles).FirstOrDefault(g => g.ID == user.Guild.Id);
            if (guildProperty is null) return;

            try
            {
                await user.AddRolesAsync(guildProperty.Roles.Where(r => r.IsAutomaticallyAdded).Select(r => r.ID));
            }
            catch
            {
                Services.GetRequiredService<Logger>().Error("Could not add automatic roles to user");
            }
            

        }

        internal static async Task MessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            throw new NotImplementedException("its ok D.NET will catch these");
        }

        internal static async Task InteractionCreated(SocketInteraction socketInteraction)
        {
            var context = new SocketInteractionContext(Discord.DiscordClient, socketInteraction);
            await TickGuild(context);
            await Discord.InteractionService.ExecuteCommandAsync(context, Services);
        }

        internal static async Task LeftGuild(SocketGuild arg)
        {
            throw new NotImplementedException("its ok D.NET will catch these");
        }

        internal static async Task JoinedGuild(SocketGuild arg)
        {
            throw new NotImplementedException("its ok D.NET will catch these");
        }

        internal static async Task DiscordLog(LogMessage message)
        {
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Verbose => LogEventLevel.Verbose,
                LogSeverity.Debug => LogEventLevel.Debug,
                _ => LogEventLevel.Information
            };

            Services.GetRequiredService<Logger>().Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
            await Task.CompletedTask;
        }
        internal static async Task Ready()
        {
            await Discord.InteractionService.RegisterCommandsGloballyAsync();
            await Discord.DiscordClient.SetGameAsync(type: ActivityType.Watching, name: "Jerma985.");
            Services.GetRequiredService<Logger>().Information("Discord Ready!");
        }

        internal static async Task TickGuild(IInteractionContext context)
        {
            using var database = Services.GetRequiredService<DatabaseContext>();

            if (database.GuildProperties.Find(context.Guild.Id) == null)
            {
            var guildProperty = new Database.Models.Guild { ID = context.Guild.Id, Starboard = new Database.Models.StarboardSettings { } };
                database.GuildProperties.Add(guildProperty);

                await database.SaveChangesAsync();

                guildProperty.Starboard.SetOrCreateEmoji(database.Emojis.AsNoTracking().FirstOrDefault(e => e.NameOrID == ":star:"), database);

                await database.SaveChangesAsync();
                
            }
        }
    }