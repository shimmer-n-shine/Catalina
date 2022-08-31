using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Catalina.Database.Models;
public class StarboardMessage
{
    [Key] public ulong ID { get; set; }

    public ulong ChannelID { get; set; }

    public ulong MessageID { get; set; }

    public ulong? StarboardMessageID { get; set; }

    public virtual ICollection<StarboardVote> UserVotes { get; set; }
}
