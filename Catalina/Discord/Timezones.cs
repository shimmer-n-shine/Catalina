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

namespace Catalina.Discord;
public static class Timezones
{
    public static Dictionary<Role, IRole> Roles = new Dictionary<Role, IRole>();

    public static async Task GetRoles(ServiceProvider services, DiscordNET.WebSocket.DiscordSocketClient client)
    {
        using var database = services.GetRequiredService<DatabaseContext>();

        var dbRoles = database.Roles.Where(r => !string.IsNullOrEmpty(r.Timezone)).ToList();
        var dbGuilds = new List<Guild>();

        dbRoles.ForEach(r =>
        {
            if (!dbGuilds.Contains(r.Guild)) dbGuilds.Add(r.Guild);
        });

        dbGuilds.ForEach(dbGuild =>
        {
            var guild = client.GetGuild(dbGuild.ID);
            dbGuild.Roles.Where(g => string.IsNullOrEmpty(g.Timezone) && !Roles.ContainsKey(g)).ToList().ForEach(r =>
            {
                Roles.Add(r, guild.GetRole(r.ID));
            });
        });

    }
    public static void AddRole(Role dbRole, IRole role)
    {
        Roles.Add(dbRole, role);
    }

    //                    60 minutes         align with nearest hour
    [Core.ScheduledInvoke(interval: 60 * 60, hourAlign: true)]
    public static async Task Tick()
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
                .GetZoneInterval(instant);

            var localTime = zonedTime
                .ToDateTimeUnspecified()
                .RoundToNearest(TimeSpan.FromMinutes(15));
            await rolePair.Value.ModifyAsync(r => r.Name = $"[{shortcode.Name}] {localTime:'HH:mm'}");
        }
    }

}
