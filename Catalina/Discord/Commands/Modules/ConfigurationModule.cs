using Catalina.Database;
using Catalina.Discord.Commands.Autocomplete;
using Catalina.Discord.Commands.Preconditions;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Modules
{
    [RequirePrivilege(AccessLevel.Administrator)]
    [Group("config", "Guild configurations")]
    public class ConfigurationModule : InteractionModuleBase
    {
        [Group("starboard", "Guild starboard configuration")]
        public class StarboardConfiguration : InteractionModuleBase
        {
            [SlashCommand("channel", "Set starboard channel for guild")]
            public async Task SetStarboardChannel([Autocomplete(typeof(ChannelSelector))] string channel)
            {
                ulong channelId;
                try
                {
                    if (!string.IsNullOrEmpty(channel)) channelId = ulong.Parse(channel);
                }
                catch
                {
                    await Context.Interaction.RespondAsync(embed: new Utils.ErrorMessage { Exception = new System.ArgumentException("did not pass a valid channel.") });
                    return;
                }

                using var database = new DatabaseContextFactory().CreateDbContext();
                var guildProperties = database.GuildProperties.FirstOrDefault(g => g.ID == Context.Guild.Id);

                guildProperties.StarBoardChannel = channel is null ? null : ulong.Parse(channel);

                await database.SaveChangesAsync();
                await RespondAsync(embed: new Utils.AcknowledgementMessage { User = Context.User });
            }
            [SlashCommand("emoji", "Set starboard emoji for guild")]
            public async Task SetStarboardEmoji(string emoji)
            {
                using var database = new DatabaseContextFactory().CreateDbContext();
                var guildProperties = database.GuildProperties.FirstOrDefault(g => g.ID == Context.Guild.Id);
                IEmote emote;
                

                try
                {
                   emote = Emoji.Parse(emoji);
                }
                catch
                {
                    try
                    {
                        emote = Emote.Parse(emoji);
                        try
                        {
                            await Context.Guild.GetEmoteAsync((emote as Emote).Id);
                        }
                        catch
                        {
                            throw new System.ArgumentException("emote is not from this guild");
                        }
                    }
                    catch
                    {
                        throw new System.ArgumentException("did not pass a valid emoji");
                    }
                }
                guildProperties.StarboardEmoji = Database.Models.Emoji.Parse(emote, Context.Guild);

                await database.SaveChangesAsync();
                await RespondAsync(embed: new Utils.AcknowledgementMessage { User = Context.User });
            }
            [SlashCommand("threshhold", "Set starboard threshhold for guild")]
            public async Task SetStarboardThreshhold(int threshhold)
            {
                if (threshhold <= 0)
                {
                    await Context.Interaction.RespondAsync(embed: new Utils.ErrorMessage { Exception = new System.ArgumentException("Threshhold cannot be less than 1.")});
                    return;
                }
                using var database = new DatabaseContextFactory().CreateDbContext();
                var guildProperties = database.GuildProperties.FirstOrDefault(g => g.ID == Context.Guild.Id);

                guildProperties.StarboardThreshhold = threshhold;

                await database.SaveChangesAsync();
                await RespondAsync(embed: new Utils.AcknowledgementMessage { User = Context.User });
            }
        }



        [Group("role", "Guild role configuration")]

        public class RoleConfiguration : InteractionModuleBase
        {
            [SlashCommand("modify", "Modify guild role configuration")]
            public async Task ConfigureRole(IRole role, [ComplexParameter] RoleProperties roleConfig)
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
                await RespondAsync(embed: new Utils.AcknowledgementMessage { User = Context.User });

            }

            [SlashCommand("list", "List guild role configurations")]
            public async Task ListRoles()
            {
                using var database = new DatabaseContextFactory().CreateDbContext();
                var guildProperties = database.GuildProperties.Include(g => g.Roles).AsNoTracking().FirstOrDefault(g => g.ID == Context.Guild.Id);

                EmbedBuilder embed = new Utils.InformationMessage { Title = $"Configured roles for {Context.Guild.Name}: ", User = Context.User };

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
            [SlashCommand("remove", "Remove guild role configurations")]
            public async Task RemoveRole([Autocomplete(typeof(RoleRemoval))] string roleID)
            {
                using var database = new DatabaseContextFactory().CreateDbContext();
                var guildProperties = database.GuildProperties.Include(g => g.Roles).FirstOrDefault(g => g.ID == Context.Guild.Id);

                EmbedBuilder embed;
                var roleToRemove = guildProperties.Roles.Find(r => r.ID == ulong.Parse(roleID));
                if (roleToRemove != null)
                {
                    guildProperties.Roles.Remove(roleToRemove);
                    database.Remove(roleToRemove);
                    embed = new Utils.AcknowledgementMessage { Title = "Successfully removed role from configuration.", User = Context.User };
                }
                else
                {
                    embed = new Utils.ErrorMessage { Title = "Could not remove role from configuration.", User = Context.User };
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
