using BepInEx.Configuration;

namespace SpawnConfig;

public class ConfigManager {
    internal ConfigEntry<int> totalAmount = null!;
    internal void Setup(ConfigFile configFile) {
        totalAmount = configFile.Bind("General", "totalAmount Override", -1, new ConfigDescription("The total amount of enemies to spawn each level. Set it to -1 to not change the vanilla behavior"));
    }
}