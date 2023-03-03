using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Preconditions
{
    public class RequireDev : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
        {
            if (context.User.Id == AppConfig.DeveloperID)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            return Task.FromResult(PreconditionResult.FromError("Insufficient Permission"));
        }
    }

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
                context.Interaction.RespondAsync(embed: new Utils.ErrorMessage (user: context.User) { Exception = new UnauthorizedAccessException()}, ephemeral: true);
                return Task.FromResult(PreconditionResult.FromError("Insufficient Permission"));
            }
        }

        public static AccessLevel GetPermission(IInteractionContext ctx)
        {
            if (ctx.User.IsBot)
            {
                return AccessLevel.Blocked;
            }

            if (ctx.User is IGuildUser user)
            {
                if (ctx.Guild.OwnerId == user.Id || user.GuildPermissions.Has(PermissionConstants.TrueAdministrator))
                {
                    return AccessLevel.SuperAdministrator;
                }
                if (user.GuildPermissions.Has(PermissionConstants.Administrator))
                {
                    return AccessLevel.Administrator;
                }
                if (user.GuildPermissions.Has(PermissionConstants.Moderator))
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
