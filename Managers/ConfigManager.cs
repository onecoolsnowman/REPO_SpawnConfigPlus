using BepInEx.Configuration;

namespace SpawnConfig;

public class ConfigManager {
    internal ConfigEntry<bool> preventSpawns = null!;
    internal ConfigEntry<bool> addMissingGroups = null!;
    //internal ConfigEntry<bool> reduceRepeats = null!;
    //internal ConfigEntry<int> enemyGroupMultiplier = null!;
    //internal ConfigEntry<double> enemyCountMultiplier = null!;
    internal void Setup(ConfigFile configFile) {
        preventSpawns = configFile.Bind("General", "Prevent enemy spawning", false, new ConfigDescription("Prevent enemy spawning entirely, turning the game into a no-stakes gathering simulator or for when you want to test something in peace"));

        addMissingGroups = configFile.Bind("General", "Re-add missing groups", true, new ConfigDescription("Whether the mod should update your custom SpawnGroups config at launch by adding all loaded enemy groups that are missing from it"));

        //reduceRepeats = configFile.Bind("General", "Reduce repeat spawns", false, new ConfigDescription("If set to true, enemy groups that have spawned in previous levels will have a lower chance of being selected again"));

        //enemyGroupMultiplier = configFile.Bind("General", "Enemy Group Multiplier", 1, new ConfigDescription("The amount of enemy groups spawned each level is multiplied by this number. Currently seems to have no effect", new AcceptableValueRange<int>(1, 20), Array.Empty<object>()));

        //enemyCountMultiplier = configFile.Bind("General", "Enemy Count Multiplier", 1.0, new ConfigDescription("The amount of individual enemies per group is multiplied by this number. Warning: Some vanilla groups consist of as many as 10 small enemies. Setting this higher than 10 is not recommended", new AcceptableValueRange<double>(1.0, 100.0), Array.Empty<object>()));
    }
}