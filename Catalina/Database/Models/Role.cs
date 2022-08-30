using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalina.Database.Models
{
    public class Role
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong ID { get; set; }
        public bool IsAutomaticallyAdded { get; set; }
        public bool IsColourable { get; set; }
        public bool IsRenamabale { get; set; }
    }
}
