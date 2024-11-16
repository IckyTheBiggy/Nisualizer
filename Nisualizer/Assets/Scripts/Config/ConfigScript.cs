using System;
using System.IO;
using UnityEngine;

namespace Config
{
    public class ConfigScript : MonoBehaviour
    {
        private bool _configChanged;
        
        public void Update()
        {
            if (!_configChanged) return;
            _configChanged = false;
            HandleConfigChanged();
        }

        public void Init()
        {
            //Load the default config from Resources
            //Getting text directly because storing it as a text asset and not a string causes some code to not execute later :/
            _defaultConfigText = Resources.Load<TextAsset>("DefaultConfig").text;
            Debug.Assert(_defaultConfigText != null, "Default config was not found in Resources, this is really bad!");
            
            //Generate the default config if it doesn't exist
            GenerateDefaultConfigFile();
            
            //Load the config file
            LoadConfigFile();
            
            //Watch for changes in the config file
            WatchForConfigChanges();
            
            //Load config values
            LoadConfig();
        }

        #region ConfigFile
        
        /// Gets loaded from resources in <see cref="Init"/>
        private string _defaultConfigText;
        
        /// Config text loaded from the config file
        private string _configText;
        
        [SerializeField] private ConfigData _data;
        
        /// Contains all the config values and gets loaded in <see cref="LoadConfig"/>
        public ConfigData Data => _data;

        /// Path to the config
        private readonly string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config/Nisualizer/config.json");
        
        /// <summary>
        /// Generates the default config file if one is not found
        /// </summary>
        /// <returns>Whether config is generated</returns>
        private bool GenerateDefaultConfigFile()
        {
            const string debugPrefix = "GenerateDefaultConfig: ";
            
            //Return if config already exists
            if (File.Exists(_configPath))
            {
                Debug.Log(debugPrefix + "Config already exists, returning");
                return false;
            }

            //Generate parent directories if they don't exist
            var configDir = Path.GetDirectoryName(_configPath);
            var dirExists = Directory.Exists(configDir);
            if (!dirExists) Directory.CreateDirectory(configDir!);
            Debug.Log(debugPrefix + (dirExists ?
                $"Config directory already exists at {configDir}" :
                $"Generated config directory at {configDir}"));
            
            //Write the contents to the target config path
            File.WriteAllText(_configPath, _defaultConfigText);
            Debug.Log(debugPrefix + $"Default config generated at: {_configPath}");
            return true;
        }

        /// Loads the config file into memory
        private void LoadConfigFile()
        {
            const string debugPrefix = "LoadConfigFile: ";
            
            Debug.Assert(File.Exists(_configPath), $"{_configPath} not found");
            _configText = File.ReadAllText(_configPath);
            Debug.Log(debugPrefix + $"Loaded config file from: {_configPath}");
        }
        
        /// Loads config values into a new Config object
        private void LoadConfig()
        {
            const string debugPrefix = "LoadConfig: ";
            
            // Reset to default to make sure all vars are set and changes no longer present in config are undone
            // Using ResetSilent because the OnChanged event will get triggered by Load() anyways
            Data.ResetSilent();
            Debug.Log(debugPrefix + "Reset to default config");

            // Reload the config
            JsonUtility.FromJsonOverwrite(_configText, Data);
            Data.Load();
            Debug.Log(debugPrefix + $"Loaded config from {_configPath}");
        }
        
        #endregion
        
        #region LiveConfigReload

        /// Used to detect file changes
        private FileSystemWatcher _configWatcher;
        
        /// Sets up the FileSystemWatcher to monitor changes to the config file
        private void WatchForConfigChanges()
        {
            const string debugPrefix = "WatchForConfigChanges: ";
            
            _configWatcher = new()
            {
                Path = Path.GetDirectoryName(_configPath),
                Filter = Path.GetFileName(_configPath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
            };
            
            // BUG: Renamed event gets triggered after editing the file(with nvim at least)
            _configWatcher.Changed             += MarkDirty;
            _configWatcher.Renamed             += MarkDirty;
            _configWatcher.Deleted             += MarkDirty;
            _configWatcher.EnableRaisingEvents =  true;
            
            Debug.Log(debugPrefix + $"Config watcher for {_configPath} set up");
        }

        private void MarkDirty(object sender, FileSystemEventArgs args) => _configChanged = true;
        
        /// Handles config file being changed
        private void HandleConfigChanged()
        {
            Debug.Log("Config changed, reloading");
            GenerateDefaultConfigFile();
            LoadConfigFile();
            LoadConfig();
        }

        #endregion

        private void OnDestroy()
        {
            if (_configWatcher == null) return;
            _configWatcher.Changed -= MarkDirty;
            _configWatcher.Renamed -= MarkDirty;
            _configWatcher.Deleted -= MarkDirty;
            _configWatcher.EnableRaisingEvents = false;
        }
    }
}
