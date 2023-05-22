using Catalina.Database;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord.Events;
public static partial class Events
{

    internal static async Task GuildMemberAdded(SocketGuildUser user)
    {
        using var database = Services.GetRequiredService<DatabaseContext>();

        var guildProperty = database.Guilds.Include(g => g.Roles).FirstOrDefault(g => g.ID == user.Guild.Id);
        if (guildProperty is null) return;

        try
        {
            await user.AddRolesAsync(guildProperty.Roles.Where(r => r.IsAutomaticallyAdded).Select(r => r.ID));
        }
        catch
        {
            Services.GetRequiredService<Logger>().Error("Could not add automatic roles to user");
        }
    }
}
