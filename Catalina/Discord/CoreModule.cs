using Catalina.Configuration;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Catalina.Discord
{
    class CoreModule : BaseCommandModule
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;

        [Command("updateconfig")]
        [Description("Update configuration for the bot. Only admins can execute this.")]
        [Aliases("updateconf", "conf", "confupdate")]
        public async Task UpdateConf(CommandContext ctx)
        {
            var verification = await IsVerifiedAsync(ctx, true);
            if (verification == PermissionCode.Qualify)
            {
                var discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.Orange, title: "Updating Configuration Files...");
                DiscordMessage message = await ctx.RespondAsync(discordEmbed);


                ConfigValues.LoadConfig();
                await Discord.UpdateChannels();
                discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.SpringGreen, title: "Done!", description: "Sucessfully updated configuration files");
                await message.ModifyAsync(discordEmbed);
            }
        }

        [Command("Setbasicrole")]
        [Description("Set the basic role for your server! This is an admin exclusive command.")]
        [Aliases("BasicRole", "basic")]
        public async Task SetBasicRole(CommandContext ctx, string mention)
        {
            DiscordEmbed discordEmbed;
            var verification = await IsVerifiedAsync(ctx, true);
            if (verification == PermissionCode.Qualify)
            {
                if (mention != null)
                {
                    var role = Discord.GetRoleFromList(ctx.Message.MentionedRoles.ToList(), ctx);

                    if (ConfigValues.BasicRoleGuildID.ContainsKey(ctx.Guild.Id))
                    {
                        foreach (var member in ctx.Guild.Members)
                        {
                            try
                            {
                                if (member.Value.Roles.Contains(ConfigValues.BasicRoleGuildID.GetValueOrDefault(ctx.Guild.Id)))
                                {
                                    await member.Value.RevokeRoleAsync(ConfigValues.BasicRoleGuildID.GetValueOrDefault(ctx.Guild.Id), "Automatic revokal of the basic role");
                                }
                            }
                            catch { }

                        }
                        ConfigValues.BasicRoleGuildID.Remove(ctx.Guild.Id);
                        
                    }
                    ConfigValues.BasicRoleGuildID.Add(ctx.Guild.Id, role);
                    ConfigValues.SaveConfig();

                    foreach (var member in ctx.Guild.Members)
                    {
                        try
                        {
                            await member.Value.GrantRoleAsync(role, "Automatic assignment of the basic role");
                        }
                        catch { }
                    }

                    discordEmbed = Discord.CreateFancyMessage(title: "Done!", description: "Set the basic role for your server!", color: role.Color);
                    await ctx.RespondAsync(discordEmbed);
                }
                else
                {
                    discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "The role you provided was invalid!", color: DiscordColor.Red);
                    await ctx.RespondAsync(discordEmbed);
                }
            }
        }

        [Command("React")]
        [Description("Add a role when a user reacts to a message")]
        [Aliases("Reaction")]
        public async Task AddReaction(CommandContext ctx, string arg = null, string messageLink = null, string emote = null, string mention = null)
        {
            //Reaction reaction;
            DiscordEmbed discordEmbed;
            var verification = await IsVerifiedAsync(ctx, true);
            if (verification == PermissionCode.Qualify)
            {
                
                if (arg != null && arg.ToLower() == "add")
                {
                    if (!ctx.Guild.CurrentMember.Roles.Any(r => r.Permissions.HasFlag(DSharpPlus.Permissions.ManageRoles) || r.Permissions.HasFlag(DSharpPlus.Permissions.Administrator)))
                    {
                        discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "I don't have permissions to manage roles.", color: DiscordColor.Red);
                        await ctx.RespondAsync(discordEmbed);
                        return;
                    }
                    if (messageLink != null && emote != null && mention != null)
                    {
                        var message = await Discord.GetMessageFromLinkAsync(ctx, messageLink);
                        var emoji = Discord.GetEmojiFromString(emote);
                        var mentions = ctx.Message.MentionedRoles.ToList();
                        var role = Discord.GetRoleFromList(mentions, ctx);

                        if (message == null)
                        {
                            discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "You provided an invalid message link.", color: DiscordColor.Red);
                            await ctx.RespondAsync(discordEmbed);
                            return;
                        }
                        
                        if (role != null && ctx.Guild.CurrentMember.Roles.All(r => r.Position < role.Position))
                        {
                            discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "I don't have permissions to manage roles.", color: DiscordColor.Red);
                            await ctx.RespondAsync(discordEmbed);
                            return;
                        } 


                        if (message != null && emoji != null && role != null)
                        {
                            discordEmbed = Discord.CreateFancyMessage(title: "Done!", description: "Added reaction to list of reactions!", color: role.Color);
                            await ctx.RespondAsync(discordEmbed);
                            var reaction = new Reaction(message.Id, emoji.Name, role.Id, message.Channel.Id);

                            if (ConfigValues.Reactions.ContainsKey(ctx.Guild.Id)) 
                            {
                                //var reactions = ConfigValues.Reactions.GetValueOrDefault(ctx.Guild.Id);
                                //reactions.Add(reaction);
                                
                                ConfigValues.Reactions[ctx.Guild.Id].Add(reaction);

                            }
                            else
                            {
                                ConfigValues.Reactions.Add(ctx.Guild.Id, new () { reaction });
                            }

                            ConfigValues.SaveConfig();
                            await message.CreateReactionAsync(emoji);

                        }
                        else
                        {
                            discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "The message link, role or reaction you provided was invalid!", color: DiscordColor.Red);
                            await ctx.RespondAsync(discordEmbed);
                        }
                    }
                    else
                    {
                        var message = await Discord.GetMessageFromLinkAsync(ctx);
                        if (message != null)
                        {
                            var emoji = await Discord.GetEmojiFromMessage(ctx);

                            if (emoji != null)
                            {
                                var role = await Discord.GetRoleFromMessage(ctx);

                                if (role != null)
                                {
                                    if (ctx.Guild.CurrentMember.Roles.All(r => r.Position < role.Position))
                                    {
                                        discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "I don't have permissions to manage roles.", color: DiscordColor.Red);
                                        await ctx.RespondAsync(discordEmbed);
                                        return;
                                    }

                                    discordEmbed = Discord.CreateFancyMessage(title: "Done!", description: "Added reaction to list of reactions!", color: role.Color);
                                    await ctx.RespondAsync(discordEmbed);
                                    var reaction = new Reaction(message.Id, emoji.Name, role.Id, message.Channel.Id);
                                    await message.CreateReactionAsync(emoji);

                                    if (ConfigValues.Reactions.ContainsKey(ctx.Guild.Id))
                                    {
                                        ConfigValues.Reactions[ctx.Guild.Id].Add(reaction);
                                    }
                                    else
                                    {
                                        ConfigValues.Reactions.Add(ctx.Guild.Id, new() { reaction });
                                    }


                                    ConfigValues.SaveConfig();
                                }
                            }
                        }
                    }
                    
                }
                else if (arg != null && arg.ToLower() == "remove")
                {
                    if (messageLink != null)
                    {
                        //in this case, message link is the reaction OR message to unlist.
                        var emoji = Discord.GetEmojiFromString(messageLink);
                        var message = await Discord.GetMessageFromLinkAsync(ctx, messageLink);
                        if (emoji != null)
                        {
                            if (ConfigValues.Reactions.ContainsKey(ctx.Guild.Id)) //ConfigValues.Reactions.Select(reaction => reaction.Value.emoji).Contains(emoji) && )
                            {
                                //var reactions = ConfigValues.Reactions[ctx.Guild.Id];
                                List<Reaction> removedItems = new List<Reaction>();
                                ConfigValues.Reactions[ctx.Guild.Id].Where(reaction => reaction.emojiName == emoji.Name).ToList().ForEach(async reaction =>
                                {
                                    removedItems.Add(reaction);

                                    try
                                    {
                                        message = await ctx.Guild.GetChannel(reaction.channelID).GetMessageAsync(reaction.messageID);
                                        try
                                        {
                                            await message.DeleteReactionsEmojiAsync(emoji);
                                        }
                                        catch { }
                                    }
                                    catch { }

                                });

                                removedItems.ForEach(item => ConfigValues.Reactions[ctx.Guild.Id].Remove(item));
                                ConfigValues.SaveConfig();
                                discordEmbed = Discord.CreateFancyMessage(title: "Done!", description: "Removed reaction from list of reactions!", color: DiscordColor.SpringGreen);
                                await ctx.RespondAsync(discordEmbed);
                            }
                            else
                            {
                                discordEmbed = Discord.CreateFancyMessage(title: "Error!", description: "You've not added any reactions to remove!", color: DiscordColor.Red);
                            }
                        }
                        else if (message != null)
                        {
                            if (ConfigValues.Reactions.ContainsKey(ctx.Guild.Id))
                            {
                                if (ConfigValues.Reactions[ctx.Guild.Id].Select(r => r.messageID).Contains(message.Id))
                                {
                                    var reactions = ConfigValues.Reactions[ctx.Guild.Id].FindAll(r => r.messageID == message.Id);
                                    await message.DeleteAllReactionsAsync();
                                    foreach (var reaction in reactions)
                                    {
                                        ConfigValues.Reactions[ctx.Guild.Id].Remove(reaction);
                                    }
                                    ConfigValues.SaveConfig();
                                    discordEmbed = Discord.CreateFancyMessage(title: "Done!", description: "Removed reaction from list of reactions!", color: DiscordColor.SpringGreen);
                                    await ctx.RespondAsync(discordEmbed);
                                }
                            }
                            else
                            {
                                discordEmbed = Discord.CreateFancyMessage(title: "Error!", description: "You've not added any reactions to remove!", color: DiscordColor.Red);
                            }

                        }
                        else
                        {
                            discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "The message link or reaction you provided was invalid!", color: DiscordColor.Red);
                            await ctx.RespondAsync(discordEmbed);
                        }
                        
                    } 
                    else
                    {
                        var message = await Discord.GetMessageFromLinkAsync(ctx);
                        if (message != null)
                        {
                            var emoji = await Discord.GetEmojiFromMessage(ctx);

                            if (emoji != null)
                            {

                                if (ConfigValues.Reactions.ContainsKey(ctx.Guild.Id))
                                {
                                    if (ConfigValues.Reactions[ctx.Guild.Id].Select(r => r.messageID).Contains(message.Id) && ConfigValues.Reactions[ctx.Guild.Id].Select(r => r.emojiName).Contains(emoji.Name))
                                    {
                                        var reaction = ConfigValues.Reactions[ctx.Guild.Id].Find(r => r.messageID == message.Id && r.emojiName == emoji.Name);
                                        try
                                        {
                                            await message.DeleteReactionsEmojiAsync(Discord.GetEmojiFromString(emoji.Name));
                                        }
                                        catch { }
                                        ConfigValues.Reactions[ctx.Guild.Id].Remove(reaction);
                                        ConfigValues.SaveConfig();

                                        discordEmbed = Discord.CreateFancyMessage(title: "Done!", description: "Removed reaction from list of reactions!", color: DiscordColor.SpringGreen);
                                        await ctx.RespondAsync(discordEmbed);
                                    }
                                }
                            }
                        }
                            
                    }
                    
                }
                else
                {
                    discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "You didn't provide an argument! try `add` or `remove`!", color: DiscordColor.Red);
                    await ctx.RespondAsync(discordEmbed);
                }
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
            var roleVerf = ctx.Member.Roles.Select(t => t.Id).Intersect(ConfigValues.RoleIDs[ctx.Guild.Id]);
            var adminVerf = ctx.Member.Roles.Select(t => t.Id).Intersect(ConfigValues.AdminRoleIDs[ctx.Guild.Id]);
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
