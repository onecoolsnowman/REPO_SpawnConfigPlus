using System.Collections.Generic;
using HarmonyLib;
using SpawnConfig.ExtendedClasses;
using static SpawnConfig.ListManager;
using UnityEngine;

namespace SpawnConfig.Patches;

[HarmonyPatch(typeof(EnemyDirector))]

public class EnemyDirectorPatch {

    public static bool setupDone = false;
    public static int currentDifficultyPick = 3;
    public static bool onlyOneSetup = false;

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void SetupOnStart(EnemyDirector __instance){

        // Only do it once
        if (!setupDone) {
            List<EnemySetup>[] enemiesDifficulties = [__instance.enemiesDifficulty3, __instance.enemiesDifficulty2, __instance.enemiesDifficulty1];

            // Go through existing EnemySetups & the contained spawnObjects and construct extended objects with default values
            int x = 3;
            foreach (List<EnemySetup> enemiesDifficulty in enemiesDifficulties){

                foreach (EnemySetup enemySetup in enemiesDifficulty){

                    // Save enemy spawnObjects to our own dict for adding custom setups later
                    foreach (GameObject spawnObject in enemySetup.spawnObjects){
                        spawnObject.name = spawnObject.name;
                        ExtendedSpawnObject extendedObj = new(spawnObject);
                        if (!spawnObjectsDict.ContainsKey(spawnObject.name)){
                            spawnObjectsDict.Add(spawnObject.name, spawnObject);
                            //extendedSpawnObjects.Add(extendedObj.name, extendedObj);
                        }
                    }
                    
                    // Extend object
                    enemySetupsDict.Add(enemySetup.name, enemySetup);
                    ExtendedEnemySetup extendedSetup = new(enemySetup, x);

                    // Save extended enemy setups to our own dict for later reusal
                    if(!extendedSetups.ContainsKey(enemySetup.name)){
                        extendedSetups.Add(extendedSetup.name, extendedSetup);
                    }
                }
                x--;
            }
            
            // Log Dictionary contents
            SpawnConfig.Logger.LogInfo("Found the following enemy spawnObjects:");
            foreach (KeyValuePair<string, GameObject> entry in spawnObjectsDict){
                SpawnConfig.Logger.LogInfo(entry.Key);
            }

            // Read / update JSON configs
            SpawnConfig.ReadAndUpdateJSON();
            setupDone = true;

            // Remove groups with invalid enemy names
            List<string> invalidGroups = [];
            foreach (KeyValuePair<string, ExtendedEnemySetup> ext in extendedSetups) {
                bool invalid = false;
                foreach(string sp in ext.Value.spawnObjects){
                    if(!spawnObjectsDict.ContainsKey(sp)) {
                        SpawnConfig.Logger.LogError("Unable to resolve enemy name \"" + sp + "\" in group \"" + ext.Value.name+ "\"! This group will be ignored");
                        invalid = true;
                    }
                }
                if(invalid) invalidGroups.Add(ext.Key);
            }
            foreach (string sp in invalidGroups) {
                extendedSetups.Remove(sp);
            }

        }
    }

    [HarmonyPatch("AmountSetup")]
    [HarmonyPrefix]
    public static void AmountSetupOverride(EnemyDirector __instance){

        // Update enemiesDifficulty lists with customized setups
        // Gotta do it here because it seems that the enemiesDifficulty lists get reset to their default values between Awake() and AmountSetup() - And doing it here is required so we can replace the spawnObjects with empty lists for the duration of one level only
        __instance.enemiesDifficulty1.Clear();
        __instance.enemiesDifficulty2.Clear();
        __instance.enemiesDifficulty3.Clear();
        onlyOneSetup = false;

        foreach (KeyValuePair<string, ExtendedEnemySetup> ext in extendedSetups) {
            if(ext.Value.difficulty1Weight > 0) __instance.enemiesDifficulty1.Add(ext.Value.GetEnemySetup());
            if(ext.Value.difficulty2Weight > 0) __instance.enemiesDifficulty2.Add(ext.Value.GetEnemySetup());
            if(ext.Value.difficulty3Weight > 0) __instance.enemiesDifficulty3.Add(ext.Value.GetEnemySetup());
        }
    }

