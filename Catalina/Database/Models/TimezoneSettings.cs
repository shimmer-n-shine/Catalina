using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalina.Database.Models
{
    public class TimezoneSettings
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong ID { get; set; }
        public bool Enabled { get; set; }
    }
}
