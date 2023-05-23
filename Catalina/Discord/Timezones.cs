using Catalina.Database.Models;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalina.Database;
using Microsoft.Extensions.DependencyInjection;
using DiscordNET = Discord;
using NodaTime.TimeZones;
using NodaTime;
using Catalina.Extensions;
using Catalina.Core;

namespace Catalina.Discord;
public static class Timezones
{
    public static Dictionary<Role, IRole> Roles = new Dictionary<Role, IRole>();

    public static async Task GetRoles(ServiceProvider services, DiscordNET.WebSocket.DiscordSocketClient client)
    {
        using var database = services.GetRequiredService<DatabaseContext>();

        var dbRoles = database.Roles.Where(r => !string.IsNullOrEmpty(r.Timezone));

        var dbGuilds = dbRoles.GroupBy(r => r.Guild);
        
        foreach (var guildGroup in dbGuilds)
        {
            if (!guildGroup.Key.TimezoneSettings.Enabled) continue;

            var guild = client.GetGuild(guildGroup.Key.ID);
            var roles = guildGroup.ToList();

            foreach (var role in roles)
            {
                Roles.Add(role, guild.GetRole(role.ID));
            }
        }
    }
    public static void AddRole(Role dbRole, IRole role)
    {
        Roles.Add(dbRole, role);
    }

    [InvokeRepeating(interval: Timings.FifteenMinutes, alignTo: AlignTo.OneHour)]
    public static async Task UpdateTimes()
    {
        foreach (var rolePair in Roles)
        {
            var timezone = TzdbDateTimeZoneSource.Default.ZoneLocations.FirstOrDefault(z => z.ZoneId == rolePair.Key.Timezone);
            var instant = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var zonedTime = instant
                .InZone(DateTimeZoneProviders.Tzdb
                .GetZoneOrNull(rolePair.Key.Timezone));
            var shortcode = TzdbDateTimeZoneSource.Default
                .ForId(timezone.ZoneId)
                .GetZoneInterval(instant)
                .Name.Replace("+", "UTC+");

            var localTime = zonedTime
                .ToDateTimeUnspecified()
                .RoundToNearest(TimeSpan.FromMinutes(15));
            await rolePair.Value.ModifyAsync(r => r.Name = $"{localTime:HH:mm} [{shortcode}]");
        }
    }

}
