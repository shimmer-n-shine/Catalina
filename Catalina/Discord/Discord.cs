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
using System.Text.RegularExpressions;
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
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { Environment.GetEnvironmentVariable(AppProperties.BotPrefix) },
                CaseSensitive = false,
            });

            commands.RegisterCommands<CoreModule>();

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            discord.GuildMemberAdded += Events.Discord_GuildMemberAdded;
            discord.MessageDeleted += Events.Discord_MessageDeleted;
            discord.MessageReactionAdded += Events.Discord_ReactionAdded;
            discord.MessageReactionRemoved += Events.Discord_ReactionRemoved;
            discord.MessageReactionsCleared += Events.Discord_ReactionsCleared;

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
            if (guildProperties.All( g => string.IsNullOrEmpty(g.CommandChannelsSerialised)))
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

        public static async Task<DiscordEmoji> GetEmojiFromMessage(CommandContext ctx)
        {
            DiscordEmbed discordEmbed;
            discordEmbed = new DiscordEmbedBuilder()
            {
                Title = "Reactions!",
                Description = "Please send the emoji to watch for:",
                Color = DiscordColor.CornflowerBlue
            }.Build();
                //CreateFancyMessage(title: "Adding a new reaction!", description: "Please send the emoji to watch for", color: DiscordColor.CornflowerBlue);
            await ctx.RespondAsync(discordEmbed);
            var body = await ctx.Message.GetNextMessageAsync();


            if (!body.TimedOut)
            {
                var emoji = GetEmojiFromString(body.Result.Content);
                if (emoji != null)
                {
                    return emoji;
                }
                else
                {
                    discordEmbed = new DiscordEmbedBuilder()
                    {
                        Title = "Sorry!",
                        Description = "The emoji you provided was invalid!",
                        Color = DiscordColor.Red
                    }.Build();
                        //CreateFancyMessage(title: "Sorry!", description: "The emoji you provided was invalid!", color: DiscordColor.Red);
                    await SendFancyMessage(ctx.Channel, discordEmbed);
                    return null;
                }
            }
            else
            {
                discordEmbed = discordEmbed = new DiscordEmbedBuilder()
                {
                    Title = "Sorry!",
                    Description = "You took too long to respond!",
                    Color = DiscordColor.Red
                }.Build();
                await SendFancyMessage(ctx.Channel, discordEmbed);
                return null;
            }
        }

        public static async Task<DiscordRole> GetRoleFromMessage(CommandContext ctx)
        {
            DiscordEmbed discordEmbed;
            discordEmbed = new DiscordEmbedBuilder
            {
                Title = "Deriving role!",
                Description = "Please mention a role.",
                Color = DiscordColor.CornflowerBlue
            }.Build();
            await ctx.RespondAsync(discordEmbed);
            var body = await ctx.Message.GetNextMessageAsync(new TimeSpan(0, 1, 0));

            if (!body.TimedOut)
            {
                var role = GetRoleFromList(body.Result.MentionedRoles.ToList(), ctx);
                if (role != null)
                {
                    return role;
                }
                else
                {
                    discordEmbed = new DiscordEmbedBuilder()
                    {
                        Title = "Sorry!",
                        Description = "The role you mentioned was invalid.",
                        Color = DiscordColor.Red
                    }.Build();
                    await SendFancyMessage(ctx.Channel, discordEmbed);
                    return null;
                }
            }
            else
            {
                discordEmbed = new DiscordEmbedBuilder()
                {
                    Title = "Sorry!",
                    Description = "You took too long to respond!",
                    Color = DiscordColor.Red
                }.Build();
                await SendFancyMessage(ctx.Channel, discordEmbed);
                return null;
            }
        }
        public static DiscordEmoji GetEmojiFromString(string text)
        {
            var pattern = new Regex("([A-z_]|[0-9]){2,}");
            try
            {
                var result = pattern.Match(text);
                if (result.Success)
                {
                    var match = ':' + result.Value + ':';
                    DiscordEmoji emoji = DiscordEmoji.FromName(discord, match, true);
                    return emoji;
                    
                }
                else
                {
                    try
                    {
                        DiscordEmoji emoji = DiscordEmoji.FromName(discord, text);
                        return emoji;
                    }
                    catch
                    {
                        try
                        {
                            DiscordEmoji emoji = DiscordEmoji.FromUnicode(discord, text);
                            return emoji;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }
            catch
            {

                return null;
            }
        }
        public static DiscordRole GetRoleFromList(List<DiscordRole> roles, CommandContext ctx)
        {
            try
            {
                ulong id = Convert.ToUInt64(roles.First().Id);
                return ctx.Guild.GetRole(id);

            }
            catch
            {
                return null;
            }
        }

        public static async Task<DiscordMessage> GetMessageFromLinkAsync(CommandContext ctx, string messageLink = null)
        {
            if (messageLink != null)
            {
                var messageID = GetMessageIDFromLink(messageLink);
                var channelID = GetChannelIDFromLink(messageLink);
                if (messageID != null && channelID != null)
                {
                    var message = await ctx.Guild.GetChannel((ulong)channelID).GetMessageAsync((ulong)messageID); //ctx.Guild.GetChannelAsync((ulong)channelID).GetMessageAsync((ulong)messageID);
                    return message;
                }
                else
                {
                    return null;
                }
            }

            else
            {
                DiscordEmbed discordEmbed = new DiscordEmbedBuilder()
                {
                    Title = "Deriving message!",
                    Description = "Please enter a message link.",
                    Color = DiscordColor.Red
                }.Build();
                await ctx.RespondAsync(discordEmbed);
                var body = await ctx.Message.GetNextMessageAsync(new TimeSpan(0, 1, 0));

                if (!body.TimedOut)
                {
                    var messageID = GetMessageIDFromLink(body.Result.Content);
                    var channelID = GetChannelIDFromLink(body.Result.Content);
                    if (messageID != null && channelID != null)
                    {
                        return await ctx.Guild.GetChannel((ulong)channelID).GetMessageAsync((ulong)messageID);
                    }
                    else
                    {
                        discordEmbed = new DiscordEmbedBuilder()
                        {
                            Title = "Sorry!",
                            Description = "The message link you provided was invalid!!",
                            Color = DiscordColor.Red
                        }.Build();
                        await SendFancyMessage(ctx.Channel, discordEmbed);
                        return null;
                    }
                }
                else
                {
                    discordEmbed = new DiscordEmbedBuilder()
                    {
                        Title = "Sorry!",
                        Description = "You took too long to respond!",
                        Color = DiscordColor.Red
                    }.Build();
                    await SendFancyMessage(ctx.Channel, discordEmbed);
                    return null;
                }
            }

        }

        public static ulong? GetMessageIDFromLink(string message)
        {
            try
            {
                var splitMessage = message.Split('/');
                return Convert.ToUInt64(splitMessage.Last());

            }
            catch
            { return null; }

        }

        public static ulong? GetChannelIDFromLink(string message)
        {
            try
            {
                var splitMessage = message.Split('/');
                return Convert.ToUInt64(splitMessage[^2]);
            }
            catch { return null; }
        }

        public static async Task<DiscordMessage> SendMessage(string text, DiscordChannel channel)
        {
            return await discord.SendMessageAsync(channel, text);
        }

        public static async Task UpdateMessage(string text, DiscordMessage message)
        {
            await message.ModifyAsync(text);
        }
        public static async Task<DiscordMessage> SendFancyMessage(DiscordChannel channel, DiscordEmbed embed)
        {
            return await discord.SendMessageAsync(channel, embed);
        }
    }
    //public struct Response
    //{
    //    public Response(string trigger, string response, string description = null, List<DiscordChannel> disallowedChannels = null)
    //    {
    //        this.trigger = trigger; this.response = response; this.description = description; this.disallowedChannels = disallowedChannels;
    //    }
    //    public string trigger; public string response; public string description; public List<DiscordChannel> disallowedChannels;
    //}
    //public struct Reaction
    //{
    //    public Reaction(ulong messageID, string emojiName, ulong roleID, ulong ChannelID)
    //    {
    //        this.messageID = messageID; this.emojiName = emojiName; this.roleID = roleID; this.channelID = ChannelID;
    //    }
    //    public ulong messageID; public string emojiName; public ulong roleID; public ulong channelID;
    //}
    //public struct Field
    //{

    //    public Field(string name, string value, bool inline = false)
    //    {
    //        this.name = name; this.value = value; this.inline = inline;
    //    }
    //    public string name; public string value; public bool inline;
    //}
}