using Catalina.Common;
using Catalina.Discord.Commands.Preconditions;
using Catalina.Discord.Commands.SelectMenuBuilders;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Modules;
public partial class CoreModule : InteractionModuleBase
{
    private static Dictionary<Guid, SayData> Data = new Dictionary<Guid, SayData>();
    public struct SayData
    {
        public string MessageTitle, MessageBody, ImageURL;
        public Color Color;
    }
    public class SayModal : IModal
    {
        public string Title => "Send a message as Catalina";

        [RequiredInput(false)]
        [InputLabel("Title")]
        [ModalTextInput("say_title", placeholder: "", style: TextInputStyle.Short)]
        public string MessageTitle { get; set; }

        [RequiredInput(true)]
        [InputLabel("Body")]
        [ModalTextInput("say_body", placeholder: "Hello world!", style: TextInputStyle.Paragraph)]
        public string MessageBody { get; set; }

        [RequiredInput(false)]
        [InputLabel("Image embed URL")]
        [ModalTextInput("say_imageURL", placeholder: null, style: TextInputStyle.Short)]
        public string ImageURL { get; set; }
    }

    [DefaultMemberPermissions(PermissionConstants.Administrator)]
    [RequirePrivilege(AccessLevel.Administrator)]
    [SlashCommand("say", "send a message as Catalina")]
    public async Task Say()
    {
        var id = Guid.NewGuid();
        try
        {
            await RespondWithModalAsync<SayModal>(ComponentConstants.SayRoleSelection.GetComponentWithID(id.ToString()));
        }
        catch (Exception ex)
        {
            await RespondAsync(embed: new Utils.ErrorMessage(user: Context.User, exception: ex), ephemeral: true);
        }

    }

    [ModalInteraction(ComponentConstants.SayRoleSelection, true)]
    public async Task ModalResponse(string id, SayModal response)
    {
        var guid = Guid.Parse(id);
        var data = new SayData()
        {
            MessageTitle = response.MessageTitle,
            MessageBody = response.MessageBody,
            ImageURL = response.ImageURL,
        };
        ComponentBuilder componentBuilder = new();
        try
        {
            componentBuilder = new ComponentBuilder()
            .WithSelectMenu(new ColourMenu { ID = $"say_select:{id}" }.ToSelectMenuBuilder());
        }
        catch (Exception ex)
        {
            await RespondAsync(embed: new Utils.ErrorMessage(user: Context.User, exception: ex), ephemeral: true);
            return;
        }



        Data.Add(guid, data);
        await RespondAsync(text: "One last thing! What colour should your message embed be?", components: componentBuilder.Build(), ephemeral: true);
    }

    [ComponentInteraction(ComponentConstants.SayRoleSelection, true)]
    public async Task MenuResponse(string id, string[] options)
    {
        await DeferAsync();
        try
        {
            await Context.Interaction.DeleteOriginalResponseAsync();
        }
        catch (Exception ex)
        {
            try
            {
                await ModifyOriginalResponseAsync(m =>
                {
                    m.Embed = ((EmbedBuilder)new Utils.ErrorMessage(user: Context.User, exception: ex)).Build();
                    m.Components = new ComponentBuilder().Build();
                });
            }
            catch
            {
                var componentBuilder = ComponentBuilder.FromComponents((await GetOriginalResponseAsync()).Components);
                var selectMenu = componentBuilder.ActionRows[0].Components.First(c => c.CustomId.StartsWith(ComponentConstants.SayRoleSelection.GetComponentName()));
                var components = componentBuilder.ActionRows[0].Components;
                components[components.FindIndex(c => c == selectMenu)] = ((SelectMenuComponent)selectMenu).ToBuilder().WithDisabled(true).Build();
                var builtComponents = componentBuilder.Build();

                await ModifyOriginalResponseAsync(m =>
                {
                    m.Components = new ComponentBuilder().Build();
                });
            }
        }

        var guid = Guid.Parse(id);
        var datum = Data[Data.First(d => d.Key.Equals(guid)).Key];
        Data[Data.First(d => d.Key.Equals(guid)).Key] = new SayData
        {
            MessageTitle = datum.MessageTitle,
            MessageBody = datum.MessageBody,
            ImageURL = datum.ImageURL,
            Color = CatalinaColours.FromName(options.First())
        };
        datum = Data[Data.First(d => d.Key.Equals(guid)).Key];

        var embed = new EmbedBuilder()
        {
            Footer = new EmbedFooterBuilder
            {
                IconUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(),
                Text = string.Format($"Message sent on behalf of {Context.User.Username}") + ((Context.User.DiscriminatorValue != 0) ? $"#{Context.User.Discriminator}" : string.Empty),
            },
            Color = datum.Color,
            Title = datum.MessageTitle,
            Description = datum.MessageBody,
            ImageUrl = datum.ImageURL
        };
        if (string.IsNullOrEmpty(datum.MessageTitle) && string.IsNullOrEmpty(datum.MessageBody)) { embed.Description = $"This is a message sent on behalf of {Context.User.Mention}"; }
        try
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            await RespondAsync(embed: new Utils.ErrorMessage(user: Context.User, exception: ex), ephemeral: true);
        }
        Data.Remove(guid);
    }
}
