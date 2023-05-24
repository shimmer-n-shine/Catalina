using Discord;
using System.Collections.Generic;

namespace Catalina.Discord.Commands.SelectMenuBuilders;
public interface ISelectMenu
{
    public static string ID { get; set; }
    public static string Placeholder { get; set; }

    public static List<SelectMenuOptionBuilder> Options { get; set; }

    public static SelectMenuBuilder ToSelectMenuBuilder() 
    {
        return new SelectMenuBuilder(customId: ID, placeholder: Placeholder, options: Options);
    }
}
