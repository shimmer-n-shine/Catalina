using System.Linq;

namespace Catalina.Discord.Commands;
public static class ComponentConstants
{
    public const string SayRoleSelection = "say_menu:*";
    public const string RoleButtonSelection = "slct_rl_btn:*";
    public const string ConfigureRoleMenu = "cfg_rl_menu:*";
    public const string ConfigureRoleRenameable = "cfg_rl_rnm:*";
    public const string ConfigureRoleColourable = "cfg_rl_clr:*";
    public const string ConfigureRoleAuto = "cfg_rl_auto:*";
    public const string ConfigureRoleRetroactiveConfirm = "cfg_rl_rtrctv_ack:*";
    public const string ConfigureRoleRetroactiveDecline = "cfg_rl_rtrctv_dec:*";
    public const string ConfigureRoleReset = "cfg_rl_rst:*";
    public const string ConfigureRoleTimezone = "cfg_rl_tz:*";

    public static string GetComponentWithID(this string s, string sd)
    {
        return s.Replace("*", sd);
    }
    public static string GetComponentName(this string s)
    {
        return s.Split(':').First();
    }
}
