using Catalina.Common;
using Catalina.Database;
using Catalina.Database.Models;
using Catalina.Discord.Commands.Autocomplete;
using Catalina.Discord.Commands.SelectMenuBuilders;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using NodaTime.TimeZones;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordNET = Discord;

namespace Catalina.Discord.Commands.Modules;
public partial class ConfigurationModule : InteractionModuleBase
{
    [Group("role", "Guild role configuration")]

    public class RoleConfiguration : InteractionModuleBase
    {
        private static Dictionary<Guid, RoleData> Data = new Dictionary<Guid, RoleData>();

        private static DiscordNET.Emoji DisabledEmoji = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":x:").Raw);
        private static DiscordNET.Emoji EditedEmoji = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":pencil:").Raw);
        private static DiscordNET.Emoji UneditedEmoji = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":page_facing_up:").Raw);
        private static DiscordNET.Emoji EnabledEmoji = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":white_check_mark:").Raw);
        private static DiscordNET.Emoji WarningEmoji = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":warning:").Raw);
        private static DiscordNET.Emoji EditingEmoji = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":pencil2:").Raw);

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

        [DefaultMemberPermissions(PermissionConstants.Administrator)]
        [SlashCommand("properties", "view/set role properties")]
        public async Task GetSetRoleProperties()
        {
            var guid = Guid.NewGuid();

            ComponentBuilder componentBuilder = new(); ;
            try
            {
                componentBuilder = new ComponentBuilder()
                .WithSelectMenu(new RolesMenu(Database, Context.Guild, Context.User as IGuildUser) { ID = ComponentConstants.ConfigureRoleMenu.GetComponentWithID(guid.ToString()) }.ToSelectMenuBuilder());
            }
            catch (Exception ex)
            {
                await RespondAsync(embed: new Utils.ErrorMessage(user: Context.User, ex), ephemeral: true);
                return;
            }

            Data.Add(guid, new RoleData()
            {
                DBRole = null,
                Role = null,
            });

            try
            {
                await RespondAsync(components: componentBuilder.Build(), ephemeral: true);
            }
            catch (Exception e)
            {
                Data.Remove(guid);
            }


            await Task.Delay(TimeSpan.FromMinutes(5));
            Data.Remove(guid);
        }

        public ComponentBuilder GenerateOverviewButtons(IInteractionContext Context, Guid guid)
        {
            var dbGuild = Database.Guilds.First(g => g.ID == Context.Guild.Id);

            Data[guid] = new RoleData()
            {
                DBRole = dbGuild.Roles.FirstOrDefault(r => r.ID == Data[guid].Role.Id),
                Role = Data[guid].Role
            };

            if (Data[guid].DBRole is null)
            {
                dbGuild.Roles.Add(new Role
                {
                    ID = Data[guid].Role.Id,
                    Guild = dbGuild,
                });
                Database.SaveChanges();

                Data[guid] = new RoleData()
                {
                    DBRole = Database.Roles.FirstOrDefault(r => r.ID == Data[guid].Role.Id),
                    Role = Data[guid].Role
                };
            }

            var roleData = Data[guid];


            var message = (Context.Interaction as IComponentInteraction).Message;

            var buttonRows = new ComponentBuilder();

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
            timezoneButtonData.ButtonStyle = ButtonStyle.Secondary;
            timezoneButtonData.Emote = UneditedEmoji;

            automationButtonData.Label = "Auto-Add: Not Set";
            automationButtonData.ButtonStyle = ButtonStyle.Secondary;
            automationButtonData.Emote = UneditedEmoji;

            if (roleData.DBRole.IsAutomaticallyAdded)
            {
                automationButtonData.Label = "Auto-Add: Yes";
                automationButtonData.ButtonStyle = ButtonStyle.Success;
                automationButtonData.Emote = EnabledEmoji;
            }
            else
            {
                automationButtonData.Label = "Auto-Add: No";
                automationButtonData.ButtonStyle = ButtonStyle.Secondary;
                automationButtonData.Emote = DisabledEmoji;
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
                renameableButtonData.ButtonStyle = ButtonStyle.Secondary;
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
                colourableButtonData.ButtonStyle = ButtonStyle.Secondary;
                colourableButtonData.Emote = DisabledEmoji;
            }

            List<ActionRowBuilder> actionRows = new List<ActionRowBuilder>(4) { new(), new(), new(), new() };

            SelectMenuBuilder roleMenu = new SelectMenuBuilder(ComponentConstants.ConfigureRoleMenu.GetComponentWithID(guid.ToString()), isDisabled: true, options: new());

            var option = new SelectMenuOptionBuilder(
                label: $"{roleData.Role.Name}",
                value: roleData.Role.Id.ToString(),
                emote: EditingEmoji,
                isDefault: true
            );

            roleMenu.AddOption(option);

            actionRows[0].WithSelectMenu(roleMenu);

            //is renameable
            actionRows[1].WithButton(label: renameableButtonData.Label
                , customId: ComponentConstants.ConfigureRoleRenameable.GetComponentWithID(guid.ToString())
                , renameableButtonData.ButtonStyle
                , emote: renameableButtonData.Emote);

            //is colourable
            actionRows[1].WithButton(label: colourableButtonData.Label
                , customId: ComponentConstants.ConfigureRoleColourable.GetComponentWithID(guid.ToString())
                , colourableButtonData.ButtonStyle
                , emote: colourableButtonData.Emote);

            //timezone button
            actionRows[2].WithButton(label: timezoneButtonData.Label
                , customId: ComponentConstants.ConfigureRoleTimezone.GetComponentWithID(guid.ToString())
                , style: timezoneButtonData.ButtonStyle
                , emote: timezoneButtonData.Emote);

            //automation button
            actionRows[2].WithButton(label: automationButtonData.Label
                , ComponentConstants.ConfigureRoleAuto.GetComponentWithID(guid.ToString())
                , style: automationButtonData.ButtonStyle
                , emote: automationButtonData.Emote);

            //reset button
            actionRows[3].WithButton(label: "Reset to default", customId: ComponentConstants.ConfigureRoleReset
                .GetComponentWithID(guid.ToString()), style: ButtonStyle.Danger, emote: WarningEmoji);

            actionRows.ForEach(r => buttonRows.AddRow(r));

            return buttonRows;
        }

        [ComponentInteraction(ComponentConstants.ConfigureRoleColourable, true)]
        public async Task ConfigureRoleColourable(string id)
        {
            await DeferAsync();

            var guid = Guid.Parse(id);
            var message = (Context.Interaction as IComponentInteraction).Message;

            var roleData = Data[guid];
            var dbGuild = Database.Guilds.First(g => g.ID == Context.Guild.Id);

            if (!dbGuild.Roles.Any(r => r.ID == roleData.DBRole.ID))
            {
                dbGuild.Roles.Add(new Role
                {
                    Guild = dbGuild,
                    ID = roleData.DBRole.ID,
                    IsColourable = false,
                });
            }

            dbGuild.Roles.FirstOrDefault(r => r.ID == roleData.DBRole.ID).IsColourable ^= true;

            await Database.SaveChangesAsync();


            await ModifyOriginalResponseAsync(m =>
            {
                m.Components = GenerateOverviewButtons(Context, guid).Build();
            });
        }

        [ComponentInteraction(ComponentConstants.ConfigureRoleRenameable, true)]
        public async Task ConfigureRoleRenameable(string id)
        {
            await DeferAsync();

            var guid = Guid.Parse(id);
            var message = (Context.Interaction as IComponentInteraction).Message;

            var roleData = Data[guid];
            var dbGuild = Database.Guilds.First(g => g.ID == Context.Guild.Id);

            if (!dbGuild.Roles.Any(r => r.ID == roleData.DBRole.ID))
            {
                dbGuild.Roles.Add(new Role
                {
                    Guild = dbGuild,
                    ID = roleData.DBRole.ID,
                    IsRenamabale = false,
                });
            }

            dbGuild.Roles.FirstOrDefault(r => r.ID == roleData.DBRole.ID).IsRenamabale ^= true;

            await Database.SaveChangesAsync();



            await ModifyOriginalResponseAsync(m =>
            {
                m.Components = GenerateOverviewButtons(Context, guid).Build();
            });
        }

        [ComponentInteraction(ComponentConstants.ConfigureRoleAuto, true)]
        public async Task ConfigureroleAutoAdd(string id)
        {
            await DeferAsync();

            var guid = Guid.Parse(id);
            var message = (Context.Interaction as IComponentInteraction).Message;

            var roleData = Data[guid];
            var dbGuild = Database.Guilds.First(g => g.ID == Context.Guild.Id);

            if (!dbGuild.Roles.Any(r => r.ID == roleData.DBRole.ID))
            {
                dbGuild.Roles.Add(new Role
                {
                    Guild = dbGuild,
                    ID = roleData.DBRole.ID,
                    IsAutomaticallyAdded = false,
                });
            }

            var configured = dbGuild.Roles.FirstOrDefault(r => r.ID == roleData.DBRole.ID).IsAutomaticallyAdded;

            await Database.SaveChangesAsync();

            ButtonBuilder confirmRetroactiveButton = new ButtonBuilder(
                label: "Yes",
                emote: WarningEmoji,
                style: ButtonStyle.Danger,
                customId: ComponentConstants.ConfigureRoleRetroactiveConfirm.GetComponentWithID(id)
                );

            ButtonBuilder declineRetroactiveButton = new ButtonBuilder(
                label: "No",
                style: ButtonStyle.Secondary,
                customId: ComponentConstants.ConfigureRoleRetroactiveDecline.GetComponentWithID(id)
                );

            ComponentBuilder components = new ComponentBuilder();
            components.WithButton(confirmRetroactiveButton).WithButton(declineRetroactiveButton);


            await ModifyOriginalResponseAsync(m =>
            {
                m.Components = components.Build();
                m.Content = $"Should I {(configured ? "remove" : "add")} {roleData.Role.Mention} {(configured ? "from" : "to")} __all users__ retroactively?";
                m.AllowedMentions = AllowedMentions.None;
            });
        }

        [ComponentInteraction(ComponentConstants.ConfigureRoleRetroactiveDecline, true)]
        public async Task ConfigureRoleRetroactiveDecline(string id)
        {
            await DeferAsync();

            var guid = Guid.Parse(id);
            var message = (Context.Interaction as IComponentInteraction).Message;

            var roleData = Data[guid];
            var dbGuild = Database.Guilds.First(g => g.ID == Context.Guild.Id);

            dbGuild.Roles.FirstOrDefault(r => r.ID == roleData.DBRole.ID).IsAutomaticallyAdded ^= true;

            await Database.SaveChangesAsync();

            await ModifyOriginalResponseAsync(m =>
            {
                m.Components = GenerateOverviewButtons(Context, guid).Build();
                m.Content = string.Empty;
            });
        }

        [ComponentInteraction(ComponentConstants.ConfigureRoleRetroactiveConfirm, true)]
        public async Task ConfigureroleRetroactiveConfirm(string id)
        {
            await DeferAsync();

            var guid = Guid.Parse(id);
            var message = (Context.Interaction as IComponentInteraction).Message;

            var roleData = Data[guid];
            var dbGuild = Database.Guilds.First(g => g.ID == Context.Guild.Id);
            var roleID = roleData.Role.Id;

            dbGuild.Roles.FirstOrDefault(r => r.ID == roleID).IsAutomaticallyAdded ^= true;

            await Database.SaveChangesAsync();

            bool toAdd = dbGuild.Roles.FirstOrDefault(r => r.ID == roleID).IsAutomaticallyAdded;

            var users = await Context.Guild.GetUsersAsync();

            List<IGuildUser> usersToProcess = new();
            if (toAdd)
            {
                usersToProcess = users.Where(u => !u.RoleIds.Contains(roleData.Role.Id)).ToList();
            }
            if (!toAdd)
            {
                usersToProcess = users.Where(u => u.RoleIds.Contains(roleData.Role.Id)).ToList();
            }

            await ModifyOriginalResponseAsync(m =>
            {
                m.Components = new ComponentBuilder().Build();
                m.Content = $"Processing {usersToProcess.Count} users... estimated time: {TimeSpan.FromSeconds(2 * usersToProcess.Count):hh\\:mm\\:ss}";
                m.AllowedMentions = AllowedMentions.None;
            });

            List<IGuildUser> failedUsers = new List<IGuildUser>();
            if (toAdd)
            {
                foreach (var user in usersToProcess)
                {
                    try
                    {
                        await user.AddRoleAsync(roleID);
                        await Task.Delay(2000);
                    }
                    catch (Exception e)
                    {
                        failedUsers.Add(user);
                        await Task.Delay(2000);
                    }
                }
            }

            else
            {
                foreach (var user in usersToProcess)
                {
                    try
                    {
                        await user.RemoveRoleAsync(roleID);
                        await Task.Delay(2000);
                    }
                    catch (Exception e)
                    {
                        failedUsers.Add(user);
                        await Task.Delay(2000);
                    }
                }
            }

            StringBuilder userList = new StringBuilder();
            userList.Append($"Failed to process {roleData.Role.Mention} for {failedUsers.Count}/{usersToProcess.Count} users:\n");

            int length = userList.Length;
            foreach (var user in failedUsers)
            {
                var mention = user.Mention;
                //+3 because "..."
                if (length + mention.Length + 3 > 2000)
                {
                    userList.Append("...");
                    break;
                }
                else
                {
                    userList.Append(mention);
                    userList.Append(", ");
                }
            }

            userList.Append(1);

            await ModifyOriginalResponseAsync(m =>
            {
                m.Components = GenerateOverviewButtons(Context, guid).Build();
                m.Content = failedUsers.Count > 0 ? userList.ToString() : $"Processed {roleData.Role.Mention} for {usersToProcess.Count} users.";
                m.AllowedMentions = AllowedMentions.None;
            });
        }


        [ComponentInteraction(ComponentConstants.ConfigureRoleMenu, true)]
        public async Task ConfigureRoleMenu(string id, string[] options)
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

            await ModifyOriginalResponseAsync(m =>
            {
                m.Components = GenerateOverviewButtons(Context, guid).Build();
            });
        }

        [ComponentInteraction(ComponentConstants.ConfigureRoleTimezone, true)]
        public async Task ConfigureRoleTimezone(string id)
        {
            await DeferAsync();

            var guid = Guid.Parse(id);
            var message = (Context.Interaction as IComponentInteraction).Message;

            var roleData = Data[guid];

            var commands = await Discord.DiscordClient.GetGlobalApplicationCommandsAsync();
            var command = commands
                .FirstOrDefault(c => c.Name.ToLower() == "config");


            await ModifyOriginalResponseAsync(m =>
            {
                m.Components = GenerateOverviewButtons(Context, guid).Build();
                m.Content = $"Please use {(command is not null ? $"</config role timezone:{command.Id}>" : "`/config role timezone`")} to set this role's timezone";
            });
        }


        [ComponentInteraction(ComponentConstants.ConfigureRoleReset, true)]
        public async Task ConfigureRoleReset(string id)
        {
            await DeferAsync();

            var guid = Guid.Parse(id);
            var message = (Context.Interaction as IComponentInteraction).Message;

            var roleData = Data[guid];
            var dbGuild = Database.Guilds.First(g => g.ID == Context.Guild.Id);
            var roleToRemove = await Database.Roles.FirstOrDefaultAsync(r => r.ID == roleData.DBRole.ID);

            if (roleToRemove is not null)
            {
                Database.Roles.Remove(roleToRemove);
                await Database.SaveChangesAsync();

                Data[guid] = new RoleData()
                {
                    Role = roleData.Role,
                    DBRole = null
                };
            }

            await ModifyOriginalResponseAsync(m =>
            {
                m.Components = GenerateOverviewButtons(Context, guid).Build();
            });

        }

        [DefaultMemberPermissions(PermissionConstants.Administrator)]
        [SlashCommand("timezone", "Modify role timezone")]
        public async Task ConfigureRole(
        IRole role, [Autocomplete(typeof(TimezoneNames))] string timezone = null)
        {
            var guildProperties = Database.Guilds.Include(g => g.Roles).FirstOrDefault(g => g.ID == Context.Guild.Id);
            Role DBrole;
            DBrole = guildProperties.Roles.FirstOrDefault(r => r.ID == role.Id);

            if (DBrole == null)
            {
                DBrole = new Role { ID = role.Id };
                guildProperties.Roles.Add(DBrole);
                Database.SaveChanges();
            }

            if (!string.IsNullOrEmpty(timezone))
            {
                var tz = TzdbDateTimeZoneSource.Default.ZoneLocations.FirstOrDefault(t => t.ZoneId == timezone);
                if (tz is not null)
                {
                    DBrole.Timezone = timezone;
                    Database.SaveChanges();
                    Timezones.AddRole(DBrole, role);
                }
                else
                {
                    await RespondAsync(embed: new Utils.ErrorMessage(user: Context.User, exception: new Exceptions.InvalidArgumentException("timezone provided is invalid")));
                }
            }

            else
            {
                DBrole.Timezone = null;
                Database.SaveChanges();
                Timezones.RemoveRole(DBrole);
            }

            //if messages weren't already sent by make automatic, send acknowledgement message
            if (!Context.Interaction.HasResponded) await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
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