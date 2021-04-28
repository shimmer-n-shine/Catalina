using Catalina.Configuration;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalina.Discord
{
    class CoreModule : BaseCommandModule
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
        [Command("AddRoleFromReaction")]
        [Description("Add a role when a user reacts to a message")]
        [Aliases("AddReaction")]
        public async Task AddReaction(CommandContext ctx)
        {
            Reaction reaction;
            DiscordEmbed discordEmbed;
            var verification = await IsVerifiedAsync(ctx, true);
            if (verification == PermissionCode.Qualify)
            {
                var id = await GetMessageIDFromLinkAsync(ctx);
                if (id != null)
                {
                    var emoji = await GetEmojiFromMessage(ctx);

                    if (emoji != null)
                    {
                        var role = await GetRoleFromMessage(ctx);

                        if (role != null)
                        {
                            reaction = new Reaction( (ulong) id, emoji, role);
                            ConfigValues.Reactions.Add(reaction);
                        }
                    }
                    
                }
            }
        }

        public async Task<DiscordEmoji> GetEmojiFromMessage(CommandContext ctx)
        {
            DiscordEmbed discordEmbed;
            discordEmbed = Discord.CreateFancyMessage(title: "Adding a new reaction!", description: "Please react with the emoji to watch for", color: DiscordColor.CornflowerBlue);
            await ctx.RespondAsync(discordEmbed);
            var body = await ctx.Message.WaitForReactionAsync(ctx.User, new TimeSpan(0, 1, 0));

            if (!body.TimedOut)
            {
                DiscordEmoji emoji = body.Result.Emoji;
                return emoji;
            }
            else
            {
                discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "You took too long to respond.", color: DiscordColor.Red);
                await Discord.SendFancyMessage(ctx.Channel, discordEmbed);
                return null;
            }
        }

        public async Task<DiscordRole> GetRoleFromMessage(CommandContext ctx)
        {
            DiscordEmbed discordEmbed;
            discordEmbed = Discord.CreateFancyMessage(title: "Adding a new reaction!", description: "Please enter the role id to assign automatically", color: DiscordColor.CornflowerBlue);
            await ctx.RespondAsync(discordEmbed);
            var body = await ctx.Message.GetNextMessageAsync(new TimeSpan(0, 1, 0));

            if (!body.TimedOut)
            {
                var id = await GetRoleIDFromString(body.Result.Content);
                if (id != null)
                {
                    return ctx.Guild.GetRole((ulong) id);
                }
                else
                {
                    discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "The role id you provided was invalid!");
                    return null;
                }
            }
            else
            {
                discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "You took too long to respond.", color: DiscordColor.Red);
                await Discord.SendFancyMessage(ctx.Channel, discordEmbed);
                return null;
            }
        }
        public async Task<ulong?> GetRoleIDFromString(string text)
        {
            try
            {
                ulong id = Convert.ToUInt64(text);
                return id;

            }
            catch
            {
                return null;
            }
        }

        public async Task<ulong?> GetMessageIDFromLinkAsync(CommandContext ctx)
        {
            DiscordEmbed discordEmbed;
            discordEmbed = Discord.CreateFancyMessage(title: "Adding a new reaction!", description: "Please enter the message link to add a reaction to", color: DiscordColor.CornflowerBlue);
            await ctx.RespondAsync(discordEmbed);
            var body = await ctx.Message.GetNextMessageAsync(new TimeSpan(0, 1, 0));

            if (!body.TimedOut)
            {
                var id = GetIDFromLink(body.Result.Content);
                if (id != null)
                {
                    return id;
                }
                else
                {
                    discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "The message link you provided was invalid!");
                    return null;
                }
            }
            else
            {
                discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "You took too long to respond.", color: DiscordColor.Red);
                await Discord.SendFancyMessage(ctx.Channel, discordEmbed);
                return null;
            }
        }

        public ulong? GetIDFromLink(string message)
        {
            try
            {
                var splitMessage = message.Split('/');
                return Convert.ToUInt64(splitMessage.Last());
                
            }
            catch
            {
                return null;
            }
            
        }

        public static async Task<PermissionCode> IsVerifiedAsync(CommandContext ctx, bool isAdminCommand = false, bool isAvailableEverywhere = false)
        {
            DiscordEmbed discordEmbed;
            if (ctx.Member.IsOwner || ctx.Member.Roles.Any(t => t.Permissions == DSharpPlus.Permissions.Administrator))
            {
                return PermissionCode.Qualify;
            }
            if (!Discord.commandChannels.Contains(ctx.Channel) && !isAvailableEverywhere)
            {
                discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.IndianRed, title: "Sorry!", description: "You're not allowed to run that command here.");
                await ctx.RespondAsync(discordEmbed);
                return PermissionCode.UnqualifyChannel;
            }
            if (ctx.Member.Id == ConfigValues.DevID)
            {
                return PermissionCode.Qualify;
            }
            var roleVerf = ctx.Member.Roles.Select(t => t.Id).Intersect(ConfigValues.RoleIDs);
            var adminVerf = ctx.Member.Roles.Select(t => t.Id).Intersect(ConfigValues.AdminRoleIDs);
            if (adminVerf.Any())
            {
                return PermissionCode.Qualify;
            }
            if (ctx.Member.IsOwner)
            {
                return PermissionCode.Qualify;
            }
            if (roleVerf.Any() && !isAdminCommand)
            {
                return PermissionCode.Qualify;
            }
            discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.IndianRed, title: "Sorry!", description: "You're not allowed to run that command.");
            await ctx.RespondAsync(discordEmbed);
            return PermissionCode.UnqualifyRole;
        }
    }
    public enum PermissionCode : ushort
    {
        Qualify,
        UnqualifyRole,
        UnqualifyChannel,
    }

}
