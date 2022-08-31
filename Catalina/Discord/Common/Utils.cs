using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord
{
    public static class Utils
    {
        public class InformationMessage 
        {
            public string Body = null;
            public string Title = "Heads Up!";
            public Color Color = CatalinaColours.Blue;
            public readonly IUser User;

            public InformationMessage(IUser user, string title = "Heads Up!", string body = null, Color? color = null)
            {
                this.Title = title;
                this.Body = body;
                this.Color = color??CatalinaColours.Blue;
                this.User = user;
            }
            public InformationMessage(IUser user)
            {
                this.User = user;
            }

            public static implicit operator EmbedBuilder(InformationMessage message) => new EmbedBuilder
            {
                Title = message.Title,
                Color = message.Color,
                Description = message.Body,
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = message.User.GetAvatarUrl() ?? message.User.GetDefaultAvatarUrl(),
                    Text = string.Format("Command executed for {0}#{1}", message.User.Username, message.User.Discriminator)
                }
            };

            public static implicit operator Embed(InformationMessage message) => ((EmbedBuilder) message).Build();
        }

        public class AcknowledgementMessage
        {
            public string Body = null;
            public string Title = "Success!";
            public Color Color = CatalinaColours.Green;
            public readonly IUser User;

            public AcknowledgementMessage(IUser user, string title = "Success!", string body = null, Color? color = null)
            {
                this.Title = title;
                this.Body = body;
                this.Color = color ?? CatalinaColours.Blue;
                this.User = user;
            }
            public AcknowledgementMessage(IUser user)
            {
                this.User = user;
            }

            public static implicit operator EmbedBuilder(AcknowledgementMessage message) => new EmbedBuilder
            {
                Title = message.Title,
                Color = message.Color,
                Description = message.Body,
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = message.User.GetAvatarUrl() ?? message.User.GetDefaultAvatarUrl(),
                    Text = string.Format("Command executed for {0}#{1}", message.User.Username, message.User.Discriminator)
                }
            };

            public static implicit operator Embed(AcknowledgementMessage message) => ((EmbedBuilder)message).Build();
        }

        public class WarningMessage
        {
            public string Body;
            public string Title = "Warning";
            public readonly IUser User;

            public WarningMessage(IUser user, string title = "Warning!", string body = null)
            {
                this.Title = title;
                this.Body = body;
                this.User = user;
            }

            public WarningMessage(IUser user)
            {
                this.User = user;
            }

            public static implicit operator EmbedBuilder(WarningMessage message) => new EmbedBuilder
            {
                Title = message.Title,
                Color = CatalinaColours.Yellow,
                Description = message.Body,
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = message.User.GetAvatarUrl() ?? message.User.GetDefaultAvatarUrl(),
                    Text = string.Format("Command executed for {0}#{1}", message.User.Username, message.User.Discriminator)
                }
            };

            public static implicit operator Embed(WarningMessage message) => ((EmbedBuilder)message).Build();
        }
        public class ErrorMessage
        {
            public string Body;
            public string Title;
            public readonly IUser User;
            public Exception Exception = new Exception("Something went wrong.");

            public ErrorMessage(IUser user, string title = "Uh Oh!", string body = null, Exception exception = null)
            {
                this.Title = title;
                this.Body = body;
                this.User = user;
                this.Exception = exception??new Exception("Something went wrong.");
            }

            public ErrorMessage(IUser user)
            {
                this.User = user;
            }

            public static implicit operator EmbedBuilder(ErrorMessage message) => new EmbedBuilder
            {
                Title = message.Title,
                Color = CatalinaColours.Red,
                Description = message.Exception is not null ? $"{message.Exception.GetType().Name}: {message.Exception.Message}" : "$Unknown exception",
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = message.User.GetAvatarUrl() ?? message.User.GetDefaultAvatarUrl(),
                    Text = string.Format("Command executed for {0}#{1}", message.User.Username, message.User.Discriminator)
                }
            };

            public static implicit operator Embed(ErrorMessage message) => ((EmbedBuilder)message).Build();
        }

        public class QueryMessage
        {
            public readonly string Body;
            public readonly string Title = "Wait.";
            public readonly IUser User;

            public static implicit operator EmbedBuilder(QueryMessage message) => new EmbedBuilder
            {
                Title = message.Title,
                Color = CatalinaColours.Lilac,
                Description = message.Body,
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = message.User.GetAvatarUrl() ?? message.User.GetDefaultAvatarUrl(),
                    Text = string.Format("Command executed for {0}:{1}", message.User.Username, message.User.Discriminator)
                }
            };

            public static implicit operator Embed(QueryMessage message) => ((EmbedBuilder)message).Build();
        }

        public static async Task<IMessage> QueryUser(ICommandContext ctx)
        {
            var message = await Discord.InteractiveService.NextMessageAsync();

            if (message.IsSuccess)
            {
                return message.Value;
            }
            else if (message.IsTimeout)
            {
                var embed = new ErrorMessage(user: ctx.User)
                {
                    Body = "You took too long to respond."
                };
                await ctx.Message.ReplyAsync(embed: embed);
            }
            return null;
        }


        public static async Task<IMessage> GetMessageFromLink(IInteractionContext ctx, string link)
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

        public static async Task<bool> VerifyRoleForUser(IInteractionContext context, ulong roleID)
        {
            var role = context.Guild.GetRole(roleID);

            var userRoles = (context.User as IGuildUser).RoleIds.Select(r => context.Guild.GetRole(r)).Where(r => r.Permissions.ManageRoles || r.Permissions.Administrator);
            if (context.Guild.OwnerId == context.User.Id) userRoles = context.Guild.Roles;
            var highestUserRole = userRoles.OrderByDescending(r => r.Position).First();
            var botRoles = (await context.Guild.GetCurrentUserAsync()).RoleIds.Select(r => context.Guild.GetRole(r)).Where(r => r.Permissions.ManageRoles || r.Permissions.Administrator && r.Position < highestUserRole.Position);
            var highestBotRole = botRoles.OrderByDescending(r => r.Position).First();
            var results = context.Guild.Roles.Where(r => r.Position < highestBotRole.Position);

            return results.Contains(role);
        }

    }
}