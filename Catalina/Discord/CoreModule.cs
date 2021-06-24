using Catalina.Database;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Catalina.Database.Models;

namespace Catalina.Discord
{
    class CoreModule : BaseCommandModule
    {

        [Command("Setbasicrole")]
        [Description("Set the basic role for your server! This is an admin exclusive command.")]
        [Aliases("BasicRole", "basic")]
        public async Task SetBasicRole(CommandContext ctx, string mention)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();

            DiscordEmbed discordEmbed;
            var verification = await IsVerifiedAsync(ctx, true);
            if (verification == PermissionCode.Qualify)
            {
                if (mention != null)
                {
                    var role = Discord.GetRoleFromList(ctx.Message.MentionedRoles.ToList(), ctx);

                    if (database.GuildProperties.AsQueryable().Any(g => g.ID == ctx.Guild.Id && g.DefaultRole != null))  //(ConfigValues.BasicRoleGuildID.ContainsKey(ctx.Guild.Id))
                    {
                        var roleID = database.GuildProperties.AsNoTracking().Where(g => g.ID == ctx.Guild.Id).Select(g => g.DefaultRole.Value).FirstOrDefault();
                        var defaultRole = ctx.Guild.GetRole(roleID);
                        foreach (var member in ctx.Guild.Members)
                        {
                            try
                            {
                                if (member.Value.Roles.Select(r => r.Id).Contains(roleID))
                                {
                                    await member.Value.RevokeRoleAsync(defaultRole, "Automatic revokal of default role"); ;
                                }
                            }
                            catch { }
                        }
                        
                    }
                    database.GuildProperties.AsQueryable().First(g => g.ID == ctx.Guild.Id).DefaultRole = role.Id;  //ConfigValues.BasicRoleGuildID.Add(ctx.Guild.Id, role);
                    await database.SaveChangesAsync();

                    foreach (var member in ctx.Guild.Members)
                    {
                        try
                        {
                            await member.Value.GrantRoleAsync(role, "Automatic assignment of the basic role");
                        }
                        catch { }
                    }

                    discordEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Done!",
                        Description = "Set the basic role for your server!",
                        Color = role.Color
                    }.Build();
                        //Discord.CreateFancyMessage(title: "Done!", description: "Set the basic role for your server!", color: role.Color);
                    await ctx.RespondAsync(discordEmbed);
                }
                else
                {
                    discordEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Sorry!",
                        Description = "The role you provided was invalid!",
                        Color = DiscordColor.Red
                    }.Build();
                        //Discord.CreateFancyMessage(title: "Sorry!", description: "The role you provided was invalid!", color: DiscordColor.Red);
                    await ctx.RespondAsync(discordEmbed);
                }
            }
        }

        [Command("React")]
        [Description("Add a role when a user reacts to a message")]
        [Aliases("Reaction")]
        public async Task AddReaction(CommandContext ctx, string arg = null, string messageLink = null, string emote = null, string mention = null)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();

            //Reaction reaction;
            DiscordEmbed discordEmbed;
            var verification = await IsVerifiedAsync(ctx, true);
            if (verification == PermissionCode.Qualify)
            {
                
                if (arg != null && arg.ToLower() == "add")
                {
                    if (!ctx.Guild.CurrentMember.Roles.Any(r => r.Permissions.HasFlag(DSharpPlus.Permissions.ManageRoles) || r.Permissions.HasFlag(DSharpPlus.Permissions.Administrator)))
                    {
                        discordEmbed = new DiscordEmbedBuilder
                        {
                            Title = "Sorry!",
                            Description = "I don't have permissions to manage roles.",
                            Color = DiscordColor.Red
                        }.Build();
                            //CreateFancyMessage(title: "Sorry!", description: "I don't have permissions to manage roles.", color: DiscordColor.Red);
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
                            discordEmbed = discordEmbed = new DiscordEmbedBuilder
                            {
                                Title = "Sorry!",
                                Description = "You provided an invalid message link.",
                                Color = DiscordColor.Red
                            }.Build();
                            await ctx.RespondAsync(discordEmbed);
                            return;
                        }
                        
                        if (role != null && ctx.Guild.CurrentMember.Roles.All(r => r.Position < role.Position))
                        {
                            discordEmbed = new DiscordEmbedBuilder
                            {
                                Title = "Sorry!",
                                Description = "I don't have permissions to manage roles.",
                                Color = DiscordColor.Red
                            }.Build();
                            await ctx.RespondAsync(discordEmbed);
                            return;
                        } 


                        if (message != null && emoji != null && role != null)
                        {
                            discordEmbed = new DiscordEmbedBuilder
                            {
                                Title = "Done!",
                                Description = "Added to internal list of reactions",
                                Color = role.Color
                            }.Build();
                            await ctx.RespondAsync(discordEmbed);
                            var reaction = new Reaction
                            {
                                MessageID = message.Id,
                                EmojiName = emoji.Name,
                                RoleID = role.Id,
                                ChannelID = message.Channel.Id,
                                GuildID = ctx.Guild.Id
                            };
                            
                            //(message.Id, emoji.Name, role.Id, message.Channel.Id);

                                //var reactions = ConfigValues.Reactions.GetValueOrDefault(ctx.Guild.Id);
                                //reactions.Add(reaction);

                            database.Reactions.Add(reaction);

                            await database.SaveChangesAsync();
                            await message.CreateReactionAsync(emoji);

                        }
                        else
                        {
                            discordEmbed = new DiscordEmbedBuilder
                            {
                                Title = "Sorry!",
                                Description = "The message link, role or reaction you provided was invalid!",
                                Color = DiscordColor.Red
                            }.Build();
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
                                        discordEmbed = new DiscordEmbedBuilder
                                        {
                                            Title = "Sorry!",
                                            Description = "I don't have permissions to manage roles.",
                                            Color = DiscordColor.Red
                                        }.Build();
                                        await ctx.RespondAsync(discordEmbed);
                                        return;
                                    }

                                    discordEmbed = new DiscordEmbedBuilder
                                    {
                                        Title = "Done!",
                                        Description = "Added to internal list of reactions",
                                        Color = role.Color
                                    }.Build();
                                    await ctx.RespondAsync(discordEmbed);

                                    var reaction = new Reaction
                                    {
                                        MessageID = message.Id,
                                        EmojiName = emoji.Name,
                                        RoleID = role.Id,
                                        ChannelID = message.Channel.Id,
                                        GuildID = ctx.Guild.Id
                                    };
                                    await message.CreateReactionAsync(emoji);

                                    database.Reactions.Add(reaction);

                                    await database.SaveChangesAsync();
                                    await message.CreateReactionAsync(emoji);
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
                        discordEmbed = new DiscordEmbedBuilder
                        {
                            Title = "Done!",
                            Description = "Removed reaction from internal list of reactions!",
                            Color = DiscordColor.SpringGreen,
                        }.Build();
                        var emoji = Discord.GetEmojiFromString(messageLink);
                        var message = await Discord.GetMessageFromLinkAsync(ctx, messageLink);
                        if (emoji != null)
                        {
                            var reactions = database.Reactions.AsNoTracking().Where(r => r.GuildID == ctx.Guild.Id);
                            if (reactions.Count() > 0)  //(ConfigValues.Reactions.ContainsKey(ctx.Guild.Id)) //ConfigValues.Reactions.Select(reaction => reaction.Value.emoji).Contains(emoji) && )
                            {
                                //var reactions = ConfigValues.Reactions[ctx.Guild.Id];
                                List<Reaction> removedItems = new List<Reaction>();
                                reactions.Where(reaction => reaction.EmojiName == emoji.Name).ToList().ForEach( async reaction =>
                                {
                                    removedItems.Add(reaction);

                                    try
                                    {
                                        message = await ctx.Guild.GetChannel(reaction.ChannelID).GetMessageAsync(reaction.MessageID);
                                        try
                                        {
                                            await message.DeleteReactionsEmojiAsync(emoji);
                                        }
                                        catch (UnauthorizedException)
                                        {
                                            discordEmbed = new DiscordEmbedBuilder(discordEmbed)
                                            {
                                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                                {
                                                    Text = "but could not remove reactions due to insufficient permissions."
                                                }
                                            }.Build();
                                        }
                                        catch { }
                                    }
                                    catch
                                    {
                                        discordEmbed = new DiscordEmbedBuilder(discordEmbed)
                                        {
                                            Footer = new DiscordEmbedBuilder.EmbedFooter
                                            {
                                                Text = "but could not remove reactions due to insufficient permissions."
                                            }
                                        }.Build();
                                    }

                                });

                                removedItems.ForEach(item => database.Reactions.Remove(item));

                                await database.SaveChangesAsync();
                                await ctx.RespondAsync(discordEmbed);
                            }
                            else
                            {
                                discordEmbed = new DiscordEmbedBuilder
                                {
                                    Title = "Sorry!",
                                    Description = "You've not created any reactions to remove.",
                                    Color = DiscordColor.Red
                                }.Build();
                            }
                        }
                        else if (message != null)
                        {
                            var reactions = database.Reactions.Where(r => r.GuildID == ctx.Guild.Id);
                            if (reactions.Count() > 0)
                            {
                                discordEmbed = new DiscordEmbedBuilder
                                {
                                    Title = "Done!",
                                    Description = "Removed reaction from internal list of reactions!",
                                    Color = DiscordColor.SpringGreen,
                                }.Build();
                                bool reactionsRemoved = true;
                                List<Reaction> removedItems = new List<Reaction>();
                                reactions.Where(reaction => reaction.MessageID == message.Id).ToList().ForEach(async reaction =>
                                {
                                    removedItems.Add(reaction);
                                });


                                removedItems.ForEach(item => database.Reactions.Remove(item));

                                await database.SaveChangesAsync();
                                await ctx.RespondAsync(discordEmbed);

                                try
                                {
                                    await message.DeleteAllReactionsAsync();
                                }
                                catch (UnauthorizedException)
                                {
                                    discordEmbed = new DiscordEmbedBuilder(discordEmbed)
                                    {
                                        Footer = new DiscordEmbedBuilder.EmbedFooter
                                        {
                                            Text = "but could not clear reactions due to insufficient permissions."
                                        }
                                    }.Build();
                                }
                                catch { }
                                if (!reactionsRemoved) discordEmbed = new DiscordEmbedBuilder
                                {
                                    Title = "Done!",
                                    Description = "Removed reaction from internal list of reactions!",
                                    Color = DiscordColor.Yellow,
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = "but could not clear reactions."
                                    }
                                }.Build();

                                await ctx.RespondAsync(discordEmbed);
                            }
                            else
                            {
                                discordEmbed = new DiscordEmbedBuilder
                                {
                                    Title = "Sorry!",
                                    Description = "You've not created any reactions to remove.",
                                    Color = DiscordColor.Red
                                }.Build();
                            }

                        }
                        else
                        {
                            discordEmbed = new DiscordEmbedBuilder
                            {
                                Title = "Sorry!",
                                Description = "The message link or reaction you provided is invalid!",
                                Color = DiscordColor.Red
                            }.Build();
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
                                var reactions = database.Reactions.Where(r => r.GuildID == ctx.Guild.Id);
                                if (reactions.Count() > 0)
                                {
                                    if (reactions.Where(r => r.MessageID == message.Id).Count() > 0 && reactions.Where(r => r.EmojiName == emoji.Name).Count() > 0)
                                    {
                                        var reactionsToDelete = reactions.Where(r => r.MessageID == message.Id && r.EmojiName == emoji.Name).ToList(); //ConfigValues.Reactions[ctx.Guild.Id].Find(r => r.messageID == message.Id && r.emojiName == emoji.Name);
                                        try
                                        {
                                            await message.DeleteReactionsEmojiAsync(Discord.GetEmojiFromString(emoji.Name));
                                        }
                                        catch { }

                                        reactionsToDelete.ForEach(r => database.Reactions.Remove(r));
                                        await database.SaveChangesAsync();

                                        discordEmbed = new DiscordEmbedBuilder
                                        {
                                            Title = "Done!",
                                            Description = "Removed from internal list of reactions",
                                            Color = DiscordColor.SpringGreen
                                        }.Build();
                                        await ctx.RespondAsync(discordEmbed);
                                    }
                                }
                            }
                        }
                            
                    }
                    
                }
                else
                {
                    discordEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Sorry!",
                        Description = "You didn't provide an argument!\ntry `add` or `remove`.",
                        Color = DiscordColor.Red
                    }.Build();
                    await ctx.RespondAsync(discordEmbed);
                }
            }
        }

        public static async Task<PermissionCode> IsVerifiedAsync(CommandContext ctx, bool isAdminCommand = false, bool isAvailableEverywhere = false)
        {
            using var database = new DatabaseContextFactory().CreateDbContext();

            DiscordEmbed discordEmbed;
            if (ctx.Member.IsOwner || ctx.Member.Roles.Any(t => t.Permissions == DSharpPlus.Permissions.Administrator))
            {
                return PermissionCode.Qualify;
            }
            if (!Discord.commandChannels.Contains(ctx.Channel) && !isAvailableEverywhere)
            {
                discordEmbed = new DiscordEmbedBuilder
                {
                    Title = "Sorry!",
                    Description = "You're not allowed to run that command here.",
                    Color = DiscordColor.Red
                }.Build();
                await ctx.RespondAsync(discordEmbed);
                return PermissionCode.UnqualifyChannel;
            }
            if (ctx.Member.Id == Convert.ToUInt64(Environment.GetEnvironmentVariable(AppProperties.DeveloperID)))
            {
                return PermissionCode.Qualify;
            }
            var adminVerf = ctx.Member.Roles.Select(t => t.Id).Intersect(database.GuildProperties.AsNoTracking().Where(g => g.ID == ctx.Guild.Id).Select(g => g.AdminRoleIDs).First());


            if (adminVerf.Any())
            {
                return PermissionCode.Qualify;
            }
            if (ctx.Member.IsOwner)
            {
                return PermissionCode.Qualify;
            }
            discordEmbed = new DiscordEmbedBuilder
            {
                Title = "Sorry!",
                Description = "You have insufficient permissions to run that command.",
                Color = DiscordColor.Red
            }.Build();
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
