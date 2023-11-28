using Catalina.Core;
using Catalina.Database;
using Catalina.Database.Models;
using Catalina.Extensions;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.TimeZones;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DiscordNET = Discord;

namespace Catalina.Discord;
public static class Timezones
{
    public static Dictionary<Role, IRole> Roles = new Dictionary<Role, IRole>();
    private static ServiceProvider Services;

    public static async Task GetRoles(ServiceProvider services, DiscordNET.WebSocket.DiscordSocketClient client)
    {
        Services = services;

        using var database = services.GetRequiredService<DatabaseContext>();

        var dbRoles = database.Roles.Where(r => !string.IsNullOrEmpty(r.Timezone));

        var dbGuilds = dbRoles.GroupBy(r => r.Guild);

        foreach (var guildGroup in dbGuilds)
        {
            //disable when option reintroduced
            //if (!guildGroup.Key.TimezoneSettings.Enabled) continue;

            var guild = client.GetGuild(guildGroup.Key.ID);
            if (guild is null) continue;
            var roles = guildGroup.ToList();

            foreach (var role in roles)
            {
                var guildRole = guild.GetRole(role.ID);
                if (guildRole is not null) Roles.Add(role, guildRole);
            }
        }
    }
    public static void AddRole(Role dbRole, IRole role)
    {
        Roles.Add(dbRole, role);
        Services.GetService<Logger>().Debug($"Added {role.Name} to scheduled roles");
    }
    public static void RemoveRole(Role dbRole)
    {

        Roles.Remove(Roles.First(r => r.Key.ID == dbRole.ID).Key);
    }
    public static void RemoveRole(IRole role)
    {

        Roles.Remove(Roles.First(r => r.Value.Id == role.Id).Key);
    }

    [InvokeRepeating(interval: Timings.OneMinute, alignTo: AlignTo.OneMinute)]
    public static async Task UpdateTimes()
    {
        var logger = Services.GetService<Logger>();
        foreach (var rolePair in Roles)
        {
            try
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
                    .RoundToNearest(TimeSpan.FromMinutes(1));
                await rolePair.Value.ModifyAsync(r => r.Name = $"{localTime:HH:mm} [{shortcode}]");
                logger.Information($"Updated timezone display for {rolePair.Value.Name}");
            }
            catch (Exception ex)
            {
                logger.Error(messageTemplate: $"Failed to update timezone display for {rolePair.Value.Name}", exception: ex);
                continue; 
            }
        }
    }

}
