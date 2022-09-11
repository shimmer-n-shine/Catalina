using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Catalina.Database.Models;
public class GuildProperty
{
    [Key]
    public ulong ID { get; set; }

    public virtual Starboard Starboard { get; set; }

    public virtual ICollection<Response> Responses { get; set; }

    public virtual ICollection<Role> Roles { get; set; }
}
