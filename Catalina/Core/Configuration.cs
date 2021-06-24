using Catalina.Discord;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Catalina.Configuration
{
    [Serializable]
    class ConfigValues
    {
        [NonSerialized] public static ConfigValues configValues = new ConfigValues();

        public Dictionary<ulong,List<ulong>> RoleIDs;
        public Dictionary<ulong,List<ulong>> AdminRoleIDs;
        public Dictionary<ulong,List<ulong>> CommandChannels;
        public DiscordActivity DiscordActivity;
        public Dictionary<ulong,string> Prefixes;
        public string Prefix;
        public string FolderPath;
        [NonSerialized] public string ConfigFolder;
        public string DiscordToken;
        public ulong? DevID;
        [NonSerialized] public Dictionary<ulong,List<Response>> Responses;
        [NonSerialized] public Dictionary<ulong,List<Reaction>> Reactions;
        public Dictionary<ulong, DiscordRole> BasicRoleGuildID;
        //[NonSerialized] public Dictionary<string, string> Events;
        public ConfigValues()
        {
            DevID = 194439970797256706;
            RoleIDs = new ();
            CommandChannels = new ();
            AdminRoleIDs = new ();
            DiscordActivity = new DiscordActivity()
            {
                Name = "The beat of your heart...",
                ActivityType = ActivityType.ListeningTo
            };
            Prefix = "c!";
            Prefixes = new Dictionary<ulong, string>();
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Catalina");
            ConfigFolder = Path.Combine(FolderPath, "Config");
            DiscordToken = null;
            Responses = new ();
            Reactions = new ();
            BasicRoleGuildID = new();
            //Events = JsonConvert.DeserializeObject<Dictionary<string, string>>("{ \"AutoBulk\": \"06:00:00\", \"Read\": \"00:15:00\" }");
        }
        public void SaveConfig()
        {
            Directory.CreateDirectory(configValues.ConfigFolder);
            var ConfigFile = Path.Combine(ConfigFolder, "config.json");
            var ResponsesFile = Path.Combine(ConfigFolder, "responses.json");
            var ReactionsFile = Path.Combine(ConfigFolder, "reactions.json");
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(this, Formatting.Indented));
            File.WriteAllText(ResponsesFile, JsonConvert.SerializeObject(this.Responses, Formatting.Indented));
            File.WriteAllText(ReactionsFile, JsonConvert.SerializeObject(this.Reactions, Formatting.Indented));
        }
        public void LoadConfig()
        {
            var ConfigFile = Path.Combine(ConfigFolder, "config.json");
            var ResponsesFile = Path.Combine(ConfigFolder, "responses.json");
            var ReactionsFile = Path.Combine(ConfigFolder, "reactions.json");
            //var EventsFile = Path.Combine(ConfigFolder, "events.json");
            if (File.Exists(ConfigFile))
            {
                var json = File.ReadAllText(ConfigFile);
                configValues = JsonConvert.DeserializeObject<ConfigValues>(json);
                configValues.ConfigFolder = Path.Combine(configValues.FolderPath, "Config");
                Console.WriteLine(string.Format("{0,-25} {1}", "Read configuration values from", ConfigFile));
            }
            else
            {
                Console.WriteLine("No configuration file found at {0}\nCreating one. please edit the file with your api keys and google secrets and press 'K'", configValues.ConfigFolder);
                SaveConfig();
                bool notEnter = true;
                while (notEnter)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.K)
                    {
                        notEnter = false;
                    }
                }
                var _ = File.ReadAllText(ConfigFile);
                configValues = JsonConvert.DeserializeObject<ConfigValues>(_);
                configValues.ConfigFolder = Path.Combine(configValues.FolderPath, "Config");
                Console.WriteLine(string.Format("{0,-25} {1}", "Read configuration values from", ConfigFile));
            }

            if (File.Exists(ResponsesFile))
            {
                var json = File.ReadAllText(ResponsesFile);
                configValues.Responses = JsonConvert.DeserializeObject<Dictionary<ulong,List<Response>>>(json);
                Console.WriteLine(string.Format("{0,-25} {1}", "Read responses from", ResponsesFile));
            }
            else
            {
                Console.WriteLine("No responses file found at {0}, creating one.", configValues.ConfigFolder);
                SaveConfig();
                var json = File.ReadAllText(ResponsesFile);
                configValues.Responses = JsonConvert.DeserializeObject<Dictionary<ulong, List<Response>>>(json);
                Console.WriteLine(string.Format("{0,-25} {1}", "Read responses from", ResponsesFile));
            }

            if (File.Exists(ReactionsFile))
            {
                var json = File.ReadAllText(ReactionsFile);
                configValues.Reactions = JsonConvert.DeserializeObject<Dictionary<ulong, List<Reaction>>>(json);
                Console.WriteLine(string.Format("{0,-25} {1}", "Read reactions from", ReactionsFile));
            }
            else
            {
                Console.WriteLine("No reactions file found at {0}, creating one.", configValues.ConfigFolder);
                SaveConfig();
                var json = File.ReadAllText(ReactionsFile);
                configValues.Reactions = JsonConvert.DeserializeObject<Dictionary<ulong, List<Reaction>>>(json);
                Console.WriteLine(string.Format("{0,-25} {1}", "Read responses from", ReactionsFile));
            }
        }
    }
}