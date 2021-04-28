using Catalina.Configuration;
using Catalina.Discord;
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
using System.Text;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Catalina.Discord
{
    public class Discord
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
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
                Intents = DiscordIntents.GuildMembers | DiscordIntents.GuildIntegrations | DiscordIntents.GuildMessages | DiscordIntents.Guilds | DiscordIntents.GuildPresences,
                Token = ConfigValues.DiscordToken,
                TokenType = TokenType.Bot,
                AlwaysCacheMembers = true,
                LoggerFactory = logFactory
            });
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = ConfigValues.Prefixes,
                CaseSensitive = false,
            });

            commands.RegisterCommands<CoreModule>();

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            //discord.GuildMemberRemoved += Events.Discord_GuildMemberRemoved;
            discord.MessageCreated += Events.Discord_MessageCreated;
            discord.MessageDeleted += Events.Discord_MessageDeleted;
            //discord.Ready += Events.Discord_Ready;
            discord.MessageReactionAdded += Events.Discord_ReactionAdded;
            discord.MessageReactionRemoved += Events.Discord_ReactionRemoved;

            await discord.ConnectAsync(ConfigValues.DiscordActivity);
            await UpdateChannels();
        }
        public static async Task UpdateChannels()
        {
            commandChannels = new List<DiscordChannel>();
            foreach (var channel in ConfigValues.CommandChannels)
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
        public static DiscordEmbed CreateFancyMessage(
            string title = null,
            string description = null,
            string url = null,
            EmbedAuthor author = null,
            DiscordColor? color = null,
            List<DiscordEmbedField> fields = null,
            EmbedFooter footer = null,
            string imageURL = null,
            EmbedThumbnail thumbnail = null, 
            DateTime? timestamp = null
            )
        {
            //var embedBuilder = new DiscordEmbedBuilder()
            //{
            //    Color = color,
            //    Description = description,
            //    Title = title
            //};
            var embedBuilder = new DiscordEmbedBuilder();

            if (title != null) embedBuilder.Title = title;
            if (description != null) embedBuilder.Description = description;
            if (url != null) embedBuilder.Url = url;
            if (author != null) embedBuilder.Author = author;
            if (color != null) embedBuilder.Color = (DiscordColor) color;
            if (fields != null) fields.ForEach(field => embedBuilder.AddField(field.Name, field.Value, field.Inline));
            if (footer != null) embedBuilder.Footer = footer;
            if (imageURL != null) embedBuilder.ImageUrl = imageURL;
            if (thumbnail != null) embedBuilder.Thumbnail = thumbnail;
            if (timestamp != null) embedBuilder.Timestamp = timestamp;


            return embedBuilder.Build();

        }
        
    }
    public struct Response
    {
        public Response(string trigger, string response, string description = null, List<DiscordChannel> disallowedChannels = null)
        {
            this.trigger = trigger; this.response = response; this.description = description; this.disallowedChannels = disallowedChannels;
        }
        public string trigger; public string response; public string description; public List<DiscordChannel> disallowedChannels;
    }
    public struct Reaction
    {
        public Reaction(ulong messageID, DiscordEmoji discordEmoji, DiscordRole role)
        {
            this.messageID = messageID; this.emoji = discordEmoji; this.role = role;
        }
        public ulong messageID; public DiscordEmoji emoji; public DiscordRole role;
    }
}