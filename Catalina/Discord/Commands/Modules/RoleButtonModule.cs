﻿using Catalina.Database;
using Catalina.Discord.Commands.Autocomplete;
using Catalina.Discord.Commands.Preconditions;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Modules;
public class RoleButtonModule : InteractionModuleBase
{
    [ComponentInteraction("addRole:*", true)]
    public async Task AddRole(ulong roleId)
    {
        var role = Context.Guild.GetRole(roleId);
        if (role is null)
        {
            var message = (Context.Interaction as IComponentInteraction).Message;
            try
            {
                var messageComponents = ComponentBuilder.FromComponents((IReadOnlyCollection<IMessageComponent>)message.Components
                    .Where(c => !c.CustomId.Contains(roleId.ToString())));

                await message.ModifyAsync(msg => msg.Components = messageComponents.Build());
                await Context.Interaction.DeferAsync();
                await FollowupAsync(embed: new Utils.ErrorMessage(user: Context.User) { Exception = new Common.Exceptions.InvalidRole("Button was removed due to the role being invalid, please let the server administrator(s) know that this happened") }, ephemeral: true);
            }
            catch
            {
                await Context.Interaction.DeferAsync();
                await RespondAsync(embed: new Utils.ErrorMessage (user: Context.User) { Exception = new Common.Exceptions.UnknownError("Button was unable to be removed, and the role being assigned is inalid, please let the server administrator(s) know that this happened") }, ephemeral: true);
                NLog.LogManager.GetCurrentClassLogger().Error($"Could not remove button from message {message.Id} in guild {Context.Guild.Name} ({Context.Guild.Id})");
                return;
            }

        }


        if ((Context.User as IGuildUser).RoleIds.Contains(roleId))
        {
            try
            {
                await Context.Interaction.DeferAsync();
                await (Context.User as IGuildUser).RemoveRoleAsync(roleId);
                await FollowupAsync(embed: new Utils.AcknowledgementMessage(user: Context.User) { Color = CatalinaColours.Red, Body = $"Successfully removed {role.Mention} from you!" }, ephemeral: true, allowedMentions: AllowedMentions.None);
            }
            catch
            {
                await Context.Interaction.DeferAsync();
                await RespondAsync(embed: new Utils.ErrorMessage(user: Context.User) { Exception = new Common.Exceptions.UnknownError($"Could not remove {role.Mention} from you, please let the server administrator(s) know that this happened.") }, ephemeral: true, allowedMentions: AllowedMentions.None);
                NLog.LogManager.GetCurrentClassLogger().Error($"Could not remove role {Context.Guild.GetRole(roleId)} from {Context.User.Username}#{Context.User.Discriminator}");
                throw new Exception($"Could not remove role {Context.Guild.GetRole(roleId)} from {Context.User.Username}#{Context.User.Discriminator}");
            }
        }
        else
        {
            try
            {
                await Context.Interaction.DeferAsync();
                await (Context.User as IGuildUser).AddRoleAsync(roleId);
                await FollowupAsync(embed: new Utils.AcknowledgementMessage(user: Context.User) { Color = CatalinaColours.Green, Body = $"Successfully added {role.Mention} to you!" }, ephemeral: true, allowedMentions: AllowedMentions.None);
            }
            catch
            {
                await Context.Interaction.DeferAsync();
                await RespondAsync(embed: new Utils.ErrorMessage(user: Context.User) { Exception = new Common.Exceptions.UnknownError($"Could not remove {role.Mention} from you, please let the server administrator(s) know that this happened") }, ephemeral: true, allowedMentions: AllowedMentions.None);
                NLog.LogManager.GetCurrentClassLogger().Error($"Could not remove role {Context.Guild.GetRole(roleId)} from {Context.User.Username}#{Context.User.Discriminator}");
                throw new Exception($"Could not remove role {Context.Guild.GetRole(roleId)} from {Context.User.Username}#{Context.User.Discriminator}");
            }
        }
    }

