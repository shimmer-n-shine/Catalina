using Catalina.Extensions;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Common;

public static class Utils
{
    public class CatalinaMessage
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public Color Color { get; set; }

        public IUser User { get; protected set; }

        private protected virtual EmbedBuilder ToEmbedBuilder() => new EmbedBuilder
        {
            Title = Title,
            Color = Color,
            Description = Body,
            Footer = new EmbedFooterBuilder
            {
                IconUrl = User.GetAvatarUrl() ?? User.GetDefaultAvatarUrl(),
                Text = $"Command executed for {User.FullName()}"
            }
        };
    }

    public class InformationMessage : CatalinaMessage
    {
        public InformationMessage(IUser user, string title = "Heads Up!", string body = null, Color? color = null)
        {
            this.Title = title;
            this.Body = body;
            this.Color = color ?? CatalinaColours.Blue;
            this.User = user;
        }
        public InformationMessage(IUser user)
        {
            this.User = user;
            this.Title = "Heads Up!";
            this.Color = CatalinaColours.Blue;
        }

        public static implicit operator EmbedBuilder(InformationMessage message) => message.ToEmbedBuilder();

        public static implicit operator Embed(InformationMessage message) => ((EmbedBuilder)message).Build();
    }

    public class AcknowledgementMessage : CatalinaMessage
    {
        public AcknowledgementMessage(IUser user, string title = "Success!", string body = null, Color? color = null)
        {
            this.Title = title;
            this.Body = body;
            this.Color = color ?? CatalinaColours.Green;
            this.User = user;
        }
        public AcknowledgementMessage(IUser user)
        {
            this.User = user;
            this.Title = "Success!";
            this.Color = CatalinaColours.Green;
        }

        public static implicit operator EmbedBuilder(AcknowledgementMessage message) => message.ToEmbedBuilder();

        public static implicit operator Embed(AcknowledgementMessage message) => ((EmbedBuilder)message).Build();
    }

    public class WarningMessage : CatalinaMessage
    {
        public WarningMessage(IUser user, string title = "Warning!", string body = null)
        {
            this.Title = title;
            this.Body = body;
            this.User = user;
            this.Color = CatalinaColours.Yellow;
        }

        public WarningMessage(IUser user)
        {
            this.Title = "Warning!";
            this.User = user;
            this.Color = CatalinaColours.Yellow;
        }

        public static implicit operator EmbedBuilder(WarningMessage message) => message.ToEmbedBuilder();

        public static implicit operator Embed(WarningMessage message) => ((EmbedBuilder)message).Build();
    }
    public class ErrorMessage : CatalinaMessage
    {
        public Exception Exception { get; set; } = new Exception("Something went wrong.");

        public ErrorMessage(IUser user, Exception exception, string title = "Uh Oh!")
        {
            this.Title = title;
            this.Body = $"{exception.GetType().Name}: {exception.Message}";
            this.User = user;
            this.Exception = exception;
            this.Color = CatalinaColours.Red;
        }

        public ErrorMessage(IUser user)
        {
            this.Title = "Uh Oh!";
            this.Body = "Exception: Something went wrong, but I'm not sure what...";
            this.User = user;
            this.Color = CatalinaColours.Red;
            this.Exception = new Exception("Something went wrong, but I'm not sure what...");
        }

        public static implicit operator EmbedBuilder(ErrorMessage message) => message.ToEmbedBuilder();

        private protected override EmbedBuilder ToEmbedBuilder() => new EmbedBuilder
        {
            Title = Title,
            Color = Color,
            Description = Body,
            Footer = new EmbedFooterBuilder
            {
                IconUrl = User.GetAvatarUrl() ?? User.GetDefaultAvatarUrl(),
                Text = $"Command executed for {User.FullName()}"
            }
        };

        public static implicit operator Embed(ErrorMessage message) => ((EmbedBuilder)message).Build();

    }

    public class QueryMessage : CatalinaMessage
    {
        public QueryMessage(IUser user, string title = "One sec!!", string body = null, Color? color = null)
        {
            this.Title = title;
            this.Body = body;
            this.Color = color ?? CatalinaColours.Lilac;
            this.User = user;
        }
        public QueryMessage(IUser user)
        {
            this.Title = "One sec!!";
            this.Body = null;
            this.Color = CatalinaColours.Lilac;
            this.User = user;
        }

        public static implicit operator EmbedBuilder(QueryMessage message) => message.ToEmbedBuilder();

        public static implicit operator Embed(QueryMessage message) => ((EmbedBuilder)message).Build();
    }

    public static async Task<IMessage> QueryUser(ICommandContext ctx)
    {
        var message = await Discord.Discord.InteractiveService.NextMessageAsync();

        if (message.IsSuccess)
        {
            return message.Value;
        }
        else if (message.IsTimeout)
        {
            var embed = new ErrorMessage(user: ctx.User, exception: new Exceptions.InvalidArgumentException("You took too long to respond."));
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
        var botRoles = (await context.Guild.GetCurrentUserAsync()).RoleIds.Select(r => context.Guild.GetRole(r)).Where(r => r.Permissions.ManageRoles || (r.Permissions.Administrator && r.Position < highestUserRole.Position));
        var highestBotRole = botRoles.OrderByDescending(r => r.Position).First();
        var results = context.Guild.Roles.Where(r => r.Position < highestBotRole.Position);

        return results.Contains(role);
    }

}