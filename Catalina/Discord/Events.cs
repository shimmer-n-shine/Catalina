using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Catalina.Discord
{
    class Events
    {
        internal static Task Discord_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            throw new NotImplementedException();
        }

        internal static Task Discord_GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            throw new NotImplementedException();
        }

        internal static Task Discord_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            throw new NotImplementedException();
        }

        internal static Task Discord_ReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            if (e.Message.Id == 836627307224629259)
            {
                throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        internal static Task Discord_ReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
