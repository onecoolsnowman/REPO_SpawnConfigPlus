using System;
using BepInEx.Configuration;

namespace SpawnConfig;

public class ConfigManager {
    internal ConfigEntry<int> test = null!;
    internal void Setup(ConfigFile configFile) {
        test = configFile.Bind("General", "Test", 1, new ConfigDescription("Test"));
    }
}