namespace Catalina.Database.Models
{
    public class Reaction
    {
        public ulong ID { get; set; }
        public ulong GuildID { get; set; }
        public ulong MessageID { get; set; }
        public ulong RoleID { get; set; }
        public ulong ChannelID { get; set; }
        public string EmojiName { get; set; }
    }
}
