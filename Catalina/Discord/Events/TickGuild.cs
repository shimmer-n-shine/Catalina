using Catalina.Database;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord.Events;
public static partial class Events
{
    internal static async Task TickGuild(IInteractionContext context)
    {
        using var database = Services.GetRequiredService<DatabaseContext>();

        if (database.Guilds.Find(context.Guild.Id) == null)
        {
            var guildProperty = new Database.Models.Guild { ID = context.Guild.Id, StarboardSettings = new Database.Models.StarboardSettings { } };
            database.Guilds.Add(guildProperty);

            await database.SaveChangesAsync();

            guildProperty.StarboardSettings.SetOrCreateEmoji(database.Emojis.AsNoTracking().FirstOrDefault(e => e.NameOrID == ":star:"), database);

            await database.SaveChangesAsync();

        }
    }
}
