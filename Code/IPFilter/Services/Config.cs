using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;

namespace IPFilter.Services
{
    class Config
    {
        static readonly Lazy<Configuration> defaultConfig = new Lazy<Configuration>(LoadDefault);

        public static Configuration Default => defaultConfig.Value;

        internal const string DefaultSettings = "settings.json";

        internal static readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        static Configuration LoadDefault()
        {
            try
            {
                if (File.Exists("settings.json"))
                {
                    var json = File.ReadAllText(DefaultSettings);
                    return Parse(json);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Couldn't load {DefaultSettings}: " + ex);
            }

            return new Configuration();
        }

        public static void Reload()
        {
            var config = LoadDefault();

            Config.Default.settings.update.isPreReleaseEnabled = config.settings.update.isPreReleaseEnabled;
            Config.Default.settings.update.isDisabled = config.settings.update.isDisabled;
            Config.Default.settings.task.isEnabled = config.settings.task.isEnabled;
            Config.Default.settings.cache.isEnabled = config.settings.cache.isEnabled;

            Config.Default.outputs.Clear();
            foreach (var output in config.outputs)
            {
                Config.Default.outputs.Add(output);
            }
        }

        public static void Save(Configuration config, string path)
        {
            File.WriteAllText(path,serializer.Serialize(config));
        }

        public static Configuration Parse(string json)
        {
            return serializer.Deserialize<Configuration>(json);
        }

        public class Configuration
        {
            public Configuration()
            {
                settings = new Settings();
                outputs = new List<string>();
            }

            public Settings settings { get; set; }

            public ICollection<string> outputs { get; set; }
        }

        public class Settings
        {
            public Settings()
            {
                update = new UpdateSettings();
                task = new TaskSettings();
                cache = new CacheSettings();
            }

            public UpdateSettings update { get; set; }

            public TaskSettings task { get; set; }

            public CacheSettings cache { get; set; }
        }

        public class UpdateSettings
        {
            public bool isDisabled { get; set; }

            public bool isPreReleaseEnabled { get; set; }
        }

        public class TaskSettings
        {
            public bool isEnabled { get; set; }
        }
        public class CacheSettings
        {
            public bool isEnabled { get; set; }
        }
    }
}