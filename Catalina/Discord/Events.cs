using Catalina.Configuration;
using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalina.Discord
{
    class Events
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
        static List<Response> responses = ConfigValues.Responses;
        static List<Reaction> reactions = ConfigValues.Reactions;
        internal static Task Discord_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            throw new NotImplementedException();
        }

        internal static Task Discord_GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            throw new NotImplementedException();
        }

        internal static async Task Discord_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (responses.Select(response => response.trigger).Contains(e.Message.Content))
            {
                var response = responses.Find(r => r.trigger == e.Message.Content);
                if (response.disallowedChannels == null || !response.disallowedChannels.Contains(e.Channel)) {
                    await e.Message.RespondAsync(response.response);
                }
            }
        }

        internal static async Task Discord_ReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            if (reactions.Select(reaction => reaction.messageID).Contains(e.Message.Id) && reactions.Select(reaction => reaction.emoji).Contains(e.Emoji)) {
                var reaction = reactions.Find(r => r.messageID == e.Message.Id && r.emoji == e.Emoji);
                var member = await e.Guild.GetMemberAsync(e.User.Id);
                await member.GrantRoleAsync(reaction.role, "Assigned role upon reaction");
            }
        }

        internal static async Task Discord_ReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            if (reactions.Select(reaction => reaction.messageID).Contains(e.Message.Id) && reactions.Select(reaction => reaction.emoji).Contains(e.Emoji))
            {
                var reaction = reactions.Find(r => r.messageID == e.Message.Id && r.emoji == e.Emoji);
                var member = await e.Guild.GetMemberAsync(e.User.Id);
                await member.RevokeRoleAsync(reaction.role, "Revoked role upon reaction");

            }
        }

        internal static Task Discord_MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
        {
            if (reactions.Select(reaction => reaction.messageID).Contains(e.Message.Id))
            {
                var reaction = reactions.Find(r => r.messageID == e.Message.Id);
                ConfigValues.Reactions.Remove(reaction);
                ConfigValues.SaveConfig();
            }
            return Task.CompletedTask;
        }
    }
}
