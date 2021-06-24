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
                    var role = GetRoleFromList(ctx.Message.MentionedRoles.ToList(), ctx);

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
                            catch
                            {

                            }

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
                        catch
                        {

                        }
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
                        var message = await GetMessageFromLinkAsync(ctx, messageLink);
                        var emoji = GetEmojiFromString(emote);
                        var mentions = ctx.Message.MentionedRoles.ToList();
                        var role = GetRoleFromList(mentions, ctx);

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
                        var message = await GetMessageFromLinkAsync(ctx);
                        if (message != null)
                        {
                            var emoji = await GetEmojiFromMessage(ctx);

                            if (emoji != null)
                            {
                                var role = await GetRoleFromMessage(ctx);

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
                        var emoji = GetEmojiFromString(messageLink);
                        var message = await GetMessageFromLinkAsync(ctx, messageLink);
                        if (emoji != null)
                        {
                            if (ConfigValues.Reactions.ContainsKey(ctx.Guild.Id)) //ConfigValues.Reactions.Select(reaction => reaction.Value.emoji).Contains(emoji) && )
                            {
                                foreach (var reaction in ConfigValues.Reactions[ctx.Guild.Id])
                                {
                                    try
                                    {
                                        await message.DeleteReactionsEmojiAsync(emoji);
                                    }
                                    catch { }
                                    ConfigValues.Reactions[ctx.Guild.Id].Remove(reaction);
                                }
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
                        var message = await GetMessageFromLinkAsync(ctx);
                        if (message != null)
                        {
                            var emoji = await GetEmojiFromMessage(ctx);

                            if (emoji != null)
                            {

                                if (ConfigValues.Reactions.ContainsKey(ctx.Guild.Id))
                                {
                                    if (ConfigValues.Reactions[ctx.Guild.Id].Select(r => r.messageID).Contains(message.Id) && ConfigValues.Reactions[ctx.Guild.Id].Select(r => r.emojiName).Contains(emoji.Name))
                                    {
                                        var reaction = ConfigValues.Reactions[ctx.Guild.Id].Find(r => r.messageID == message.Id && r.emojiName == emoji.Name);
                                        try
                                        {
                                            await message.DeleteReactionsEmojiAsync(GetEmojiFromString(emoji.Name));
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

        public async Task<DiscordEmoji> GetEmojiFromMessage(CommandContext ctx)
        {
            DiscordEmbed discordEmbed;
            discordEmbed = Discord.CreateFancyMessage(title: "Adding a new reaction!", description: "Please send the emoji to watch for", color: DiscordColor.CornflowerBlue);
            await ctx.RespondAsync(discordEmbed);
            var body = await ctx.Message.GetNextMessageAsync();


            if (!body.TimedOut)
            {
                var emoji = GetEmojiFromString(body.Result.Content);
                if (emoji != null)
                {
                    return emoji;
                }
                else
                {
                    discordEmbed = Discord.CreateFancyMessage(title: "Sorry!", description: "The emoji you provided was invalid!", color: DiscordColor.Red);
                    await Discord.SendFancyMessage(ctx.Channel, discordEmbed);
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

        public async Task<DiscordRole> GetRoleFromMessage(CommandContext ctx)
        {
            DiscordEmbed discordEmbed;
            discordEmbed = Discord.CreateFancyMessage(title: "A new reaction!", description: "Please mention the role to assign", color: DiscordColor.CornflowerBlue);
            await ctx.RespondAsync(discordEmbed);
            var body = await ctx.Message.GetNextMessageAsync(new TimeSpan(0, 1, 0));

            if (!body.TimedOut)
            {
                var role = GetRoleFromList(body.Result.MentionedRoles.ToList(), ctx);
                if (role != null)
                {
                    return role;
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
        public DiscordEmoji GetEmojiFromString(string text)
        {
            var pattern = new Regex("([A-z_]|[0-9]){2,}");
            try
            {
                var result = pattern.Match(text);
                if (result.Success)
                {
                    var match = ':' + result.Value + ':';
                    DiscordEmoji emoji = DiscordEmoji.FromName(Discord.discord, match, true);
                    return emoji;
                }
                else
                {
                    try
                    {
                        DiscordEmoji emoji = DiscordEmoji.FromUnicode(Discord.discord, text);
                        return emoji;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            catch
            {

                return null;
            }
        }
        public DiscordRole GetRoleFromList(List<DiscordRole> roles, CommandContext ctx)
        {
            try
            {
                ulong id = Convert.ToUInt64(roles.First().Id);
                return ctx.Guild.GetRole(id);

            }
            catch
            {
                return null;
            }
        }

        public async Task<DiscordMessage> GetMessageFromLinkAsync(CommandContext ctx, string messageLink = null)
        {
            if (messageLink != null)
            {
                var messageID = GetMessageIDFromLink(messageLink);
                var channelID = GetChannelIDFromLink(messageLink);
                if (messageID != null && channelID != null)
                {
                    var message = await ctx.Guild.GetChannel((ulong)channelID).GetMessageAsync((ulong) messageID); //ctx.Guild.GetChannelAsync((ulong)channelID).GetMessageAsync((ulong)messageID);
                    return message;
                }
                else
                {
                    return null;
                }
            }

            else
            {
                DiscordEmbed discordEmbed;
                discordEmbed = Discord.CreateFancyMessage(title: "A new reaction!", description: "Please enter the message link for the reaction", color: DiscordColor.CornflowerBlue);
                await ctx.RespondAsync(discordEmbed);
                var body = await ctx.Message.GetNextMessageAsync(new TimeSpan(0, 1, 0));

                if (!body.TimedOut)
                {
                    var messageID = GetMessageIDFromLink(body.Result.Content);
                    var channelID = GetChannelIDFromLink(body.Result.Content);
                    if (messageID != null && channelID != null)
                    {
                        return await ctx.Guild.GetChannel((ulong)channelID).GetMessageAsync((ulong)messageID);
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
            
        }
        //public async Task<DiscordMessage> GetMessageFromIDAsync(CommandContext ctx, ulong channelID, ulong messageID)
        //{
        //    DiscordMessage message = null;
        //    try
        //    {
        //        message = await ctx.Guild.GetChannel(channelID).GetMessageAsync(messageID);
        //    }
        //    catch { }
        //    //foreach (var channel in channels)
        //    //{
        //    //    try
        //    //    {
        //    //        message = await channel.GetMessageAsync(messageID);
        //    //        break;
        //    //    }
        //    //    catch
        //    //    {

        //    //    }
        //    //}
             
        //    if (message != null)
        //    {
        //        return message;
        //    }
        //    else
        //    {
        //        return null;
        //    }

        //}

        public ulong? GetMessageIDFromLink(string message)
        {
            try
            {
                var splitMessage = message.Split('/');
                return Convert.ToUInt64(splitMessage.Last());
                
            }
            catch
            { return null; }
            
        }

        public ulong? GetChannelIDFromLink(string message)
        {
            try
            {
                var splitMessage = message.Split('/');
                return Convert.ToUInt64(splitMessage[^2]);
            }
            catch { return null; }
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
