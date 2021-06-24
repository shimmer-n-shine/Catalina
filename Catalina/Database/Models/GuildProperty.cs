using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Catalina.Database.Models
{
    public class GuildProperty
    {
        public ulong ID { get; set; }
        public string AdminRoleIDsSerialised { get; set; }

        [NotMapped]
        public ulong[] AdminRoleIDs
        {
            get => !string.IsNullOrEmpty(AdminRoleIDsSerialised) ? AdminRoleIDsSerialised.Split(',').Select(x => Convert.ToUInt64(x)).ToArray() : null;
            set => AdminRoleIDsSerialised = string.Join(',', value);
        }
        public string CommandChannelsSerialised { get; set; }
        [NotMapped]
        public ulong[] CommandChannels
        {
            get => !string.IsNullOrEmpty(CommandChannelsSerialised) ? CommandChannelsSerialised.Split(',').Select(x => Convert.ToUInt64(x)).ToArray() : null;
            set => CommandChannelsSerialised = string.Join(',', value);
        }
        public string Prefix { get; set; }
        public ulong? DefaultRole { get; set; }
    }
}
