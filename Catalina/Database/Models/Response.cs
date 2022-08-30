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
        public string Name { get; set; }
        public string Content { get; set; }

    }
}
