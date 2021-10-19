using Discord.Commands;
using System;
using System.Reflection;

namespace Catalina.Discord.Commands
{
    public class Methods
    {
        String GetCommandName(MethodBase method)
        {
            return method.GetCustomAttribute<CommandAttribute>().Text;
        }
    }
}
