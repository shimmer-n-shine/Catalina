using Catalina.Configuration;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord
{
    class Events
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;

        internal static Task Discord_GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            throw new NotImplementedException();
        }

        internal static async Task Discord_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            throw new NotImplementedException();
        }

        internal static async Task Discord_ReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            
            if (ConfigValues.Reactions.ContainsKey(e.Guild.Id))
            {
                var reactions = ConfigValues.Reactions[e.Guild.Id];
                if (reactions.Select(reaction => reaction.messageID).Contains(e.Message.Id) && reactions.Select(reaction => reaction.emojiID).Contains(e.Emoji.Id) && !e.User.IsBot)
                {
                    var reaction = reactions.Find(r => r.messageID == e.Message.Id && r.emojiID == e.Emoji.Id);
                    var member = await e.Guild.GetMemberAsync(e.User.Id);
                    await member.GrantRoleAsync(e.Guild.GetRole(reaction.roleID), "Assigned role upon reaction");
                }
            }
        }

        internal static async Task Discord_ReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            if (ConfigValues.Reactions.ContainsKey(e.Guild.Id))
            {
                var reactions = ConfigValues.Reactions[e.Guild.Id];
                if (reactions.Select(reaction => reaction.messageID).Contains(e.Message.Id) && reactions.Select(reaction => reaction.emojiID).Contains(e.Emoji.Id) && !e.User.IsBot)
                {
                    var reaction = reactions.Find(r => r.messageID == e.Message.Id && r.emojiID == e.Emoji.Id);
                    var member = await e.Guild.GetMemberAsync(e.User.Id);
                    await member.RevokeRoleAsync(e.Guild.GetRole(reaction.roleID), "Revoked role upon reaction");

                }
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
            if (ConfigValues.Reactions.ContainsKey(e.Guild.Id))
            {
                var reactions = ConfigValues.Reactions[e.Guild.Id];
                if (reactions.Select(reaction => reaction.messageID).Contains(e.Message.Id))
                {
                    var matches = reactions.FindAll(r => r.messageID == e.Message.Id);
                    matches.ForEach(match =>
                    {
                        ConfigValues.Reactions[e.Guild.Id].Remove(match);
                    });
                    ConfigValues.SaveConfig();
                }
            }
            Log.Information("Removed reactions from deleted message!");
            return Task.CompletedTask;
        }
    }
}
