using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Catalina.Database.Models;
public class GuildProperty
{
    [Key]
    public ulong ID { get; set; }
    public List<Role> Roles { get; set; } = new List<Role>();

    public ulong? StarBoardChannel { get; set; } = null;

    public Emoji? StarboardEmoji { get; set; } = null;

    public int StarboardThreshhold { get; set; } = 5;

}
