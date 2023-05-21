using Catalina.Database.Models;
using Catalina.Discord.Commands.Autocomplete;
using Catalina.Discord.Commands.Preconditions;
using Discord;
using Discord.Interactions;
using Humanizer;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Catalina.Discord;
using Catalina.Database;

namespace Catalina.Discord.Commands
{
    public class CoreModule : InteractionModuleBase
    {

        [RequirePrivilege(AccessLevel.User)]
        [SlashCommand("ping", "Pong!")]
        public async Task PingMe()
        {
            var originalTime = DateTime.UtcNow;
            Embed embed = new Utils.WarningMessage(user: Context.User)
            {
                Title = "Pong!",
                Body = "Latency: " + Catalina.Discord.Discord.DiscordClient.Latency + " ms",
            };
            await RespondAsync(embed: embed);
            var message = await Context.Interaction.GetOriginalResponseAsync();
            var latency = (message.Timestamp - originalTime).TotalMilliseconds;

            embed = new Utils.AcknowledgementMessage (user: Context.User)
            {
                Title = "Pong!",
                Body = "Latency: " + latency + " ms",
            };
            await Context.Interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embed);
        }

    [RequirePrivilege(AccessLevel.Administrator)]
    [SlashCommand("say", "Send a message as Catalina")]
    public async Task Say([ComplexParameter] EmbedParameter embed = null)
    {
        var embedBuilder = new EmbedBuilder()
        {
            Color = embed.Color,
            Title = embed.Title.Replace("\\n", Environment.NewLine),
            Description = embed.Description.Replace("\\n", Environment.NewLine),
            Footer = new EmbedFooterBuilder
            {
                IconUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(),
                Text = string.Format($"Message sent on behalf of {Context.User.Username}#{Context.User.Discriminator}")
            },
            ImageUrl = embed.ImageUrl
        };
        try
        {
            if (string.IsNullOrEmpty(embed.Title) && string.IsNullOrEmpty(embed.Description)) embedBuilder.Description = $"This is a message sent on behalf of {Context.User.Mention}";
            await Context.Interaction.RespondAsync(embed: new Utils.InformationMessage(Context.User) { Body = "Creating message on your behalf." });
            await Task.Delay(1000);
            await DeleteOriginalResponseAsync();
            await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }
        catch (Exception e)
        {
            await Context.Interaction.RespondAsync(embed: new Utils.ErrorMessage(user: Context.User) { Exception = e }, ephemeral: true);
        }

    }
    public class EmbedParameter
        {
            public string Title; 
            public string Description;
            public string ImageUrl;
            public Color Color;

            [ComplexParameterCtor]
            public EmbedParameter(string title = "", string description = "", [Autocomplete(typeof(ColourNames))] string color = "", string imageUrl = null)
            {
                this.Title = title;
                this.Description = description;
                var colours = CatalinaColours.ToDictionary();
                this.Color = colours.ContainsKey(color) ? colours[color] : CatalinaColours.None;
                this.ImageUrl = imageUrl;
            }
        } 
    }
}
