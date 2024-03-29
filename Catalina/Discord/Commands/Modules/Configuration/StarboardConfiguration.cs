﻿using Catalina.Common;
using Catalina.Database;
using Catalina.Discord.Commands.Preconditions;
using Discord;
using Discord.Interactions;
using Serilog.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Modules;

[RequirePrivilege(AccessLevel.Administrator)]
[Group("config", "Guild configurations")]
public partial class ConfigurationModule : InteractionModuleBase
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
            var guildProperty = Database.Guilds.FirstOrDefault(g => g.ID == Context.Guild.Id);

            guildProperty.StarboardSettings.ChannelID = channel?.Id;

            await Database.SaveChangesAsync();
            await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
        }
        [SlashCommand("emoji", "Set starboard emoji")]
        public async Task SetStarboardEmoji(
            [Summary("Emoji")] string emoji)
        {
            var guildProperties = Database.Guilds.FirstOrDefault(g => g.ID == Context.Guild.Id);
            IEmote emote;
            Database.Models.Emoji catalinaEmoji;
            try
            {
                catalinaEmoji = await Catalina.Database.Models.Emoji.ParseAsync(emoji, Context.Guild);
                emote = await Catalina.Database.Models.Emoji.ToEmoteAsync(catalinaEmoji, Context.Guild);
            }
            catch (Exception exception)
            {
                await RespondAsync(embed: new Utils.ErrorMessage(user: Context.User, exception: exception));
                return;
            }

            guildProperties.StarboardSettings.SetOrCreateEmoji(catalinaEmoji, Database);

            await Database.SaveChangesAsync();

            await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
        }
        [SlashCommand("threshold", "Set starboard threshhold")]
        public async Task SetStarboardThreshhold(
            [Summary("Threshhold")] int threshhold)
        {
            if (threshhold <= 0)
            {
                await Context.Interaction.RespondAsync(embed: new Utils.ErrorMessage(user: Context.User, exception: new Exceptions.InvalidArgumentException("Threshold cannot be less than 1.")));
                return;
            }
            var guildProperties = Database.Guilds.FirstOrDefault(g => g.ID == Context.Guild.Id);

            guildProperties.StarboardSettings.Threshhold = threshhold;

            await Database.SaveChangesAsync();
            await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
        }
    }
}
