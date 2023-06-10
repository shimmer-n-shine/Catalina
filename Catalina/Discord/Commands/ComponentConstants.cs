using System.Linq;

namespace Catalina.Discord.Commands;
public static class ComponentConstants
{
    public const string SayRoleSelection = "say_menu:*";
    public const string RoleButtonSelection = "slct_role_btn:*";
    public const string ConfigureRoleMenu = "cfg_role_menu:*";
    public const string ConfigureRoleRenameable = "cfg_role_rename:*";
    public const string ConfigureRoleColourable = "cfg_role_colour:*";
    public const string ConfigureRoleAutomation = "cfg_role_auto:*";
    public const string ConfigureRoleReset = "cfg_role_reset:*";
    public const string ConfigureRoleTimezone = "cfg_role_tz:*";

    public static string GetComponentWithID(this string s, string sd)
    {
        return s.Replace("*", sd);
    }
    public static string GetComponentName(this string s)
    {
        return s.Split(':').First();
    }
}
