using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SpawnConfig.ExtendedClasses;
using UnityEngine;

namespace SpawnConfig.Patches;

[HarmonyPatch(typeof(EnemyDirector))]

public class EnemyDirectorPatch {

    public static bool setupDone = false;
    public static Dictionary<string, EnemySetupExtender> enemySetups = new Dictionary<string, EnemySetupExtender>();
    public static Dictionary<string, GameObject> enemySpawnObjects = new Dictionary<string, GameObject>();

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void SetupOnAwake(EnemyDirector __instance){

        // Only do it once
        if (!setupDone) {
            List<EnemySetup>[] enemiesDifficulties = [__instance.enemiesDifficulty3, __instance.enemiesDifficulty2, __instance.enemiesDifficulty1];

            // Go through existing enemy setups and construct extended setups + generate config
            int x = 3;
            foreach (List<EnemySetup> enemiesDifficulty in enemiesDifficulties){
                SpawnConfig.Logger.LogInfo("Checking difficulty " + x);

                foreach (EnemySetup enemySetup in enemiesDifficulty){
                    
                    EnemySetupExtender extendedSetup = new();
                    extendedSetup.difficulty = x;
                    if(enemySetup.name.Contains("Gnome") || enemySetup.name.Contains("Bang")) extendedSetup.hasDirector = true;
                    
                    // Multiply spawnObjects with enemyCountMultiplier
                    int spawnGroupSize = enemySetup.spawnObjects.Count;
                    int copyIndex = 0;
                    if(extendedSetup.hasDirector){
                        // Gnome and Bang edge case since they have a special object that shouldn't be duplicated
                        copyIndex = 1;
                        spawnGroupSize--;
                    }
                    int objectsToAdd = (SpawnConfig.configManager.enemyCountMultiplier.Value - 1) * spawnGroupSize;
                    SpawnConfig.Logger.LogInfo("Adding " + objectsToAdd + " extra enemies to group " + enemySetup.name);
                    for(int i = 0; i < objectsToAdd; i++){
                        enemySetup.spawnObjects.Add(enemySetup.spawnObjects[copyIndex]);
                    }

                    // Save extended enemy setups to our own dict for later reusal
                    extendedSetup.enemySetup = enemySetup;
                    if(!enemySetups.ContainsKey(enemySetup.name)){
                        enemySetups.Add(enemySetup.name, extendedSetup);
                    }
                    
                    // Save enemy spawnObjects to our own dict for adding custom setups later
                    foreach (GameObject spawnObject in enemySetup.spawnObjects){
                        if(!enemySpawnObjects.ContainsKey(spawnObject.name)){
                            enemySpawnObjects.Add(spawnObject.name, spawnObject);
                        }
                    }
                }
                x--;
            }
            
            // Log Dictionary contents
            SpawnConfig.Logger.LogInfo("Found the following enemy spawnObjects:");
            foreach (KeyValuePair<string, GameObject> entry in enemySpawnObjects){
                SpawnConfig.Logger.LogInfo(entry.Key);
            }

            setupDone = true;
        }
    }

    [HarmonyPatch("AmountSetup")]
	[HarmonyPrefix]
    public static bool AmountSetupOverride(EnemyDirector __instance){
        
        int amountCurve3Value = (int)__instance.amountCurve3.Evaluate(SemiFunc.RunGetDifficultyMultiplier());
		int amountCurve2Value = (int)__instance.amountCurve2.Evaluate(SemiFunc.RunGetDifficultyMultiplier());
		int amountCurve1Value = (int)__instance.amountCurve1.Evaluate(SemiFunc.RunGetDifficultyMultiplier());

        string logCurveValues = "Enemy setup counts: " + amountCurve3Value + ", " + amountCurve2Value + ", " + amountCurve1Value;
        amountCurve3Value *= SpawnConfig.configManager.enemyGroupMultiplier.Value;
        amountCurve2Value *= SpawnConfig.configManager.enemyGroupMultiplier.Value;
        amountCurve1Value *= SpawnConfig.configManager.enemyGroupMultiplier.Value;
        SpawnConfig.Logger.LogInfo(logCurveValues + " => " + amountCurve3Value + ", " + amountCurve2Value + ", " + amountCurve1Value);

        SpawnConfig.Logger.LogInfo("Picking difficulty 3 enemy setups:");
        for (int i = 0; i < amountCurve3Value; i++)
		{
			__instance.PickEnemies(__instance.enemiesDifficulty3);
		}
        SpawnConfig.Logger.LogInfo("Picking difficulty 2 enemy setups:");
		for (int j = 0; j < amountCurve2Value; j++)
		{
			__instance.PickEnemies(__instance.enemiesDifficulty2);
		}
        SpawnConfig.Logger.LogInfo("Picking difficulty 1 enemy setups:");
		for (int k = 0; k < amountCurve1Value; k++)
		{
			__instance.PickEnemies(__instance.enemiesDifficulty1);
		}
        __instance.amountCurve3Value = amountCurve3Value;
        __instance.amountCurve2Value = amountCurve2Value;
        __instance.amountCurve1Value = amountCurve1Value;
		__instance.totalAmount = amountCurve1Value + amountCurve2Value + amountCurve3Value;

        SpawnConfig.Logger.LogInfo("Enemy setup total = " + __instance.totalAmount);
        return false;
    } 

    [HarmonyPatch("PickEnemies")]
    [HarmonyPostfix]
    public static void LogPickEnemies(List<EnemySetup> _enemiesList, EnemyDirector __instance){
        SpawnConfig.Logger.LogInfo(__instance.enemyList[__instance.enemyList.Count - 1].name);
    }

}