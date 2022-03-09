using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Catalina.Database.Models
{
    public class GuildProperty
    {
        [Key]
        public ulong ID { get; set; }
        public string Prefix { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
    }
}
