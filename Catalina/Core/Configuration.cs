namespace Catalina.Core;

public class Configuration
{
    public CoreConfig Core { get; set; }
    public class CoreConfig
    {
        public string ConnectionString { get; set; }
        public string DiscordToken { get; set; }
    }
}
