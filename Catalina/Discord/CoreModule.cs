using Catalina.Configuration;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    if (messageLink != null && emote != null && mention != null)
                    {
                        var messageID = GetMessageIDFromLink(messageLink);
                        var emoji = GetEmojiFromString(emote);
                        var mentions = ctx.Message.MentionedRoles.ToList();
                        var role = GetRoleFromList(mentions, ctx);
                        DiscordMessage message = null;
                        if (messageID != null) message = await GetMessageFromIDAsync(ctx, (ulong)messageID);


                        if (message != null && emoji != null && role != null)
                        {
                            discordEmbed = Discord.CreateFancyMessage(title: "Done!", description: "Added reaction to list of reactions!", color: DiscordColor.SpringGreen);
                            await ctx.RespondAsync(discordEmbed);
                            var reaction = new Reaction(message.Id, emoji, role, message.Channel.Id);

                            ConfigValues.Reactions.Add(reaction);
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
                        var id = await GetMessageIDFromLinkAsync(ctx);
                        if (id != null)
                        {
                            var message = await GetMessageFromIDAsync(ctx, (ulong) id);
                            if (message != null)
                            {
                                var emoji = await GetEmojiFromMessage(ctx);

                                if (emoji != null)
                                {
                                    var role = await GetRoleFromMessage(ctx);

                                    if (role != null)
                                    {
                                        discordEmbed = Discord.CreateFancyMessage(title: "Done!", description: "Added reaction to list of reactions!", color: DiscordColor.SpringGreen);
                                        await ctx.RespondAsync(discordEmbed);
                                        var reaction = new Reaction(message.Id, emoji, role, message.Channel.Id);
                                        await message.CreateReactionAsync(emoji);
                                        ConfigValues.Reactions.Add(reaction);
                                        ConfigValues.SaveConfig();
                                    }
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
                        var messageID = GetMessageIDFromLink(messageLink);
                        DiscordMessage message = null;
                        if (messageID != null) message = await GetMessageFromIDAsync(ctx, (ulong)messageID);
                        if (emoji != null)
                        {
                            if (ConfigValues.Reactions.Select(reaction => reaction.emoji).Contains(emoji))
                            {
                                var reactions = ConfigValues.Reactions.FindAll(r => r.emoji == emoji);
                                foreach (var reaction in reactions)
                                {
                                    message = await ctx.Guild.GetChannel(reaction.channelID).GetMessageAsync(reaction.messageID);
                                    await message.DeleteReactionsEmojiAsync(emoji);
                                    ConfigValues.Reactions.Remove(reaction);
                                }
                                //reactions.ForEach(reaction =>
                                //{
                                //    reaction.message.DeleteReactionsEmojiAsync(reaction.emoji);
                                //    ConfigValues.Reactions.Remove(reaction);
                                //});
                                ConfigValues.SaveConfig();
                                discordEmbed = Discord.CreateFancyMessage(title: "Done!", description: "Removed reaction from list of reactions!", color: DiscordColor.SpringGreen);
                                await ctx.RespondAsync(discordEmbed);
                            }
                        }
                        else if (message != null)
                        {
                            if (ConfigValues.Reactions.Select(reaction => reaction.messageID).Contains((ulong) messageID))
                            {
                                var reactions = ConfigValues.Reactions.FindAll(r => r.messageID == messageID);
                                await message.DeleteAllReactionsAsync();
                                foreach (var reaction in reactions)
                                {
                                    ConfigValues.Reactions.Remove(reaction);
                                }
                                //reactions.ForEach(reaction =>
                                //{
                                //    reaction.message.DeleteReactionsEmojiAsync(reaction.emoji);
                                //    ConfigValues.Reactions.Remove(reaction);
                                //});
                                ConfigValues.SaveConfig();
                                discordEmbed = Discord.CreateFancyMessage(title: "Done!", description: "Removed reaction from list of reactions!", color: DiscordColor.SpringGreen);
                                await ctx.RespondAsync(discordEmbed);
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
                        var id = await GetMessageIDFromLinkAsync(ctx);
                       
                        if (id != null)
                        {
                            var message = await GetMessageFromIDAsync(ctx, (ulong) id);
                            if (message != null)
                            {
                                var emoji = await GetEmojiFromMessage(ctx);

                                if (emoji != null)
                                {

                                    if (ConfigValues.Reactions.Select(reaction => reaction.messageID).Contains((ulong)id) && ConfigValues.Reactions.Select(reaction => reaction.emoji).Contains(emoji))
                                    {
                                        var reaction = ConfigValues.Reactions.Find(r => r.messageID == (ulong)id && r.emoji == emoji);
                                        await message.DeleteReactionsEmojiAsync(reaction.emoji);
                                        ConfigValues.Reactions.Remove(reaction);
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

        [Command("responses")]
        [Description("View current responses stored and watched for")]
        [Aliases("response")]
        public async Task Responses(CommandContext ctx, string args = null, [RemainingText] string text = null)
        {
            var roles = ctx.Member.Roles.ToList();
            var verification = await CoreModule.IsVerifiedAsync(ctx);
            if (verification == PermissionCode.Qualify)
            {
                    List<Response> responses = ConfigValues.Responses;
                    bool fail = false;
                    if (args == "list")
                    {
                        List<Field> fields = new List<Field>();
                        foreach (Response response in responses)
                        {
                            fields.Add(new Field(response.trigger, response.description));
                        }
                        var Embed = Discord.CreateFancyMessage(color: DiscordColor.CornflowerBlue, title: "Responses", description: "Control Catamagne's behaviours when responding to users:", fields: fields);
                        var Message = await Discord.SendFancyMessage(ctx.Channel, Embed);
                    }
                    else if (args == "add")
                    {
                        if (text != null)
                        {
                            var trigger = text;
                            var Embed = Discord.CreateFancyMessage(color: DiscordColor.Orange, title: "Adding Response", description: "Please send the text to use as the body of the response:");
                            await Discord.SendFancyMessage(ctx.Channel, Embed);
                            var body = await ctx.Message.GetNextMessageAsync();
                            if (!body.TimedOut)
                            {
                                Embed = Discord.CreateFancyMessage(color: DiscordColor.Yellow, title: "Adding Response", description: "Please give a description of the response:");
                                await Discord.SendFancyMessage(ctx.Channel, Embed);
                                var description = await ctx.Message.GetNextMessageAsync();
                                if (!description.TimedOut)
                                {
                                    Embed = Discord.CreateFancyMessage(color: DiscordColor.SpringGreen, title: "Working", description: "Please enter the channel ids where the responseis allowed, seperated by commas.\nsend 'all' for all channels");
                                    await Discord.SendFancyMessage(ctx.Channel, Embed);
                                    var channelString = await ctx.Message.GetNextMessageAsync();
                                    if (!channelString.TimedOut)
                                    {
                                        var workingList = ConfigValues.Responses.ToList();
                                        if (channelString.Result.Content == "all" || string.IsNullOrWhiteSpace(channelString.Result.Content))
                                        {
                                            var response = new Response(trigger, body.Result.Content, description.Result.Content);
                                            workingList.Add(response);
                                        }
                                        else
                                        {
                                            string[] channelsStrings = string.Join("", channelString.Result.Content.Where(t => !char.IsWhiteSpace(t))).Split(',');
                                            List<DiscordChannel> channels = new List<DiscordChannel>();
                                            channelsStrings.ToList().ForEach(async channel =>
                                            {
                                                channels.Add(await Discord.discord.GetChannelAsync(Convert.ToUInt64(channel)));
                                            });
                                            var response = new Response(trigger, body.Result.Content, description.Result.Content, channels);
                                            workingList.Add(response);
                                        }
                                        ConfigValues.Responses = workingList;
                                        ConfigValues.SaveConfig();
                                        Embed = Discord.CreateFancyMessage(color: DiscordColor.CornflowerBlue, title: "Added", description: "Successfully added response to pool.");
                                        var message = await Discord.SendFancyMessage(ctx.Channel, Embed);

                                    }
                                    else fail = true;
                                }
                                else fail = true;

                            }
                            else fail = true;
                            if (fail)
                            {
                                var discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.IndianRed, title: "Sorry!", description: "You took too long to respond.");
                                await ctx.RespondAsync(discordEmbed);
                            }
                        }
                        else
                        {
                            var discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.IndianRed, title: "Sorry!", description: "Please mention a trigger to add");
                            await ctx.RespondAsync(discordEmbed);
                        }

                    }
                    else if (args == "edit")
                    {
                        if (text != null)
                        {
                            var Embed = Discord.CreateFancyMessage(color: DiscordColor.SpringGreen, title: "Working", description: "Finding response from pool");
                            var message = await Discord.SendFancyMessage(ctx.Channel, Embed);
                            var match = ConfigValues.Responses.Select(t => t.trigger);
                            if (match.Contains(text))
                            {
                                var trigger = text;
                                Embed = Discord.CreateFancyMessage(color: DiscordColor.Orange, title: "Editing Response", description: "Please send the text to use as the body of the response:");
                                await message.ModifyAsync(Embed);
                                var body = await ctx.Message.GetNextMessageAsync();
                                if (!body.TimedOut)
                                {
                                    Embed = Discord.CreateFancyMessage(color: DiscordColor.Yellow, title: "Editing Response", description: "Please give a description of the response:");
                                    await Discord.SendFancyMessage(ctx.Channel, Embed);
                                    var description = await ctx.Message.GetNextMessageAsync();
                                    if (!description.TimedOut)
                                    {
                                        Embed = Discord.CreateFancyMessage(color: DiscordColor.SpringGreen, title: "Editing Response", description: "Please enter the channel ids where the responseis allowed, seperated by commas.\nsend 'all' for all channels");
                                        await Discord.SendFancyMessage(ctx.Channel, Embed);
                                        var channelString = await ctx.Message.GetNextMessageAsync();
                                        if (!channelString.TimedOut)
                                        {
                                            Embed = Discord.CreateFancyMessage(color: DiscordColor.SpringGreen, title: "Working", description: "Updating response.");
                                            message = await Discord.SendFancyMessage(ctx.Channel, Embed);
                                            var _ = ConfigValues.Responses.ToList();
                                            _.Remove(ConfigValues.Responses.ToList().Find(t => t.trigger == text));
                                            if (channelString.Result.Content == "all" || string.IsNullOrWhiteSpace(channelString.Result.Content))
                                            {
                                                var response = new Response(trigger, body.Result.Content, description.Result.Content);
                                                _.Add(response);
                                            }
                                            else
                                            {
                                                string[] channelsStrings = string.Join("", channelString.Result.Content.Where(t => !char.IsWhiteSpace(t))).Split(',');
                                                List<DiscordChannel> channels = new List<DiscordChannel>();
                                                channelsStrings.ToList().ForEach(async channel =>
                                                {
                                                    channels.Add(await Discord.discord.GetChannelAsync(Convert.ToUInt64(channel)));
                                                });
                                                var response = new Response(trigger, body.Result.Content, description.Result.Content, channels);
                                                _.Add(response);
                                            }

                                            ConfigValues.Responses = _;
                                            ConfigValues.SaveConfig();
                                            Embed = Discord.CreateFancyMessage(color: DiscordColor.CornflowerBlue, title: "Updated", description: "Successfully updated response.");
                                            await message.ModifyAsync(Embed);

                                        }
                                        else fail = true;
                                    }
                                    else fail = true;

                                }
                                else fail = true;
                                if (fail)
                                {
                                    var discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.IndianRed, title: "Sorry!", description: "You took too long to respond.");
                                    await ctx.RespondAsync(discordEmbed);
                                }

                            }
                            else
                            {
                                var discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.IndianRed, title: "Sorry!", description: "Trigger not found");
                                await ctx.RespondAsync(discordEmbed);
                            }
                        }
                        else
                        {
                            var discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.IndianRed, title: "Sorry!", description: "Please mention a trigger to update");
                            await ctx.RespondAsync(discordEmbed);
                        }
                    }
                    else if (args == "remove")
                    {
                        if (text != null)
                        {
                            var Embed = Discord.CreateFancyMessage(color: DiscordColor.SpringGreen, title: "Working", description: "Removing response from pool.");
                            var message = await Discord.SendFancyMessage(ctx.Channel, Embed);
                            var match = ConfigValues.Responses.Select(t => t.trigger);
                            if (match.Contains(text))
                            {
                                var _ = ConfigValues.Responses.ToList();
                                _.Remove(ConfigValues.Responses.ToList().Find(t => t.trigger == text));
                                ConfigValues.Responses = _;
                                ConfigValues.SaveConfig();
                                Embed = Discord.CreateFancyMessage(color: DiscordColor.SpringGreen, title: "Removed", description: "Successfully removed response from pool.");
                                message = await message.ModifyAsync(Embed);
                            }
                            else
                            {
                                var discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.IndianRed, title: "Sorry!", description: "Trigger not found");
                                await ctx.RespondAsync(discordEmbed);
                            }
                        }
                        else
                        {
                            var discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.IndianRed, title: "Sorry!", description: "Please mention a trigger to remove");
                            await ctx.RespondAsync(discordEmbed);
                        }
                    }
                    else if (args == null)
                    {
                        var discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.IndianRed, title: "Sorry!", description: "Please provide arguments");
                        await ctx.RespondAsync(discordEmbed);
                    }
                    else
                    {
                        var discordEmbed = Discord.CreateFancyMessage(color: DiscordColor.IndianRed, title: "Sorry!", description: "Invalid argument. either use:\nresponse add, response remove, response list or response edit");
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
            discordEmbed = Discord.CreateFancyMessage(title: "Adding a new reaction!", description: "Please mention the role to assign", color: DiscordColor.CornflowerBlue);
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

        public async Task<ulong?> GetMessageIDFromLinkAsync(CommandContext ctx)
        {
            DiscordEmbed discordEmbed;
            discordEmbed = Discord.CreateFancyMessage(title: "Adding a new reaction!", description: "Please enter the message link for the reaction", color: DiscordColor.CornflowerBlue);
            await ctx.RespondAsync(discordEmbed);
            var body = await ctx.Message.GetNextMessageAsync(new TimeSpan(0, 1, 0));

            if (!body.TimedOut)
            {
                var id = GetMessageIDFromLink(body.Result.Content);
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
        public async Task<DiscordMessage> GetMessageFromIDAsync(CommandContext ctx, ulong messageID)
        {
            DiscordMessage message = null;
            var channels = ctx.Guild.Channels.Select(c => c.Value).Where(c => c.IsCategory != true && c.Type != DSharpPlus.ChannelType.Voice).ToList();
            //.ForEach(async c =>
            //{
            //    try
            //    {
            //        message = await c.GetMessageAsync(messageID);
            //    }
            //    catch
            //    {

            //    }
            //});

            foreach (var channel in channels)
            {
                try
                {
                    message = await channel.GetMessageAsync(messageID);
                    break;
                }
                catch
                {

                }
            }
             
            if (message != null)
            {
                return message;
            }
            else
            {
                return null;
            }

        }

        public ulong? GetMessageIDFromLink(string message)
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
