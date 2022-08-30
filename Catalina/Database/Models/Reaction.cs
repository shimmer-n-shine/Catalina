using System.ComponentModel.DataAnnotations;

namespace Catalina.Database.Models
{
    public class Reaction
    {
        [Key] public ulong ID { get; set; }
        public ulong MessageID { get; set; }
        public Role Role { get; set; }
        public ulong ChannelID { get; set; }
        public Emoji Emoji { get; set; }
    }
}
