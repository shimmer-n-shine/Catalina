using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Catalina.Discord
{
    public static class Utils
    {
        public class InformationMessage 
        {
            public string Body;
            public string Title = "Info";
            public Color Color = DiscordColor.Blue;
            public IUser User;

            public static implicit operator Embed(InformationMessage message) => new EmbedBuilder
            {
                Title = message.Title,
                Color = message.Color,
                Description = message.Body,
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = message.User.GetAvatarUrl() ?? message.User.GetDefaultAvatarUrl(),
                    Text = string.Format("Command executed for {0}#{1}", message.User.Username, message.User.Discriminator)
                }
            }.Build();
        }

        public class AcknowledgementMessage
        {
            public string Body;
            public string Title = "Success!";
            public Color Color = DiscordColor.Green;
            public IUser User;

            public static implicit operator Embed(AcknowledgementMessage message) => new EmbedBuilder
            {
                Title = message.Title,
                Color = message.Color,
                Description = message.Body,
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = message.User.GetAvatarUrl() ?? message.User.GetDefaultAvatarUrl(),
                    Text = string.Format("Command executed for {0}#{1}", message.User.Username, message.User.Discriminator)
                }
            }.Build();
        }

        public class WarningMessage
        {
            public string Body;
            public string Title = "Warning:";
            public IUser User;

            public static implicit operator Embed(WarningMessage message) => new EmbedBuilder
            {
                Title = message.Title,
                Color = DiscordColor.Yellow,
                Description = message.Body,
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = message.User.GetAvatarUrl() ?? message.User.GetDefaultAvatarUrl(),
                    Text = string.Format("Command executed for {0}#{1}", message.User.Username, message.User.Discriminator)
                }
            }.Build();
        }
        public class ErrorMessage
        {
            public string Body;
            public string Title = "Uh oh.";
            public IUser User;
            public Exception Exception = null;

            public static implicit operator Embed(ErrorMessage message) => new EmbedBuilder
            {
                Title = message.Title,
                Color = DiscordColor.Red,
                Description = message.Exception is not null ? message.Exception.ToString() : message.Body,
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = message.User.GetAvatarUrl() ?? message.User.GetDefaultAvatarUrl(),
                    Text = string.Format("Command executed for {0}#{1}", message.User.Username, message.User.Discriminator)
                }
            }.Build();
        }

        public class QueryMessage
        {
            public string Body;
            public string Title = "Wait.";
            public IUser User;

            public static implicit operator Embed(QueryMessage message) => new EmbedBuilder
            {
                Title = message.Title,
                Color = DiscordColor.Lilac,
                Description = message.Body,
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = message.User.GetAvatarUrl() ?? message.User.GetDefaultAvatarUrl(),
                    Text = string.Format("Command executed for {0}:{1}", message.User.Username, message.User.Discriminator)
                }
            }.Build();
        }

        public static async Task<IMessage> QueryUser(ICommandContext ctx)
        {
            var message = await Discord.interactivity.NextMessageAsync();
                //GetNextMessageAsync(new TimeSpan(0, 1, 0));

            if (message.IsSuccess)
            {
                return message.Value;
            }
            else if (message.IsTimeout)
            {
                var embed = new ErrorMessage
                {
                    Body = "You took too long to respond.",
                    User = ctx.User
                };
                await ctx.Message.ReplyAsync(embed: embed);
            }
            return null;
        }

        public static void GetImagesFromMessage(ICommandContext ctx)
        {
        }

        public static IEmote GetEmojiFromString(string text)
        {
            var pattern = new Regex("([A-z_]|[0-9]){2,}");
            try
            {
                var result = pattern.Match(text);
                if (result.Success)
                {
                    var match = ':' + result.Value + ':';
                    var emoji = Emote.Parse(match);
                    return emoji;

                }
                else
                {
                    try
                    {
                        var emoji = Emote.Parse(text);
                        return emoji;
                    }
                    catch
                    {
                        try
                        {
                            var emoji = Emoji.Parse(text);
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

        public static async Task<IMessage> GetMessageFromLink(ICommandContext ctx, string link)
        {
            var messageID = GetMessageIDFromLink(link);
            var channelID = GetChannelIDFromLink(link);
            if (messageID.HasValue && channelID.HasValue)
            {
                var channel = await ctx.Guild.GetTextChannelAsync(channelID.Value);
                return await channel.GetMessageAsync(messageID.Value); 
            }
            else
            {
                return null;
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

    }
}