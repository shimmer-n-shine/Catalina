using Catalina.Database.Models;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalina.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Catalina.Common;

namespace Catalina.Discord;
public static class Starboard
{
    public static ServiceProvider Services;
    public static async Task ProcessVote(Guild guild, IMessage dMessage, IUser user)
    {
        using var database = Services.GetRequiredService<DatabaseContext>();
        //modify incoming guildProperty to include starboard messages, better to do it here than upstream :)

        guild = database.Guilds.Include(g => g.Starboard).First(g => g == guild);
        
        //microoptimisation; won't need to evaluate and find these twice this way
        var knownMessage = guild.Starboard.Messages.FirstOrDefault(m => m.ChannelID == dMessage.Channel.Id && m.MessageID == dMessage.Id);
        var starboardMessage = guild.Starboard.Messages.FirstOrDefault(m => guild.Starboard.ChannelID == dMessage.Channel.Id && m.StarboardMessageID == dMessage.Id);

        //if reacting to a known starboard candidate
        if (knownMessage is not null)
        {
            //if user hasn't voted already
            if (!knownMessage.Votes.Any(v => v.UserId == user.Id))
            {
                knownMessage.Votes.Add(new Vote { UserId = user.Id });
                await database.SaveChangesAsync();
                if (knownMessage.Votes.Count >= guild.Starboard.Threshhold)
                {
                    //if no starboard message has been posted yet
                    if (knownMessage.StarboardMessageID is null)
                    {
                        knownMessage.StarboardMessageID = (await PostStarboardMessage(guild, dMessage, (uint)knownMessage.Votes.Count)).Id;
                        await database.SaveChangesAsync();
                    }
                    else
                    {
                        await UpdateStarboardMessage(guild, knownMessage, (uint)knownMessage.Votes.Count);
                    }
                }
            }
        }
        //if reacting to a message in the starboard channel
        else if (starboardMessage is not null)
        {
            //if user hasn't voted already
            if (!starboardMessage.Votes.Any(v => v.UserId == user.Id))
            {
                starboardMessage.Votes.Add(new Vote { UserId = user.Id });
                await database.SaveChangesAsync();
                if (starboardMessage.Votes.Count >= guild.Starboard.Threshhold)
                {
                    await UpdateStarboardMessage(guild, starboardMessage, (uint)starboardMessage.Votes.Count);
                }
            }
        }
        //if reacting to a new starboard candidate
        else
        {
            var message = new Database.Models.Message { ChannelID = dMessage.Channel.Id, MessageID = dMessage.Id, Votes = new List<Vote>() { new Vote { UserId = user.Id } } };
            guild.Starboard.Messages.Add(message);

            if (message.Votes.Count >= guild.Starboard.Threshhold)
            {
                //if no starboard message has been posted yet
                if (message.StarboardMessageID is null)
                {
                    message.StarboardMessageID = (await PostStarboardMessage(guild, dMessage, (uint)message.Votes.Count)).Id;
                    await database.SaveChangesAsync();
                }
                else
                {
                    await UpdateStarboardMessage(guild, message, (uint)message.Votes.Count);
                }
            }
            await database.SaveChangesAsync();
        }
    }

    public static async Task<IMessage> PostStarboardMessage(Guild guildProperty, IMessage message, uint votes)
    {
        using var database = Services.GetRequiredService<DatabaseContext>();
        var guild = (message.Channel as IGuildChannel).Guild;
        //guildProperty = database.GuildProperties.Include(g => g.StarboardEmoji).First(g => g == guildProperty);
        IGuildChannel sbChannel;

        try
        {
            sbChannel = await guild.GetChannelAsync(guildProperty.Starboard.ChannelID.Value);
        }
        catch
        {
            throw new ArgumentException($"could not get channel for {guild.Name} ({guild.Id})");
        }
        //no channel found, terminate
        if (sbChannel is null) return null;

        var finalMessage = await (sbChannel as IMessageChannel).SendMessageAsync(embed: new StarboardMessage {
            //Message = message, User = message.Author, Votes = votes, GuildProperty = database.GuildProperties.Include(g => g.StarboardEmoji).First(g => g == guildProperty) 
            Message = message, User = message.Author, Votes = votes, Guild = database.Guilds.First(g => g == guildProperty) 
        });
        await finalMessage.AddReactionAsync(await Database.Models.Emoji.ToEmoteAsync(guildProperty.Starboard.Emoji, guild));
        return finalMessage;
    }

    public static async Task<IMessage> UpdateStarboardMessage(Guild guildProperty, Database.Models.Message message, uint votes)
    {
        using var database = Services.GetRequiredService<DatabaseContext>();
        //guildProperty = database.GuildProperties.Include(g => g.StarboardEmoji).First(g => g == guildProperty);
        IGuildChannel sbChannel;
        IGuildChannel ogChannel;
        IMessage ogMessage;

        try
        {
            sbChannel = Discord.DiscordClient.GetGuild(guildProperty.ID).GetChannel(guildProperty.Starboard.ChannelID.Value);
            ogChannel = Discord.DiscordClient.GetGuild(guildProperty.ID).GetChannel(message.ChannelID);
            ogMessage = await (ogChannel as ITextChannel).GetMessageAsync(message.MessageID);
        }
        catch
        {
            throw new ArgumentException($"could not get message for updating starboard message");
        }
        if (sbChannel is null || ogChannel is null) return null;

        var sbMessageEmbed = (await (sbChannel as ITextChannel).GetMessageAsync(message.StarboardMessageID.Value)).Embeds.First().ToEmbedBuilder().WithTitle(
            $"{votes} {await Database.Models.Emoji.ToEmoteAsync(guildProperty.Starboard.Emoji, ogChannel.Guild)} | <t:{ogMessage.Timestamp.ToUnixTimeSeconds()}>")
            .Build();
        return await (sbChannel as ITextChannel).ModifyMessageAsync(message.StarboardMessageID.Value, msg => msg.Embeds = new[] { sbMessageEmbed });
    }


    public class StarboardMessage
    {

        public IUser User;
        public IMessage Message;
        public Guild Guild;
        public uint Votes;

        public static implicit operator EmbedBuilder(StarboardMessage message) => new EmbedBuilder
        {

            Fields = new List<EmbedFieldBuilder>() { 
                new EmbedFieldBuilder { IsInline = true, Name = "Sent by:", Value = $"<@{message.User.Id}>"}, 
                new EmbedFieldBuilder { IsInline = true, Name = "In:", Value = $"<#{message.Message.Channel.Id}>" },
                new EmbedFieldBuilder { IsInline = false, Name = "Original Message", Value = $"[Jump to Message]({(message.Message as IMessage).GetJumpUrl()})" }
            },
            Color = CatalinaColours.Gold,
            Title = $"{message.Votes} {Database.Models.Emoji.ToEmoteAsync(message.Guild.Starboard.Emoji, (message.Message.Channel as IGuildChannel).Guild).Result} | <t:{message.Message.Timestamp.ToUnixTimeSeconds()}>",
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
