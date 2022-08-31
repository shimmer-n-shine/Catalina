using Catalina.Database;
using Catalina.Discord.Commands.Autocomplete;
using Catalina.Discord.Commands.Preconditions;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Modules
{
    [RequirePrivilege(AccessLevel.Administrator)]
    [Group("config", "Guild configurations")]
    public class ConfigurationModule : InteractionModuleBase
    {
        [Group("starboard", "Starboard configuration")]
        public class StarboardConfiguration : InteractionModuleBase
        {
            [SlashCommand("channel", "Set starboard channel")]
            public async Task SetStarboardChannel(
                [Summary("Channel")] [ChannelTypes(ChannelType.Text)] IChannel channel = null)
            {
                using var database = new DatabaseContextFactory().CreateDbContext();
                var guildProperties = database.GuildProperties.FirstOrDefault(g => g.ID == Context.Guild.Id);

                guildProperties.StarboardChannelID = channel?.Id;

                await database.SaveChangesAsync();
                await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
            }
            [SlashCommand("emoji", "Set starboard emoji")]
            public async Task SetStarboardEmoji(
                [Summary("Emoji")] string emoji)
            {
                using var database = new DatabaseContextFactory().CreateDbContext();
                var guildProperties = database.GuildProperties.FirstOrDefault(g => g.ID == Context.Guild.Id);
                IEmote emote;
                Database.Models.Emoji catalinaEmoji;
                try
                {
                    catalinaEmoji = await Database.Models.Emoji.ParseAsync(emoji, Context.Guild);
                    emote = await Database.Models.Emoji.ToEmoteAsync(catalinaEmoji, Context.Guild);
                }
                catch (Exception exception)
                {
                    await RespondAsync(embed: new Utils.ErrorMessage(user:Context.User) { Exception = exception });
                    return;
                }

                if (database.Emojis.Any(e => e.NameOrID == catalinaEmoji.NameOrID)) {
                    guildProperties.StarboardEmoji = database.Emojis.First(e => e.NameOrID == catalinaEmoji.NameOrID);
                }
                else
                {
                    guildProperties.StarboardEmoji = catalinaEmoji;
                }

                await database.SaveChangesAsync();
                
                await RespondAsync(embed: new Utils.AcknowledgementMessage (user: Context.User));
            }
            [SlashCommand("threshhold", "Set starboard threshhold")]
            public async Task SetStarboardThreshhold(
                [Summary("Threshhold")]int threshhold)
            {
                if (threshhold <= 0)
                {
                    await Context.Interaction.RespondAsync(embed: new Utils.ErrorMessage (user: Context.User) { Exception = new System.ArgumentException("Threshhold cannot be less than 1.")});
                    return;
                }
                using var database = new DatabaseContextFactory().CreateDbContext();
                var guildProperties = database.GuildProperties.FirstOrDefault(g => g.ID == Context.Guild.Id);

                guildProperties.StarboardThreshhold = threshhold;

                await database.SaveChangesAsync();
                await RespondAsync(embed: new Utils.AcknowledgementMessage (user: Context.User));
            }
        }



        [Group("role", "Guild role configuration")]

        public class RoleConfiguration : InteractionModuleBase
        {
            [SlashCommand("modify", "Modify role configuration")]
            public async Task ConfigureRole(
                [Summary("Role")] IRole role,
                [ComplexParameter] RoleProperties roleConfig)
            {
                using var database = new DatabaseContextFactory().CreateDbContext();
                var guildProperties = database.GuildProperties.Include(g => g.Roles).FirstOrDefault(g => g.ID == Context.Guild.Id);
                Database.Models.Role DBrole;
                DBrole = guildProperties.Roles.FirstOrDefault(r => r.ID == role.Id);

                if (DBrole == null)
                {
                    DBrole = new Database.Models.Role { ID = role.Id };
                    guildProperties.Roles.Add(DBrole);
                    database.SaveChanges();
                }

                if (roleConfig.isRenamable.HasValue) DBrole.IsRenamabale = roleConfig.isRenamable.Value;
                if (roleConfig.isColourable.HasValue) DBrole.IsColourable = roleConfig.isColourable.Value;
                if (roleConfig.isAutomatic.HasValue) DBrole.IsAutomaticallyAdded = roleConfig.isAutomatic.Value;


                await database.SaveChangesAsync();
                await RespondAsync(embed: new Utils.AcknowledgementMessage (user: Context.User));

            }

            [SlashCommand("list", "List guild role configurations")]
            public async Task ListRoles()
            {
                using var database = new DatabaseContextFactory().CreateDbContext();
                var guildProperties = database.GuildProperties.Include(g => g.Roles).AsNoTracking().FirstOrDefault(g => g.ID == Context.Guild.Id);

                EmbedBuilder embed = new Utils.InformationMessage (user: Context.User) { Title = $"Configured roles for {Context.Guild.Name}: " };

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
                [Summary("Role")] [Autocomplete(typeof(RoleRemoval))] string roleID)
            {
                using var database = new DatabaseContextFactory().CreateDbContext();
                var guildProperties = database.GuildProperties.Include(g => g.Roles).FirstOrDefault(g => g.ID == Context.Guild.Id);

                EmbedBuilder embed;
                var roleToRemove = guildProperties.Roles.ToList().Find(r => r.ID == ulong.Parse(roleID));
                if (roleToRemove != null)
                {
                    guildProperties.Roles.Remove(roleToRemove);
                    database.Remove(roleToRemove);
                    embed = new Utils.AcknowledgementMessage (user: Context.User) { Title = "Successfully removed role from configuration."};
                }
                else
                {
                    embed = new Utils.ErrorMessage (user: Context.User) { Title = "Could not remove role from configuration."};
                }

                await database.SaveChangesAsync();
                await RespondAsync(embed: embed.Build());
            }

            public class RoleProperties
            {
                public bool? isRenamable, isColourable, isAutomatic;

                [ComplexParameterCtor]
                public RoleProperties(bool? isRenamable = null, bool? isColourable = null, bool? isAutomatic = null)
                {
                    this.isRenamable = isRenamable;
                    this.isColourable = isColourable;
                    this.isAutomatic = isAutomatic;
                }
            }

            public enum RoleConfigurationChoice : int
            {
                List,
                Remove,
                Modify
            }
        }
    }
}
