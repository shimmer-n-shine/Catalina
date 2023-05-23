using Catalina.Database;
using Discord.WebSocket;
using Discord;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Catalina.Discord.Events;
public static partial class Events
{
    internal static async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        if (reaction.User.Value.IsBot || reaction.User.Value.IsWebhook) return;
        using var database = Services.GetRequiredService<DatabaseContext>();

        var guild = (channel.Value as IGuildChannel).Guild;
        if (database.Guilds.Any(g => g.ID == guild.Id))
        {
            Database.Models.Guild guildProperty = null;
            try
            {
                //guildProperty = database.GuildProperties.Include(g => g.StarboardEmoji).First(g => g.ID == guild.Id);
                guildProperty = database.Guilds.First(g => g.ID == guild.Id);
            }
            catch
            {

            }


            var emoji = await Database.Models.Emoji.ParseAsync(reaction.Emote, guild);

            if (emoji.NameOrID == guildProperty.StarboardSettings.Emoji.NameOrID)
            {
                await Starboard.ProcessVote(guildProperty, await message.GetOrDownloadAsync(), reaction.User.Value);
            }
        }


    }
}