    [HarmonyPatch("PickEnemies")]
    [HarmonyPrefix]
    public static bool PickEnemiesOverride(List<EnemySetup> _enemiesList, EnemyDirector __instance){
        if(_enemiesList == __instance.enemiesDifficulty1) currentDifficultyPick = 1;
        if(_enemiesList == __instance.enemiesDifficulty2) currentDifficultyPick = 2;
        if(_enemiesList == __instance.enemiesDifficulty3) currentDifficultyPick = 3;
        SpawnConfig.Logger.LogInfo("Picking difficulty " + currentDifficultyPick + " setup...");

        int num = DataDirector.instance.SettingValueFetch(DataDirector.Setting.RunsPlayed);
        List<EnemySetup> possibleEnemies = [];

        // Filter the list before doing the selection because we need to only use the weights of EnemySetups that can actually spawn
        int weightSum = 0;
        foreach(EnemySetup enemy in _enemiesList){
            
            // Vanilla code
            if ((enemy.levelsCompletedCondition && (RunManager.instance.levelsCompleted < enemy.levelsCompletedMin || RunManager.instance.levelsCompleted > enemy.levelsCompletedMax)) || num < enemy.runsPlayed)
			{
				continue;
			}

            // Weight logic
            int weight = 0;
            if(currentDifficultyPick == 3) weight = extendedSetups[enemy.name].difficulty3Weight;
            if(currentDifficultyPick == 2) weight = extendedSetups[enemy.name].difficulty2Weight;
            if(currentDifficultyPick == 1) weight = extendedSetups[enemy.name].difficulty1Weight;
            if (weight < 1) continue;
            weightSum += weight;

            possibleEnemies.Add(enemy);
            SpawnConfig.Logger.LogInfo(enemy.name + " = " + weight);
        }

        // Pick EnemySetup
        EnemySetup item = null;
        int randRoll = UnityEngine.Random.Range(1, weightSum);
        foreach (EnemySetup enemy in possibleEnemies) {
            int weight = 0;
            if(currentDifficultyPick == 3) weight = extendedSetups[enemy.name].difficulty3Weight;
            if(currentDifficultyPick == 2) weight = extendedSetups[enemy.name].difficulty2Weight;
            if(currentDifficultyPick == 1) weight = extendedSetups[enemy.name].difficulty1Weight;

            SpawnConfig.Logger.LogDebug("=> " + enemy.name + " = " + weight + " / " + randRoll);

            if (weight >= randRoll) {
                SpawnConfig.Logger.LogInfo("Selected: " + enemy.name);
                if(onlyOneSetup){
                    item = ScriptableObject.CreateInstance<EnemySetup>();
                    item.name = enemy.name;
                    item.spawnObjects = [];
                }else{
                    item = enemy;
                }
                break;
            }else{
                randRoll -= weight;
            }
        }
        
        // Replace all other EnemySetups with empty objects if only this one should spawn
        if(extendedSetups[item.name].thisGroupOnly && !onlyOneSetup){
            
            List<string> names = [];
            int count = __instance.enemyList.Count;
            foreach(EnemySetup enemy in __instance.enemyList){
                names.Add(enemy.name);
            }
            __instance.enemyList.Clear();
            __instance.enemyList.Add(item);
            onlyOneSetup = true;

            for(int i = 0; i < count; i++){
                EnemySetup item2 = ScriptableObject.CreateInstance<EnemySetup>();
                item2.name = names[i];
                item2.spawnObjects = [];
                __instance.enemyList.Add(item2);
            }

        }else{
            __instance.enemyList.Add(item);
        }
        
        return false;
    }

}