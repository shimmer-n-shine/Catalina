
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalina.Database.Models;

public class DependentRole
{

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong ID { get; set; }
    public ulong RoleID { get; set; }
    public ulong DependentRoleID { get; set; }

    [ForeignKey("RoleID")]
    public virtual Role Role { get; set; }
    [ForeignKey("DependentRoleID")]
    public virtual Role Dependent { get; set; }

}
