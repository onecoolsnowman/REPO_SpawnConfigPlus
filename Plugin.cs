using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using SpawnConfig.ExtendedClasses;
using SpawnConfig.Patches;

namespace SpawnConfig;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class SpawnConfig : BaseUnityPlugin
{
    public static SpawnConfig Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }
    internal static ConfigManager configManager = null!;
    internal static ConfigFile Conf = null!;
    internal static readonly string configVersion = "1.0";
    internal static readonly string exportPath = Path.Combine(Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    internal static readonly string spawnObjectsCfg = Path.Combine(exportPath, "Enemies.json");
    internal static readonly string defaultSpawnObjectsCfg = Path.Combine(exportPath, "Defaults", "Enemies.json");
    internal static readonly string enemySetupsCfg = Path.Combine(exportPath, "SpawnGroups.json");
    internal static readonly string defaultEnemySetupsCfg = Path.Combine(exportPath, "Defaults", "SpawnGroups.json");

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        configManager = new ConfigManager();
        Conf = Config;
        configManager.Setup(Config);
        Directory.CreateDirectory(exportPath);
        Directory.CreateDirectory(Path.Combine(exportPath, "Defaults"));

        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} has loaded!");
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }

    public static ExtendedEnemySetup[] GetObjArrayFromJSON(string path){
        ExtendedEnemySetup[] temp = [];
        if(File.Exists(path)){
            string readFile = File.ReadAllText(path);
            if(readFile != null && readFile != ""){
                temp = JsonConvert.DeserializeObject<ExtendedEnemySetup[]>(readFile);
            }
        }
        return temp;
    }

    public static void ReadAndUpdateJSON(){

        // Read default EnemySetup configs
        ExtendedEnemySetup[] defaultSetupsList = GetObjArrayFromJSON(defaultEnemySetupsCfg);
        // Read custom EnemySetup configs
        ExtendedEnemySetup[] customSetupsList = GetObjArrayFromJSON(enemySetupsCfg);

        // Save default ExtendedEnemySetups to file for comparison purposes on next launch
        ExtendedEnemySetup[] extendedSetupsList = EnemyDirectorPatch.extendedSetups.Select(obj => obj.Value).ToArray();
        File.WriteAllText(defaultEnemySetupsCfg, JsonConvert.SerializeObject(extendedSetupsList, Formatting.Indented));
        if (customSetupsList.Length < 1) {
            Logger.LogInfo("No custom config found! Creating default file and stopping early");
            File.WriteAllText(enemySetupsCfg, JsonConvert.SerializeObject(extendedSetupsList, Formatting.Indented));
            return;
        }
        
        // Add missing default setups back into the custom config
        foreach(ExtendedEnemySetup obj in defaultSetupsList){
            if(!customSetupsList.Any(x => x.name == obj.name)){
                customSetupsList.AddItem(obj);
            }
        }

        // Update custom setups with new the default values from the source code if necessary
        foreach(ExtendedEnemySetup obj in customSetupsList){
            if (!EnemyDirectorPatch.extendedSetups.ContainsKey(obj.name)) {
                // Add custom setups to the default value arrays to avoid errors
                EnemyDirectorPatch.extendedSetups.Add(obj.name, obj);
                defaultSetupsList.AddItem(obj);
            }
            obj.UpdateWithDefaults(defaultSetupsList[Array.FindIndex(defaultSetupsList, objTemp => objTemp.name == obj.name)]);
        }

        // Save custom setups with new updated default values to file
        File.WriteAllText(enemySetupsCfg, JsonConvert.SerializeObject(customSetupsList, Formatting.Indented));

        // Replace vanilla extended setups with the custom ones so that the custom changes take effect ingame
        EnemyDirectorPatch.extendedSetups = customSetupsList.ToDictionary(obj => obj.name);
        
    }
}
