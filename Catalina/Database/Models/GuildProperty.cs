using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Catalina.Database.Models;
public class GuildProperty
{
    [Key]
    public ulong ID { get; set; }

    public ulong? StarboardChannelID { get; set; } = null;

    public virtual Emoji? StarboardEmoji { get; set; }

    public int StarboardThreshhold { get; set; } = 5;

    public List<StarboardMessage> StarboardMessages { get; set; } = new List<StarboardMessage>();

    public List<Response> Responses { get; set; } = new List<Response>();

    public List<Role> Roles { get; set; } = new List<Role>();

    public List<Reaction> Reactions { get; set; }  = new List<Reaction>();
}
