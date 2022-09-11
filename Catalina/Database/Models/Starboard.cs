using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Catalina.Database.Models;
public class Starboard
{
    public Starboard()
    {
        Messages = new HashSet<Message>();

    }

    [Key]
    public ulong ID { get; set; }

    public ulong? ChannelID { get; set; } = null;

    public virtual Emoji Emoji { get; private set; }

    public int Threshhold { get; set; } = 5;

    public virtual ICollection<Message> Messages { get; set; }

    public void SetOrCreateEmoji(Emoji emoji, DatabaseContext database)
    {

        if (database.Emojis.Any(e => e.NameOrID == emoji.NameOrID))
        {
            this.Emoji = database.Emojis.First(e => e.NameOrID == emoji.NameOrID);
        }
        else
        {
            this.Emoji = emoji;
        }

        //this.Emoji = database.Emojis.Any(e => e.NameOrID == emoji.NameOrID) ? database.Emojis.First(e => e.NameOrID == emoji.NameOrID) : emoji;

        database.SaveChanges();
    }
}
public class Message
{
    [Key] public ulong ID { get; set; }

    public ulong ChannelID { get; set; }

    public ulong MessageID { get; set; }

    public ulong? StarboardMessageID { get; set; }

    public virtual ICollection<Vote> Votes { get; set; }
}

public class Vote
{
    [Key]
    public ulong ID { get; set; }
    public ulong UserId { get; set; }
}

