using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BobDono.Models
{
    public static class Config
    {
        static Config()
        {
            var configRoot =
                JsonConvert.DeserializeObject<ConfigRoot>(File.ReadAllText($"{AppContext.BaseDirectory}/config.json"));

#if DEBUG
            var config = configRoot.Debug;
#else
            var config = configRoot.Release;
#endif

            ServerId = ulong.Parse(config.ServerId);
            BotId = ulong.Parse(config.BotId);
            Prefix = config.Prefix;
            AuthUsers = config.AuthUsers;
            AllowCreateElection = config.AllowCreateElection;
            AllowCreateMatchup = config.AllowCreateMatchup;
            ExcludedModules = config.ExcludedModules ?? new List<string>();
            BotKey = config.BotKey;
            DatabaseConnectionString = config.DatabaseConnectionString;
        }

        public static ulong ServerId { get; }
        public static ulong BotId { get; }
        public static string Prefix { get; }
        public static List<ulong> AuthUsers { get; }
        public static bool AllowCreateElection { get; }
        public static bool AllowCreateMatchup { get; }
        public static List<string> ExcludedModules { get; }
        public static string BotKey { get; }
        public static string DatabaseConnectionString { get; }

        class ConfigRoot
        {
            public ConfigDto Debug { get; set; }
            public ConfigDto Release { get; set; }
        }

        class ConfigDto
        {
            public string ServerId { get; set; }
            public string BotId { get; set; }
            public string Prefix { get; set; }
            public List<ulong> AuthUsers { get; set; }
            public bool AllowCreateElection { get; set; }
            public bool AllowCreateMatchup { get; set; }
            public List<string> ExcludedModules { get; set; }
            public string BotKey { get; set; }
            public string DatabaseConnectionString { get; set; }
        }
    }
}
