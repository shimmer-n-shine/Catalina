using Catalina.Discord.Commands.Preconditions;
using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands
{
    public class CoreModule : InteractionModuleBase
    {
        [RequirePrivilege(AccessLevel.User)]
        [SlashCommand("ping", "Pong!")]
        public async Task PingMe()
        {
            var originalTime = DateTime.UtcNow;
            Embed embed = new Utils.WarningMessage
            {
                Title = "Pong!",
                Body = "Latency: " + Discord.discord.Latency + " ms",
                User = Context.User
            };
            await RespondAsync(embed: embed);
            var message = await Context.Interaction.GetOriginalResponseAsync();
            var latency = (message.Timestamp - originalTime).TotalMilliseconds;

            embed = new Utils.AcknowledgementMessage
            {
                Title = "Pong!",
                Body = "Latency: " + latency + " ms",
                User = Context.User
            };
            await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embed);
        }
    }
}
