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
using static SpawnConfig.ListManager;

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
    internal static readonly string explanationCfg = Path.Combine(exportPath, "SpawnGroups-Explained.json");
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

    public static int PickNonDirector(EnemySetup enemySetup){
        int pickedIndex = -1;
        while(pickedIndex == -1){
            int index = UnityEngine.Random.Range(0, enemySetup.spawnObjects.Count);
            if(!enemySetup.spawnObjects[index].name.Contains("Director")) pickedIndex = index;
        }
        return pickedIndex;
    }

    public static void ReadAndUpdateJSON(){

        // Save config explanation file
        ExtendedEnemyExplained[] explained = [new ExtendedEnemyExplained()];
        File.WriteAllText(explanationCfg, JsonConvert.SerializeObject(explained, Formatting.Indented));

        // Read default EnemySetup configs
        ExtendedEnemySetup[] defaultSetupsArray = GetObjArrayFromJSON(defaultEnemySetupsCfg);
        // Read custom EnemySetup configs
        ExtendedEnemySetup[] customSetupsArray = GetObjArrayFromJSON(enemySetupsCfg);

        // Save default ExtendedEnemySetups to file for comparison purposes on next launch
        ExtendedEnemySetup[] extendedSetupsArray = extendedSetups.Select(obj => obj.Value).ToArray();
        File.WriteAllText(defaultEnemySetupsCfg, JsonConvert.SerializeObject(extendedSetupsArray, Formatting.Indented));
        if (customSetupsArray.Length < 1) {
            Logger.LogInfo("No custom config found! Creating default file and stopping early");
            File.WriteAllText(enemySetupsCfg, JsonConvert.SerializeObject(extendedSetupsArray, Formatting.Indented));
            return;
        }

        // Update custom setups with the default values from the source code where necessary
        foreach(ExtendedEnemySetup custom in customSetupsArray){
            if (extendedSetups.ContainsKey(custom.name)) {
                custom.UpdateWithDefaults(defaultSetupsArray[Array.FindIndex(defaultSetupsArray, objTemp => objTemp.name == custom.name)]);
            }
        }

        // Add missing enemies from source into the custom config
        Dictionary<string, ExtendedEnemySetup> tempDict = customSetupsArray.ToDictionary(obj => obj.name);
        foreach (KeyValuePair<string, ExtendedEnemySetup> source in extendedSetups) {
            if(!tempDict.ContainsKey(source.Value.name) && configManager.addMissingGroups.Value){
                Logger.LogInfo("Adding missing entry to custom config: " + source.Value.name);
                tempDict.Add(source.Value.name, source.Value);
            }
        }
        customSetupsArray = tempDict.Values.ToArray();

        // Save custom setups with new updated default values to file
        File.WriteAllText(enemySetupsCfg, JsonConvert.SerializeObject(customSetupsArray, Formatting.Indented));

        // Replace vanilla extended setups with the custom ones so that the custom changes take effect ingame
        extendedSetups = customSetupsArray.ToDictionary(obj => obj.name);
        
    }
}