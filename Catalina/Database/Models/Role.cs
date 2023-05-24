using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalina.Database.Models;

public class Role
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong ID { get; set; }
    public bool IsAutomaticallyAdded { get; set; }
    public bool IsRetroactivelyAdded { get; set; }

    public virtual ICollection<DependentRole> DependentRoles { get; set; } = new List<DependentRole>();
    public bool IsColourable { get; set; }
    public bool IsRenamabale { get; set; }
    public string Timezone { get; set; }
    public virtual Guild Guild { get; set; }
}
