using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Catalina.Database.Models
{
    public class Response
    {
        public string ID { get; set; }
        public string GuildID { get; set; }
        public string Trigger { get; set; }
        public string Content { get; set; }
        public string AllowedChannelsSerialised { get; set; }

        [NotMapped]
        public ulong[] AllowedChannels
        {
            get => !string.IsNullOrEmpty(AllowedChannelsSerialised) ? AllowedChannelsSerialised.Split(',').Select(x => Convert.ToUInt64(x)).ToArray() : null;
            set => AllowedChannelsSerialised = string.Join(',', value);
        }

    }
}
