using System;
using BepInEx.Configuration;

namespace SpawnConfig;

public class ConfigManager {
    internal ConfigEntry<bool> preventSpawns = null!;
    internal ConfigEntry<bool> reduceRepeats = null!;
    internal ConfigEntry<int> enemyGroupMultiplier = null!;
    internal ConfigEntry<int> enemyCountMultiplier = null!;
    internal void Setup(ConfigFile configFile) {
        preventSpawns = configFile.Bind("General", "Prevent enemy spawning", false, new ConfigDescription("Prevent enemy spawning entirely, turning the game into a no-stakes gathering simulator"));

        reduceRepeats = configFile.Bind("General", "Reduce repeat spawns", false, new ConfigDescription("If set to true, enemy groups that have spawned in previous levels will have a lower chance of being selected again"));

        enemyGroupMultiplier = configFile.Bind("General", "Enemy Group Multiplier", 1, new ConfigDescription("The amount of enemy groups spawned each level is multiplied by this number. Currently seems to have no effect", new AcceptableValueRange<int>(1, 20), Array.Empty<object>()));

        enemyCountMultiplier = configFile.Bind("General", "Enemy Count Multiplier", 1, new ConfigDescription("The amount of individual enemies per group is multiplied by this number. Warning: Some vanilla groups consist of up to 4 medium-sized or 10 small enemies", new AcceptableValueRange<int>(1, 100), Array.Empty<object>()));
    }
}