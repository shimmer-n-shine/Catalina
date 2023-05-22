using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
