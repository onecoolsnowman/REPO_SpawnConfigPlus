using System;
using BepInEx.Configuration;

namespace SpawnConfig;

public class ConfigManager {
    internal ConfigEntry<bool> preventSpawns = null!;
    internal ConfigEntry<int> enemyGroupMultiplier = null!;
    internal ConfigEntry<int> enemyCountMultiplier = null!;
    internal void Setup(ConfigFile configFile) {
        preventSpawns = configFile.Bind("General", "Prevent enemy spawning", false, new ConfigDescription("Prevent enemy spawning entirely, turning the game into a no-stakes gathering simulator"));

        enemyGroupMultiplier = configFile.Bind("General", "Enemy Group Multiplier", 1, new ConfigDescription("The amount of enemy groups spawned each level is multiplied by this number. Currently has no effect since the game seems to always spawn 2 or 3 per level", new AcceptableValueRange<int>(1, 20), Array.Empty<object>()));

        enemyCountMultiplier = configFile.Bind("General", "Enemy Count Multiplier", 1, new ConfigDescription("The amount of individual enemies per group is multiplied by this number. Warning: Some vanilla groups consist of up to 4 medium-sized or 10 small enemies", new AcceptableValueRange<int>(1, 100), Array.Empty<object>()));
    }
}