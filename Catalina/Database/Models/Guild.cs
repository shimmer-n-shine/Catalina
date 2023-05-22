using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Catalina.Database.Models;
public class Guild
{
    [Key]
    public ulong ID { get; set; }

    public virtual StarboardSettings Starboard { get; set; }
    
    public virtual TimezoneSettings Timezones { get; set; }

    public virtual ICollection<Response> Responses { get; set; }

    public virtual ICollection<Role> Roles { get; set; }
}
