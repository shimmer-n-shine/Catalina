using Catalina.Common;
using Catalina.Discord.Commands.Preconditions;
using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Modules;

public partial class CoreModule : InteractionModuleBase
{
    [RequirePrivilege(AccessLevel.User)]
    [SlashCommand("ping", "Pong!")]
    public async Task Ping()
    {
        var originalTime = DateTime.UtcNow;
        Embed embed = new Utils.WarningMessage(user: Context.User)
        {
            Title = "Pong!",
            Body = "Latency: " + Discord.DiscordClient.Latency + " ms",
        };
        await RespondAsync(embed: embed);
        var message = await Context.Interaction.GetOriginalResponseAsync();
        var latency = (message.Timestamp - originalTime).TotalMilliseconds;

        embed = new Utils.AcknowledgementMessage (user: Context.User)
        {
            Title = "Pong!",
            Body = "Latency: " + latency + " ms",
        };
        await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embed);
    }
} 
