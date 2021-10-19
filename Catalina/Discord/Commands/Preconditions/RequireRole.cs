using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Preconditions
{
    public class RequireRole : PreconditionAttribute
    {
        private readonly AccessLevel _requiredLevel;

        public RequireRole(AccessLevel accessLevel)
        {
            _requiredLevel = accessLevel;
        }
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var access = GetPermission(context);
            if (access >= _requiredLevel) return Task.FromResult(PreconditionResult.FromSuccess());
            else return Task.FromResult(PreconditionResult.FromError("Insufficient Permission"));
        }

        public AccessLevel GetPermission(ICommandContext ctx)
        {
            if (ctx.User.IsBot)
            {
                return AccessLevel.Blocked;
            }

            if (ctx.User is IGuildUser user)
            {
                if (ctx.Guild.OwnerId == user.Id)
                {
                    return AccessLevel.SuperAdministrator;
                }
                if (user.GuildPermissions.Administrator)
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
