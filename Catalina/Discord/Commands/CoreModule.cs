using Catalina.Discord;
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
            var embed = new Utils.AcknowledgementMessage
            {
                Body = "Pong!",
                User = Context.User
            };
            await Context.Message.ReplyAsync(embed: embed);
        }
    }
}
