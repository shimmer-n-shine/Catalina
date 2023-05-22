using Catalina.Database;
using Catalina.Common.Commands.Autocomplete;
using Catalina.Common.Commands.Preconditions;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using NodaTime.TimeZones;
using Serilog;
using Serilog.Core;
using Skuld.Discord.InteractionHelpers.AutoCompleters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Common.Commands.Modules
{
    [RequirePrivilege(AccessLevel.Administrator)]
    [Group("config", "Guild configurations")]
    public class ConfigurationModule : InteractionModuleBase
    {
        [Group("starboard", "Starboard configuration")]
        public class StarboardConfiguration : InteractionModuleBase
        {
            public Logger Log { get; set; }
            public DatabaseContext Database { get; set; }
            [SlashCommand("channel", "Set starboard channel")]
            public async Task SetStarboardChannel(
                [Summary("Channel")][ChannelTypes(ChannelType.Text)] IChannel channel = null)
            {
                var guildProperty = Database.GuildProperties.FirstOrDefault(g => g.ID == Context.Guild.Id);

                guildProperty.Starboard.ChannelID = channel?.Id;

                await Database.SaveChangesAsync();
                await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
            }
            [SlashCommand("emoji", "Set starboard emoji")]
            public async Task SetStarboardEmoji(
                [Summary("Emoji")] string emoji)
            {
                var guildProperties = Database.GuildProperties.FirstOrDefault(g => g.ID == Context.Guild.Id);
                IEmote emote;
                Database.Models.Emoji catalinaEmoji;
                try
                {
                    catalinaEmoji = await Catalina.Database.Models.Emoji.ParseAsync(emoji, Context.Guild);
                    emote = await Catalina.Database.Models.Emoji.ToEmoteAsync(catalinaEmoji, Context.Guild);
                }
                catch (Exception exception)
                {
                    await RespondAsync(embed: new Utils.ErrorMessage(user: Context.User) { Exception = exception });
                    return;
                }

                guildProperties.Starboard.SetOrCreateEmoji(catalinaEmoji, Database);

                await Database.SaveChangesAsync();

                await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
            }
            [SlashCommand("threshold", "Set starboard threshhold")]
            public async Task SetStarboardThreshhold(
                [Summary("Threshhold")] int threshhold)
            {
                if (threshhold <= 0)
                {
                    await Context.Interaction.RespondAsync(embed: new Utils.ErrorMessage(user: Context.User) { Exception = new System.ArgumentException("Threshhold cannot be less than 1.") });
                    return;
                }
                var guildProperties = Database.GuildProperties.FirstOrDefault(g => g.ID == Context.Guild.Id);

                guildProperties.Starboard.Threshhold = threshhold;

                await Database.SaveChangesAsync();
                await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
            }
        }



        [Group("role", "Guild role configuration")]

        public class RoleConfiguration : InteractionModuleBase
        {
            public Logger Log { get; set; }
            public DatabaseContext Database { get; set; }
            [SlashCommand("modify", "Modify role configuration")]
            public async Task ConfigureRole(
                [Summary("Role")] IRole role,
                [ComplexParameter] RoleProperties roleConfig)
            {
                var guildProperties = Database.GuildProperties.Include(g => g.Roles).FirstOrDefault(g => g.ID == Context.Guild.Id);
                Database.Models.Role DBrole;
                DBrole = guildProperties.Roles.FirstOrDefault(r => r.ID == role.Id);
                var failures = 0;
                var rolesAssigned = false;
                string timezone = null;

                if (DBrole == null)
                {
                    DBrole = new Database.Models.Role { ID = role.Id };
                    guildProperties.Roles.Add(DBrole);
                    Database.SaveChanges();
                }

                if (roleConfig.isRenamable.HasValue) DBrole.IsRenamabale = roleConfig.isRenamable.Value;
                if (roleConfig.isColourable.HasValue) DBrole.IsColourable = roleConfig.isColourable.Value;
                if (roleConfig.timezone is not null)
                {
                   DBrole.Timezone = TzdbDateTimeZoneSource.Default.ZoneLocations.Any(t => t.ZoneId == roleConfig.timezone) ? roleConfig.timezone : null;
                }

                //process if automatic
                if (roleConfig.isAutomatic.HasValue)
                {
                    if (roleConfig.isAutomatic.Value && !DBrole.IsAutomaticallyAdded)
                    {
                        rolesAssigned = true;
                        await RespondAsync(embed: new Utils.InformationMessage(user: Context.User, body: $"Assigning {role.Mention} to all users"), allowedMentions: AllowedMentions.None);
                        foreach (var user in await Context.Guild.GetUsersAsync())
                        {
                            try
                            {
                                await user.AddRoleAsync(role);
                            }
                            catch
                            {
                                failures++;
                                Log.Error("Could not add automatic role to user");
                            }
                        }

                    }
                    else if (!roleConfig.isAutomatic.Value && DBrole.IsAutomaticallyAdded)
                    {
                        await RespondAsync(embed: new Utils.InformationMessage(user: Context.User, body: $"Removing {role.Name} from all users"), allowedMentions: AllowedMentions.None);
                        foreach (var user in await Context.Guild.GetUsersAsync())
                        {
                            try
                            {
                                await user.RemoveRoleAsync(role);
                            }
                            catch
                            {
                                failures++;
                                Log.Error("Could not remove automatic role from user");
                            }
                        }
                    }
                    DBrole.IsAutomaticallyAdded = roleConfig.isAutomatic.Value;
                }


                await Database.SaveChangesAsync();
                //if messages weren't already sent by make automatic, send acknowledgement message
                if (!Context.Interaction.HasResponded) await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
                //else update make automatic message to feedback role changes
                else
                {
                    if (failures > 0)
                    {
                        await ModifyOriginalResponseAsync(msg =>
                        {
                            msg.Embed = (Embed)(new Utils.AcknowledgementMessage(color: CatalinaColours.Red, body: $"But could not {(rolesAssigned ? "assign" : "remove")} {role.Mention} {(rolesAssigned ? "to" : "from")} {failures} user{(rolesAssigned ? "s" : "")}", user: Context.User));
                            msg.AllowedMentions = AllowedMentions.None;
                        });
                    }
                    else
                    {
                        await ModifyOriginalResponseAsync(msg =>
                        {
                            msg.Embed = (Embed)(new Utils.AcknowledgementMessage(color: CatalinaColours.Green, body: $"{(rolesAssigned ? "Assigned" : "Removed")} {role.Mention} {(rolesAssigned ? "to" : "from")} all users.", user: Context.User));
                            msg.AllowedMentions = AllowedMentions.None;
                        });
                    }
                }
            }

            [SlashCommand("list", "List guild role configurations")]
            public async Task ListRoles()
            {
                var guildProperties = Database.GuildProperties.Include(g => g.Roles).AsNoTracking().FirstOrDefault(g => g.ID == Context.Guild.Id);

                EmbedBuilder embed = new Utils.InformationMessage(user: Context.User) { Title = $"Configured roles for {Context.Guild.Name}: " };

                foreach (var r in guildProperties.Roles.Where(r => r.IsColourable || r.IsRenamabale || r.IsAutomaticallyAdded))
                {
                    var role = Context.Guild.GetRole(r.ID);
                    List<string> properties = new List<string>();

                    if (r.IsAutomaticallyAdded) properties.Add("Automatic");
                    if (r.IsRenamabale) properties.Add("Renamable");
                    if (r.IsColourable) properties.Add("Colourable");

                    properties[0] = " - " + properties[0];
                    var fieldValue = string.Join(", ", properties);
                    embed.AddField($"{role.Name}", fieldValue);
                };


                await RespondAsync(embed: embed.Build());
            }
            [SlashCommand("remove", "Remove role configuration")]
            public async Task RemoveRole(
                [Summary("Role")][Autocomplete(typeof(RemoveableRoles))] string roleID)
            {
                var guildProperties = Database.GuildProperties.Include(g => g.Roles).FirstOrDefault(g => g.ID == Context.Guild.Id);

                EmbedBuilder embed;
                var roleToRemove = guildProperties.Roles.ToList().Find(r => r.ID == ulong.Parse(roleID));
                if (roleToRemove != null)
                {
                    guildProperties.Roles.Remove(roleToRemove);
                    Database.Remove(roleToRemove);
                    embed = new Utils.AcknowledgementMessage(user: Context.User) { Title = "Successfully removed role from configuration." };
                }
                else
                {
                    embed = new Utils.ErrorMessage(user: Context.User) { Title = "Could not remove role from configuration." };
                }

                await Database.SaveChangesAsync();
                await RespondAsync(embed: embed.Build());
            }


            public class RoleProperties
            {
                public bool? isRenamable, isColourable, isAutomatic;
                public string timezone = null;

                [ComplexParameterCtor]
                public RoleProperties(bool? isRenamable = null, bool? isColourable = null, bool? isAutomatic = null, [Autocomplete(typeof(TimezoneNames))] string timezone = null)
                {
                    this.isRenamable = isRenamable;
                    this.isColourable = isColourable;
                    this.isAutomatic = isAutomatic;
                    this.timezone = timezone;
                }
            }

            public enum RoleConfigurationChoice : int
            {
                List,
                Remove,
                Modify
            }
        }

        [Group("timezones", "Timezones configurations")]
        public class TimezonesConfiguration : InteractionModuleBase
        {
            public Logger Log { get; set; }
            public DatabaseContext Database { get; set; }
            [SlashCommand("enabled", "Enable timezones role updates")]
            public async Task EnableTimezonesFeature(
                [Summary("Enabled")] bool enabled = false)
            {
                var guildProperty = Database.GuildProperties.FirstOrDefault(g => g.ID == Context.Guild.Id);

                guildProperty.Timezones.Enabled = enabled;

                await Database.SaveChangesAsync();
                await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
            }
        }

    }
}
