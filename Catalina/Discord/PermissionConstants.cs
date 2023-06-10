using Discord;

namespace Catalina.Discord;
public static class PermissionConstants
{
    public const GuildPermission Moderator = GuildPermission.KickMembers | GuildPermission.ManageMessages;
    public const GuildPermission Administrator = GuildPermission.ManageChannels | GuildPermission.ManageGuild | GuildPermission.BanMembers;
    public const GuildPermission TrueAdministrator = GuildPermission.Administrator;
}