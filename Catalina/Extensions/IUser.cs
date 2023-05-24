using Discord;

namespace Catalina.Extensions;
public static class IUserExtensions
{
    public static string FullName(this IUser user)
    {
        var userName = user.Username;
        if (user.DiscriminatorValue != 0) userName += '#' + user.Discriminator;
        return userName;
    }
}
