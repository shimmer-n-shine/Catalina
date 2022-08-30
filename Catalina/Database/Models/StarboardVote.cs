using System.ComponentModel.DataAnnotations;

namespace Catalina.Database.Models;
public class StarboardVote
{
    [Key] public ulong ID { get; set; }
    public ulong UserId { get; set; }
}
