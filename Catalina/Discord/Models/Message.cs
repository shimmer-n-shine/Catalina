using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Threading.Tasks;

namespace Catalina.Discord.Models
{
    public static class Message
    {
        public class InformationMessage
        {
            public string Body;
            public DiscordColor Color = DiscordColor.Aquamarine;
            public DiscordUser User;

            public static implicit operator DiscordEmbed(InformationMessage message) => new DiscordEmbedBuilder
            {
                Title = "Hey!",
                Color = message.Color,
                Description = message.Body,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = message.User.AvatarUrl,
                    Text = string.Format("Command executed for {0}:{1}", message.User.Username, message.User.Discriminator)
                }
            };
        }

        public class AcknowledgementMessage
        {
            public string Body;
            public DiscordUser User;

            public static implicit operator DiscordEmbed(AcknowledgementMessage message) => new DiscordEmbedBuilder
            {
                Title = "Yay!",
                Color = DiscordColor.SapGreen,
                Description = message.Body,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = message.User.AvatarUrl,
                    Text = string.Format("Command executed for {0}:{1}", message.User.Username, message.User.Discriminator)
                }
            };
        }

        public class WarningMessage
        {
            public string Body;
            public DiscordUser User;

            public static implicit operator DiscordEmbed(WarningMessage message) => new DiscordEmbedBuilder
            {
                Title = "Wait!",
                Color = DiscordColor.Orange,
                Description = message.Body,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = message.User.AvatarUrl,
                    Text = string.Format("Command executed for {0}:{1}", message.User.Username, message.User.Discriminator)
                }
            };
        }
        public class ErrorMessage
        {
            public string Body;
            public DiscordUser User;
            public Exception Exception = null;

            public static implicit operator DiscordEmbed(ErrorMessage message) => new DiscordEmbedBuilder
            {
                Title = "Oh No!",
                Color = DiscordColor.Red,
                Description = message.Exception is not null ? message.Exception.ToString() : message.Body,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = message.User.AvatarUrl,
                    Text = string.Format("Command executed for {0}:{1}", message.User.Username, message.User.Discriminator)
                }
            };
        }

        public class QueryMessage
        {
            public string Body;
            public DiscordUser User;

            public static implicit operator DiscordEmbed(QueryMessage message) => new DiscordEmbedBuilder
            {
                Title = "Question:",
                Color = DiscordColor.Lilac,
                Description = message.Body,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = message.User.AvatarUrl,
                    Text = string.Format("Command executed for {0}:{1}", message.User.Username, message.User.Discriminator)
                }
            };
        }

        public static async Task QueryUser(CommandContext ctx)
        {
            var message = await ctx.Message.GetNextMessageAsync(new TimeSpan(0, 1, 0));

            if (!message.TimedOut)
            {
                
            }
            else
            {
                var embed = new ErrorMessage
                {
                    Body = "You took too long to respond.",
                    User = ctx.User
                };
                await ctx.RespondAsync(embed);
            }
        }
    }
}