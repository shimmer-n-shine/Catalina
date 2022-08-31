using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Catalina.Database.Models;
public class GuildProperty
{
    [Key]
    public ulong ID { get; set; }

    public ulong? StarboardChannelID { get; set; } = null;

    public virtual Emoji StarboardEmoji { get; set; }

    public int StarboardThreshhold { get; set; } = 5;

    public virtual ICollection<StarboardMessage> StarboardMessages { get; set; }

    public virtual ICollection<Response> Responses { get; set; }

    public virtual ICollection<Role> Roles { get; set; }
}
