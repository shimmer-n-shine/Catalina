using System.ComponentModel.DataAnnotations;

namespace Catalina.Database.Models;

public class Response
{
    [Key] public ulong ID { get; set; }
    public string Trigger { get; set; }
    public string Name { get; set; }
    public string Content { get; set; }

}
