using Catalina.Database.Models;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalina.Database;
using Microsoft.EntityFrameworkCore;

namespace Catalina.Discord;
public class Starboard
{
    public static async Task ProcessVote(GuildProperty guildProperty, IMessage dMessage, IUser user)
    {
        using var database = new DatabaseContextFactory().CreateDbContext();
        guildProperty = database.GuildProperties.Include(g => g.StarboardMessages).ThenInclude(g => g.UserVotes).First(g => g == guildProperty);

        //if reacting to a known starboard candidate
        if (guildProperty.StarboardMessages.Any(m => m.ChannelID == dMessage.Channel.Id && m.MessageID == dMessage.Id))
        {
            var message = guildProperty.StarboardMessages.First(m => m.ChannelID == dMessage.Channel.Id && m.MessageID == dMessage.Id);
            if (!message.UserVotes.Any(v => v.UserId == user.Id))
            {
                message.UserVotes.Add(new StarboardVote { UserId = user.Id });
                await database.SaveChangesAsync();
                if (message.UserVotes.Count >= guildProperty.StarboardThreshhold)
                {
                    if (message.StarboardMessageID is null)
                    {
                        message.StarboardMessageID = (await PostStarboardMessage(guildProperty, dMessage, (uint)message.UserVotes.Count)).Id;
                        await database.SaveChangesAsync();
                    }
                    else
                    {
                        await UpdateStarboardMessage(guildProperty, message, (uint)message.UserVotes.Count);
                    }
                }
            }
        }
        //if reacting to a message in the starboard channel
        else if (guildProperty.StarboardMessages.Any(m => guildProperty.StarboardChannelID == dMessage.Channel.Id && m.StarboardMessageID == dMessage.Id))
        {
            var message = guildProperty.StarboardMessages.First(m => guildProperty.StarboardChannelID == dMessage.Channel.Id && m.StarboardMessageID == dMessage.Id);
            if (!message.UserVotes.Any(v => v.UserId == user.Id))
            {
                message.UserVotes.Add(new StarboardVote { UserId = user.Id });
                await database.SaveChangesAsync();
                if (message.UserVotes.Count >= guildProperty.StarboardThreshhold)
                {
                    await UpdateStarboardMessage(guildProperty, message, (uint)message.UserVotes.Count);
                }
            }
        }
        //if not reacting to a known starboard candidate
        else
        {
            var message = new Database.Models.StarboardMessage { ChannelID = dMessage.Channel.Id, MessageID = dMessage.Id, UserVotes = new List<StarboardVote>() { new StarboardVote { UserId = user.Id } } };
            guildProperty.StarboardMessages.Add(message);
            if (message.UserVotes.Count >= guildProperty.StarboardThreshhold)
            {
                if (message.StarboardMessageID is null)
                {
                    message.StarboardMessageID = (await PostStarboardMessage(guildProperty, dMessage, (uint)message.UserVotes.Count)).Id;
                    await database.SaveChangesAsync();
                }
                else
                {
                    await UpdateStarboardMessage(guildProperty, message, (uint)message.UserVotes.Count);
                }
            }
            database.SaveChanges();
        }
    }

    public static async Task<IMessage> PostStarboardMessage(GuildProperty guildProperty, IMessage message, uint votes)
    {
        using var database = new DatabaseContextFactory().CreateDbContext();
        var guild = (message.Channel as IGuildChannel).Guild;
        guildProperty = database.GuildProperties.Include(g => g.StarboardEmoji).First(g => g == guildProperty);
        IGuildChannel sbChannel;

        try
        {
            sbChannel = await guild.GetChannelAsync(guildProperty.StarboardChannelID.Value);
        }
        catch
        {
            throw new ArgumentException($"could not get channel for {guild.Name} ({guild.Id})");
        }
        if (sbChannel is null) return null;
        var finalMessage = await (sbChannel as IMessageChannel).SendMessageAsync(embed: new StarboardMessage { Message = message, User = message.Author, Votes = votes, GuildProperty = database.GuildProperties.Include(g => g.StarboardEmoji).First(g => g == guildProperty) });
        await finalMessage.AddReactionAsync(await Database.Models.Emoji.ToEmoteAsync(guildProperty.StarboardEmoji, guild));
        return finalMessage;
    }

    public static async Task<IMessage> UpdateStarboardMessage(GuildProperty guildProperty, Database.Models.StarboardMessage message, uint votes)
    {
        using var database = new DatabaseContextFactory().CreateDbContext();
        guildProperty = database.GuildProperties.Include(g => g.StarboardEmoji).First(g => g == guildProperty);
        IGuildChannel sbChannel;
        IGuildChannel ogChannel;
        IMessage ogMessage;

        try
        {
            sbChannel = Discord.discord.GetGuild(guildProperty.ID).GetChannel(guildProperty.StarboardChannelID.Value);
            ogChannel = Discord.discord.GetGuild(guildProperty.ID).GetChannel(message.ChannelID);
            ogMessage = await (ogChannel as ITextChannel).GetMessageAsync(message.MessageID);
        }
        catch
        {
            throw new ArgumentException($"could not get message for updating starboard message");
        }
        if (sbChannel is null || ogChannel is null) return null;

        var sbMessageEmbed = (await (sbChannel as ITextChannel).GetMessageAsync(message.StarboardMessageID.Value)).Embeds.First().ToEmbedBuilder().WithTitle(
            $"{votes} {Database.Models.Emoji.ToEmoteAsync(guildProperty.StarboardEmoji, ogChannel.Guild).Result} | <t:{ogMessage.Timestamp.ToUnixTimeSeconds()}>")
            .Build();
        return await (sbChannel as ITextChannel).ModifyMessageAsync(message.StarboardMessageID.Value, msg => msg.Embeds = new[] { sbMessageEmbed });
    }


    public class StarboardMessage
    {

        public IUser User;
        public IMessage Message;
        public GuildProperty GuildProperty;
        public uint Votes;

        public static implicit operator EmbedBuilder(StarboardMessage message) => new EmbedBuilder
        {

            Fields = new List<EmbedFieldBuilder>() { 
                new EmbedFieldBuilder { IsInline = true, Name = "Sent by:", Value = $"<@{message.User.Id}>"}, 
                new EmbedFieldBuilder { IsInline = true, Name = "In:", Value = $"<#{message.Message.Channel.Id}>" },
                new EmbedFieldBuilder { IsInline = false, Name = "Original Message", Value = $"[Jump to Message]({(message.Message as IMessage).GetJumpUrl()})" }
            },
            Color = CatalinaColours.Gold,
            Title = $"{message.Votes} {(Database.Models.Emoji.ToEmoteAsync(message.GuildProperty.StarboardEmoji, (message.Message.Channel as IGuildChannel).Guild)).Result} | <t:{message.Message.Timestamp.ToUnixTimeSeconds()}>",
            Footer = new EmbedFooterBuilder
            {
                IconUrl = message.User.GetAvatarUrl() ?? message.User.GetDefaultAvatarUrl(),
                Text = string.Format($"{message.User.Username}#{message.User.Discriminator}")
            },
            Description = message.Message.Content,
            ImageUrl = message.Message.Attachments.Any(a => a.ContentType.ToLower().Contains("image")) ? message.Message.Attachments.First(a => a.ContentType.ToLower().Contains("image")).Url : null
        };

        public static implicit operator Embed(StarboardMessage message) => ((EmbedBuilder)message).Build();
    }
}
