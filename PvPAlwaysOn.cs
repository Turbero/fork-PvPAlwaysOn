using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace PvPAlwaysOn
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class PvPAlwaysPlugin : BaseUnityPlugin
    {
        internal const string ModName = "PvPAlwaysOn";
        internal const string ModVersion = "1.0.0";
        private const string ModGUID = "azumatt.PvPAlwaysOn";
        public static string ConnectionError = "";
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        public static readonly ManualLogSource PvPAlwaysLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        private void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Harmony harmony = new(ModGUID);
            _serverConfigLocked = config("General", "Force Server Config", true, "Force Server Config");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            /* General */
            _forcePvP = config("General", "PvPForced", true, "Force PvP on the server");

            harmony.PatchAll(assembly);

            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                PvPAlwaysLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
                
            }
            catch
            {
                PvPAlwaysLogger.LogError($"There was an issue loading your {ConfigFileName}");
                PvPAlwaysLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        #region ConfigOptions

        private static ConfigEntry<bool>? _serverConfigLocked;
        internal static ConfigEntry<bool>? _forcePvP;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        #endregion
    }
}