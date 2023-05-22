using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Catalina.Discord.Events;
public static partial class Events
{
    internal static async Task InteractionCreated(SocketInteraction socketInteraction)
    {
        var context = new SocketInteractionContext(Discord.DiscordClient, socketInteraction);
        await TickGuild(context);
        await Discord.InteractionService.ExecuteCommandAsync(context, Services);
    }
}
