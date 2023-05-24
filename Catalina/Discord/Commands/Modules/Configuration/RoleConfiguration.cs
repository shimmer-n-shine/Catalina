﻿using Catalina.Common;
using Catalina.Database;
using Catalina.Database.Models;
using Catalina.Discord.Commands.Autocomplete;
using Catalina.Discord.Commands.SelectMenuBuilders;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using NodaTime.TimeZones;
using Serilog.Core;
using DiscordNET = Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Catalina.Discord.Commands.Modules.CoreModule;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data;

namespace Catalina.Discord.Commands.Modules;
public partial class ConfigurationModule : InteractionModuleBase
{
    [Group("role", "Guild role configuration")]

    public class RoleConfiguration : InteractionModuleBase
    {
        private static Dictionary<Guid, RoleData> Data = new Dictionary<Guid, RoleData>();

        private static DiscordNET.Emoji CogEmoji = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":cog:").Raw);
        private static DiscordNET.Emoji DisabledEmoji = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":x:").Raw);
        private static DiscordNET.Emoji EnabledEmoji = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":white_check_mark:").Raw);
        private static DiscordNET.Emoji ResetEmoji = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":arrows_counterclockwise:").Raw);

        public struct RoleData
        {
            public Role DBRole { get; set; }
            public IRole Role { get; set; }
        }

        public struct ButtonData
        {
            public ButtonData() { }
            public bool IsTimezoneButton { get; set; } = false;
            public string Label { get; set; }
            public ButtonStyle ButtonStyle { get; set; } = ButtonStyle.Secondary;
            public IEmote Emote { get; set; }
        } 

        public DatabaseContext Database { get; set; }

        [SlashCommand("properties", "view/set role properties")]
        public async Task GetSetRoleProperties()
        {
            var guid = Guid.NewGuid();

            ComponentBuilder componentBuilder = new(); ;
            try
            {
                componentBuilder = new ComponentBuilder()
                .WithSelectMenu(new ColourMenu { ID = $"config_role_menu:{guid}" }.ToSelectMenuBuilder());
            }
            catch (Exception ex)
            {
                await RespondAsync(embed: new Utils.ErrorMessage(user: Context.User) { Exception = ex }, ephemeral: true);
                return;
            }

            Data.Add(guid, new RoleData()
            {
                DBRole = null,
                Role = null,
            });

            await RespondAsync(text: "Please select a role to modify.", components: componentBuilder.Build(), ephemeral: true);

            await Task.Delay(TimeSpan.FromMinutes(5));
            Data.Remove(guid);
        }

        public async Task UpdateButtons(IInteractionContext Context, Guid guid)
        {
            var roleData = Data[guid];
            var message = (Context.Interaction as IComponentInteraction).Message;

            var originalActionRows = ComponentBuilder.FromComponents(message.Components).ActionRows;
            ComponentBuilder filteredActionRows = new ComponentBuilder()
            {
                ActionRows = originalActionRows
                .Where(ar => ar.Components
                .Any(c => c.Type == ComponentType.SelectMenu))
                .ToList()
            };

            var buttonRow = new ActionRowBuilder();

            ButtonData renameableButtonData = new ButtonData();
            ButtonData colourableButtonData = new ButtonData();
            ButtonData timezoneButtonData = new ButtonData();
            ButtonData automationButtonData = new ButtonData();
            timezoneButtonData.IsTimezoneButton = true;

            // automatic button
            renameableButtonData.ButtonStyle = ButtonStyle.Secondary;
            renameableButtonData.Label = "Is Renameable: Not Set";

            colourableButtonData.ButtonStyle = ButtonStyle.Secondary;
            colourableButtonData.Label = "Is Colourable: Not Set";

            timezoneButtonData.Label = "Timezone: Not Set";
            timezoneButtonData.ButtonStyle = ButtonStyle.Link;
            timezoneButtonData.Emote = CogEmoji;

            automationButtonData.Label = "Automation: Not Set";
            automationButtonData.ButtonStyle = ButtonStyle.Link;
            automationButtonData.Emote = CogEmoji;

            if (roleData.DBRole is not null)
            {
                if (roleData.DBRole.IsAutomaticallyAdded)
                {
                    automationButtonData.Label = "Automation: Configure";
                }

                if (!string.IsNullOrEmpty(roleData.DBRole.Timezone))
                {
                    timezoneButtonData.Label = $"Timezone: {roleData.DBRole.Timezone.Replace("/", " - ").Replace('_', ' ')}";
                }

                if (roleData.DBRole.IsRenamabale)
                {
                    renameableButtonData.Label = "Can Change Name: Yes";
                    renameableButtonData.ButtonStyle = ButtonStyle.Success;
                    renameableButtonData.Emote = EnabledEmoji;
                }
                else
                {
                    renameableButtonData.Label = "Can Change Name: No";
                    renameableButtonData.ButtonStyle = ButtonStyle.Danger;
                    renameableButtonData.Emote = DisabledEmoji;
                }

                if (roleData.DBRole.IsColourable)
                {
                    colourableButtonData.Label = "Can Change Colour: Yes";
                    colourableButtonData.ButtonStyle = ButtonStyle.Success;
                    colourableButtonData.Emote = EnabledEmoji;
                }
                else
                {
                    colourableButtonData.Label = "Can Change Colour: No";
                    colourableButtonData.ButtonStyle = ButtonStyle.Danger;
                    colourableButtonData.Emote = DisabledEmoji;
                }
            }

            //is renameable
            buttonRow.WithButton(label: renameableButtonData.Label, customId: $"config_role_rename:{guid}", renameableButtonData.ButtonStyle, emote: renameableButtonData.Emote);

            //is colourable
            buttonRow.WithButton(label: colourableButtonData.Label, customId: $"config_role_colour:{guid}", colourableButtonData.ButtonStyle, emote: colourableButtonData.Emote);

            //timezone button
            buttonRow.WithButton(label: timezoneButtonData.Label, customId: $"config_role_timezone:{guid}", style: timezoneButtonData.ButtonStyle, emote: timezoneButtonData.Emote);

            //automation button
            buttonRow.WithButton(label: automationButtonData.Label, customId: $"config_role_automation:{guid}", style: automationButtonData.ButtonStyle, emote: automationButtonData.Emote);

            //reset button
            buttonRow.WithButton(label: "Reset", customId: $"config_role_reset:{guid}", style: ButtonStyle.Primary, emote: ResetEmoji);

            filteredActionRows.AddRow(buttonRow);

            await message.ModifyAsync(msg =>
            {
                msg.Components = filteredActionRows.Build();
            });
        }

        [ComponentInteraction("role_menu:*", true)]
        public async Task RoleResponse(string id, string[] options)
        {
            await DeferAsync();
            if (options is null || options.Length < 1 || !ulong.TryParse(options.First(), out ulong roleId)) return;

            var role = Context.Guild.GetRole(roleId);
            var dbRole = Database.Roles.FirstOrDefault(r => r.ID == roleId);
            var guid = Guid.Parse(id);
            var message = (Context.Interaction as IComponentInteraction).Message;

            Data[guid] = new RoleData
            {
                Role = role,
                DBRole = dbRole,
            };

            await UpdateButtons(Context, guid);
        }
    }
}


    //properties
    // make roles colourable
    // make roles renameable
    // set role timezones


    //automation 
    // make roles auto assigned
    // retroactively add/remove roles
    // make roles depend on other roles to be assigned
    // make roles get removed with others