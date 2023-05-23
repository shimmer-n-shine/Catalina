using Catalina.Core;
using Catalina.Database;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Catalina.Discord.Events;
public static partial class Events 
{
    [Invoke(alignTo: AlignTo.OneMinute)]
    internal static async Task SetupTimezones()
    {
        using var database = Services.GetRequiredService<DatabaseContext>();

        await Timezones.GetRoles(Services, Discord.DiscordClient);
    }
}
