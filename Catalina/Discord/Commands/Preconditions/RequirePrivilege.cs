using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Preconditions
{
    public class RequirePrivilege : PreconditionAttribute
    {
        private readonly AccessLevel _requiredLevel;

        public RequirePrivilege(AccessLevel accessLevel)
        {
            _requiredLevel = accessLevel;
        }
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
        {
            var access = GetPermission(context);
            if (access >= _requiredLevel) return Task.FromResult(PreconditionResult.FromSuccess());
            else
            {
                context.Interaction.RespondAsync(embed: new Utils.ErrorMessage { User = context.User , Exception = new UnauthorizedAccessException()}, ephemeral: true);
                return Task.FromResult(PreconditionResult.FromError("Insufficient Permission"));
            }
        }

        public AccessLevel GetPermission(IInteractionContext ctx)
        {
            if (ctx.User.IsBot)
            {
                return AccessLevel.Blocked;
            }

            if (ctx.User is IGuildUser user)
            {
                if (ctx.Guild.OwnerId == user.Id || user.GuildPermissions.Administrator)
                {
                    return AccessLevel.SuperAdministrator;
                }
                if (user.GuildPermissions.ManageChannels && user.GuildPermissions.ManageGuild && user.GuildPermissions.BanMembers)
                {
                    return AccessLevel.Administrator;
                }
                if (user.GuildPermissions.KickMembers && user.GuildPermissions.ManageMessages)
                {
                    return AccessLevel.Moderator;
                }
            }
            return AccessLevel.User;
        }
    }
    
    public enum AccessLevel
    {
        User = 0,
        Moderator = 1,
        Administrator = 2,
        SuperAdministrator = 3,
        Blocked = -1
    }
}
