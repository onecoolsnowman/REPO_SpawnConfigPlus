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

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void SetupOnAwake(EnemyDirector __instance){

        // Only do it once
        if (!setupDone) {
            List<EnemySetup>[] enemiesDifficulties = [__instance.enemiesDifficulty3, __instance.enemiesDifficulty2, __instance.enemiesDifficulty1];

            // Go through existing EnemySetups & the contained spawnObjects and construct extended objects with default values
            int x = 3;
            foreach (List<EnemySetup> enemiesDifficulty in enemiesDifficulties){
                SpawnConfig.Logger.LogInfo("Checking difficulty " + x);

                foreach (EnemySetup enemySetup in enemiesDifficulty){

                    // Save enemy spawnObjects to our own dict for adding custom setups later
                    foreach (GameObject spawnObject in enemySetup.spawnObjects){
                        ExtendedSpawnObject extendedObj = new(spawnObject);
                        if (!extendedSpawnObjects.ContainsKey(extendedObj.name)){
                            spawnObjectsDict.Add(spawnObject.name, spawnObject);
                            extendedSpawnObjects.Add(extendedObj.name, extendedObj);
                        }
                    }
                    
                    // Extend object
                    enemySetup.name = enemySetup.name;
                    enemySetupsDict.Add(enemySetup.name, enemySetup);
                    ExtendedEnemySetup extendedSetup = new(enemySetup, x);

                    // Save extended enemy setups to our own dict for later reusal
                    if(!extendedSetups.ContainsKey(enemySetup.name)){
                        extendedSetups.Add(extendedSetup.name, extendedSetup);
                    }else{
                        SpawnConfig.Logger.LogWarning("Duplicate EnemySetup name!");
                    }
                }
                x--;
            }
            
            // Log Dictionary contents
            SpawnConfig.Logger.LogInfo("Found the following enemy spawnObjects:");
            foreach (KeyValuePair<string, ExtendedSpawnObject> entry in extendedSpawnObjects){
                SpawnConfig.Logger.LogInfo(entry.Key);
            }

            // Read / update JSON configs
            SpawnConfig.ReadAndUpdateJSON();
            setupDone = true;

            // Update enemiesDifficulty lists with altered setups
            __instance.enemiesDifficulty1.Clear();
            __instance.enemiesDifficulty2.Clear();
            __instance.enemiesDifficulty3.Clear();
            foreach (KeyValuePair<string, ExtendedEnemySetup> ext in extendedSetups) {
                if(ext.Value.difficulty1Weight > 0) __instance.enemiesDifficulty1.Add(ext.Value.GetEnemySetup(spawnObjectsDict));
                if(ext.Value.difficulty2Weight > 0) __instance.enemiesDifficulty2.Add(ext.Value.GetEnemySetup(spawnObjectsDict));
                if(ext.Value.difficulty3Weight > 0) __instance.enemiesDifficulty3.Add(ext.Value.GetEnemySetup(spawnObjectsDict));
            }

        }
    }

    [HarmonyPatch("PickEnemies")]
    [HarmonyPrefix]
    public static bool PickEnemiesOverride(List<EnemySetup> _enemiesList, EnemyDirector __instance){
        SpawnConfig.Logger.LogInfo("Picking difficulty " + currentDifficultyPick + " enemy");
        int num = DataDirector.instance.SettingValueFetch(DataDirector.Setting.RunsPlayed);
        List<EnemySetup> possibleEnemies = [];

        // Make list of pickable setups
        int weightSum = 0;
        foreach(EnemySetup enemy in _enemiesList){
            if ((enemy.levelsCompletedCondition && (RunManager.instance.levelsCompleted < enemy.levelsCompletedMin || RunManager.instance.levelsCompleted > enemy.levelsCompletedMax)) || num < enemy.runsPlayed)
			{
				continue;
			}

            int weight = 0;
            if(currentDifficultyPick == 3) weight = extendedSetups[enemy.name].difficulty3Weight;
            if(currentDifficultyPick == 2) weight = extendedSetups[enemy.name].difficulty2Weight;
            if(currentDifficultyPick == 1) weight = extendedSetups[enemy.name].difficulty1Weight;

            if (weight < 1) continue;

            weightSum += weight;
            possibleEnemies.Add(enemy);
            SpawnConfig.Logger.LogInfo(enemy.name + " = " + weight);
        }

        // Pick setup
        EnemySetup item = null;
        int randRoll = Random.Range(1, weightSum);
        foreach (EnemySetup enemy in possibleEnemies) {
            int weight = 0;
            if(currentDifficultyPick == 3) weight = extendedSetups[enemy.name].difficulty3Weight;
            if(currentDifficultyPick == 2) weight = extendedSetups[enemy.name].difficulty2Weight;
            if(currentDifficultyPick == 1) weight = extendedSetups[enemy.name].difficulty1Weight;

            SpawnConfig.Logger.LogInfo("=> " + enemy.name + " = " + weight + " / " + randRoll);

            if (weight >= randRoll) {
                item = enemy;
                break;
            }else{
                randRoll -= weight;
            }
        }
        __instance.enemyList.Add(item);

        // Skip vanilla code
        currentDifficultyPick = 1;
        return false;
    }

}