    [RequirePrivilege(AccessLevel.Administrator)]
    [Group("reaction", "Guild reactions")]
    public class ConfigurationModule : InteractionModuleBase
    {
        [SlashCommand("add", "add a reaction button")]
        public async Task AddRoleButtonToMessageAsync(
            [Summary("MessageLink")] string messageLink,
            [Summary("Role")] [Autocomplete(typeof(AssignableRoles))] string roleID,
            [Summary("Emoji")] string emoji)
        {
            EmbedBuilder embed;
            try
            {
                ulong.Parse(roleID);
            }
            catch
            {
                embed = new Utils.ErrorMessage (user: Context.User) { Exception = new Common.Exceptions.InvalidParameter("The role you provided is invalid") };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }
            var role = Context.Guild.GetRole(ulong.Parse(roleID));

            if ( role is null) {
                embed = new Utils.ErrorMessage (user: Context.User) { Exception = new Common.Exceptions.InvalidRole("The role you provided could not be found") };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            } 

            if (!await Utils.VerifyRoleForUser(Context, ulong.Parse(roleID))) {
                embed = new Utils.ErrorMessage (user: Context.User) { Exception = new Common.Exceptions.InsufficientPermissions() };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            using var database = new DatabaseContextFactory().CreateDbContext();
            
            var message = await Utils.GetMessageFromLink(Context, messageLink);

            if (message == null)
            {
                embed = new Utils.ErrorMessage (user: Context.User) { Exception = new Common.Exceptions.InvalidMessageLink("Could not process message link, is it from the current guild?") };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            
            Database.Models.Emoji catalinaEmoji;
            IEmote emote;
            try
            {
                catalinaEmoji = await Database.Models.Emoji.ParseAsync(emoji, Context.Guild);
                emote = await Database.Models.Emoji.ToEmoteAsync(catalinaEmoji, Context.Guild);
            }
            catch (Exception exception)
            {
                await RespondAsync(embed: new Utils.ErrorMessage (user: Context.User) {Exception = exception });
                return;
            }

            try
            {
                var messageComponents = ComponentBuilder.FromComponents(message.Components);
                messageComponents = messageComponents.WithButton(emote: emote, label: role.Name, customId: ComponentConstants.AddRoleButton.Replace("*", role.Id.ToString()), style: ButtonStyle.Secondary);
                await (message as IUserMessage).ModifyAsync(msg => msg.Components = messageComponents.Build());

                await RespondAsync(embed: new Utils.AcknowledgementMessage (user: Context.User), ephemeral: true);
            }
            catch
            {
                embed = new Utils.ErrorMessage (user: Context.User) { Exception = new Common.Exceptions.InvalidMessageLink("Could not add button, is the message linked from Catalina?") };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            await RespondAsync(embed: new Utils.AcknowledgementMessage (user: Context.User));
        }
        [SlashCommand("remove", "remove a reaction button")]
        public async Task RemoveRoleButtonFromMessageAsync(
            [Summary("MessageLink")] string messageLink, 
            [Summary("Role")] [Autocomplete(typeof(AssignableRoles))] string roleID,
            [Summary("Emoji")] string emoji)
        {
            EmbedBuilder embed;
            try
            {
                ulong.Parse(roleID);
            }
            catch
            {
                embed = new Utils.ErrorMessage (user: Context.User) { Exception = new Common.Exceptions.InvalidParameter("The role you provided is invalid") };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }
            var role = Context.Guild.GetRole(ulong.Parse(roleID));

            if (role is null)
            {
                embed = new Utils.ErrorMessage (user: Context.User) { Exception = new Common.Exceptions.InvalidRole("The role you provided could not be found") };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            if (!await Utils.VerifyRoleForUser(Context, ulong.Parse(roleID)))
            {
                embed = new Utils.ErrorMessage (user: Context.User) { Exception = new Common.Exceptions.InsufficientPermissions() };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            using var database = new DatabaseContextFactory().CreateDbContext();

            var message = await Utils.GetMessageFromLink(Context, messageLink);

            if (message == null)
            {
                embed = new Utils.ErrorMessage (user: Context.User) { Exception = new Common.Exceptions.InvalidMessageLink("Could not process message link, is it from the current guild?") };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }


            Database.Models.Emoji catalinaEmoji;
            IEmote emote;
            try
            {
                catalinaEmoji = await Database.Models.Emoji.ParseAsync(emoji, Context.Guild);
                emote = await Database.Models.Emoji.ToEmoteAsync(catalinaEmoji, Context.Guild);
            }
            catch (Exception exception)
            {
                await RespondAsync(embed: new Utils.ErrorMessage (user: Context.User) { Exception = exception });
                return;
            }

            try
            {
                var messageComponents = ComponentBuilder.FromComponents(message.Components).ActionRows.SelectMany(r => r.Components)
                    .Where(c => !(c.CustomId.Contains(role.Id.ToString()) || (c.Type == ComponentType.Button && (c as ButtonComponent).Emote == emote))).ToList();
                
                await (message as IUserMessage).ModifyAsync(msg => msg.Components = ComponentBuilder.FromComponents(messageComponents).Build());

                await RespondAsync(embed: new Utils.AcknowledgementMessage (user: Context.User), ephemeral: true);
            }
            catch (Exception e)
            {
                embed = new Utils.ErrorMessage (user: Context.User) { Exception = e };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            await RespondAsync(embed: new Utils.AcknowledgementMessage (user: Context.User));
        }
        [RequirePrivilege(AccessLevel.Administrator)]
        [SlashCommand("removeall", "remove a reaction button")]
        public async Task RemoveAllRoleButtonsFromMessageAsync(
            [Summary("MessageLink")] string messageLink)
        {
            EmbedBuilder embed;

            using var database = new DatabaseContextFactory().CreateDbContext();

            var message = await Utils.GetMessageFromLink(Context, messageLink);

            if (message == null)
            {
                embed = new Utils.ErrorMessage(user: Context.User) { Exception = new Common.Exceptions.InvalidMessageLink("Could not process message link, is it from the current guild?") };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }
            try
            {  
                await (message as IUserMessage).ModifyAsync(msg => msg.Components = new Optional<MessageComponent>());
                await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User), ephemeral: true);
            }
            catch (Exception e)
            {
                embed = new Utils.ErrorMessage(user: Context.User) { Exception = e };
                await RespondAsync(embed: embed.Build(), ephemeral: true);
                return;
            }

            await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
        }
    }
}