using Catalina.Discord.Commands.Preconditions;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands
{
    [Group, Name("Core Functionality"), Remarks("The commands required for catalina to function.")]
    [RequireRole(AccessLevel.User)]
    public class CoreModule : ModuleBase
    {

        [Command("Ping")]
        [Summary("Pong!")]
        public async Task PingMe()
        {
            Embed embed = new Utils.WarningMessage
            {
                Title = "Pong!",
                Body = "Latency: " + Discord.discord.Latency + " ms",
                User = Context.User
            };
            var message = await Context.Message.ReplyAsync(embed: embed);
            embed = new Utils.AcknowledgementMessage
            {
                Title = "Pong!",
                Body = "Latency: " + (message.Timestamp - Context.Message.Timestamp + System.TimeSpan.FromMilliseconds(Discord.discord.Latency)).TotalMilliseconds + " ms",
                User = Context.User
            };
            await message.ModifyAsync(msg => msg.Embed = embed);
        }
    }
}
