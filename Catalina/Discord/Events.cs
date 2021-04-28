using Catalina.Configuration;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Serilog;
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
            if (reactions.Select(reaction => reaction.messageID).Contains(e.Message.Id) && reactions.Select(reaction => reaction.emoji).Contains(e.Emoji) && !e.User.IsBot) {
                var reaction = reactions.Find(r => r.messageID == e.Message.Id && r.emoji == e.Emoji);
                var member = await e.Guild.GetMemberAsync(e.User.Id);
                await member.GrantRoleAsync(reaction.role, "Assigned role upon reaction");
            }
        }

        internal static async Task Discord_ReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            if (reactions.Select(reaction => reaction.messageID).Contains(e.Message.Id) && reactions.Select(reaction => reaction.emoji).Contains(e.Emoji) && !e.User.IsBot)
            {
                var reaction = reactions.Find(r => r.messageID == e.Message.Id && r.emoji == e.Emoji);
                var member = await e.Guild.GetMemberAsync(e.User.Id);
                await member.RevokeRoleAsync(reaction.role, "Revoked role upon reaction");

            }
        }

        internal static async Task Discord_GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            if (ConfigValues.BasicRoleGuildID.ContainsKey(e.Guild.Id))
            {
                await e.Member.GrantRoleAsync(ConfigValues.BasicRoleGuildID.GetValueOrDefault(e.Guild.Id), "Automatic role assignment upon joining");
            }
        }

        internal static Task Discord_MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
        {
            if (reactions.Select(reaction => reaction.messageID).Contains(e.Message.Id))
            {
                var reactions = Events.reactions.FindAll(r => r.messageID == e.Message.Id);
                reactions.ForEach(reaction =>
                {
                    ConfigValues.Reactions.Remove(reaction);
                });
                ConfigValues.SaveConfig();
            }
            Log.Information("Removed reactions from deleted message!");
            return Task.CompletedTask;
        }
    }
}
