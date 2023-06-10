using Catalina.Common;
using Discord;
using System.Collections.Generic;

namespace Catalina.Discord.Commands.SelectMenuBuilders;
public class ColourMenu : ISelectMenu
{
    public string ID { get; set; } = "colour_menu";
    public string Placeholder { get; set; } = "Pick your colours here!";
    public List<SelectMenuOptionBuilder> Options
    {
        get
        {
            var options = new List<SelectMenuOptionBuilder>();
            var colours = CatalinaColours.ToDictionary();
            var unicode = EmojiToolkit.Emoji.Get(":art:").Raw;
            var emote = Emoji.Parse(unicode);
            foreach (var colour in colours)
            {

                options.Add(new SelectMenuOptionBuilder()
                {
                    Label = colour.Key,
                    Value = colour.Key,
                    Emote = emote
                });
            }
            return options;
        }
    }

    public SelectMenuBuilder ToSelectMenuBuilder()
    {
        return new SelectMenuBuilder(customId: ID, placeholder: Placeholder, options: Options);
    }
}
