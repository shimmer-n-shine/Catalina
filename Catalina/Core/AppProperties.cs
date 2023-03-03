using IniParser;
using IniParser.Model;
using System;
using System.IO;
using Serilog;
using System.Linq;

namespace Catalina
{
    public static class AppConfig
    {
        public static ulong ChannelID;
        public static ulong[] RoleIDs;
        public static string DiscordToken;
        public static string FormLink;
        public static ulong DeveloperID;

        public static void LoadConfig()
        {;
            var data = new FileIniDataParser().ReadFile(Path.Combine(AppContext.BaseDirectory, "config.ini"));
            try
            {

                var channelID = ParseOrDefault(data, "Greeter", "ChannelID");
                if (string.IsNullOrEmpty(channelID))
                {
                    ChannelID = 0;
                }
                else
                {
                    ChannelID = ulong.Parse(channelID);
                }

                var roleIDs = data.Sections["Greeter.IDs"].Select(s => s.Value);
                if (roleIDs.All(string.IsNullOrEmpty))
                {
                    RoleIDs = null;
                }
                else
                {
                    RoleIDs = roleIDs.Select(ulong.Parse).ToArray();
                }

                var developerID = ParseOrDefault(data, "AppData", "DeveloperID");
                if (string.IsNullOrEmpty(developerID))
                {
                    DeveloperID = 0;
                }
                else
                {
                    DeveloperID = ulong.Parse(developerID);
                }

                var botToken = ParseOrDefault(data, "AppData", "Token");
                if (string.IsNullOrEmpty(botToken))
                {
                    throw new Exception("Bot token can not be blank.");
                }
                else
                {
                    DiscordToken = botToken;
                }

                var formLink = ParseOrDefault(data, "Greeter", "FormLink");
                if (string.IsNullOrEmpty(formLink))
                {
                    throw new Exception("Formlink can not be blank.");
                }
                else
                {
                    FormLink = formLink;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("Config.ini could not parse correctly. Please double check:\n {exception}", ex);
                Environment.Exit(-1);
            }
        }
        private static string ParseOrDefault(IniData data, string section, string value)
        {
            try
            {
                return data[section][value];
            }
            catch
            {
                return null;
            }
        }
        public static void SaveConfig()
        {
            var data = new FileIniDataParser().ReadFile(Path.Combine(AppContext.BaseDirectory, "config.ini"));

            try
            {
                data["Greeter"]["ChannelID"] = ChannelID.ToString();
                var keyData = new KeyDataCollection();
                for (int i = 0; i < RoleIDs.Length; i++)
                {
                    keyData.AddKey(new KeyData($"RoleID{i}"));
                    keyData[$"RoleID{i}"] = RoleIDs[i].ToString();
                }
                data.Sections["Greeter.IDs"].RemoveAllKeys();
                foreach (var kd in keyData)
                {
                    data.Sections["Greeter.IDs"].AddKey(kd);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("Could not attribute save data properly: {exception}", ex);
                Environment.Exit(-1);
            }

            new FileIniDataParser().WriteFile(Path.Combine(AppContext.BaseDirectory, "config.ini"), data);
        }
    }
}
