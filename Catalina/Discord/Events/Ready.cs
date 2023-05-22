using Discord;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using System.Threading.Tasks;

namespace Catalina.Discord.Events;
public static partial class Events
{
    internal static async Task Ready()
    {
        await Discord.InteractionService.RegisterCommandsGloballyAsync();
        await Discord.DiscordClient.SetGameAsync(type: ActivityType.Watching, name: "Jerma985.");
        Services.GetRequiredService<Logger>().Information("Discord Ready!");
    }
}
