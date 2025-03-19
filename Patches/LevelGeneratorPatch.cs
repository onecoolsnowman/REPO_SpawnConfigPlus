using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SpawnConfig.Patches;

[HarmonyPatch(typeof(LevelGenerator))]

public class LevelGeneratorPatch {

    [HarmonyPatch("EnemySpawn")]
    [HarmonyPrefix]
    public static void LogAndModifySpawns(EnemySetup enemySetup, Vector3 position, LevelGenerator __instance){

        // Logging & individual enemy type disabling
        Dictionary<string, int> spawnObjects = new();
        foreach (GameObject spawnObject in enemySetup.spawnObjects){
            if(spawnObject.name == "ww"){
                // TODO
            }

            if(spawnObjects.ContainsKey(spawnObject.name)){
                spawnObjects[spawnObject.name] = spawnObjects[spawnObject.name] + 1;
            }else{
                spawnObjects.Add(spawnObject.name, 1);
            }
        }
        string logString = "";
        foreach (KeyValuePair<string, int> obj in spawnObjects){
            if(logString != "") logString += ", ";
            logString += obj.Key + " x " + obj.Value;
        }

        SpawnConfig.Logger.LogInfo("Attempting to spawn: [" + enemySetup.name + "]   (" + logString.Replace("Enemy - ", "") + ")");
        if(SpawnConfig.configManager.preventSpawns.Value){
            // "Safe" way of doing it without having to skip the original PickEnemies logic
            enemySetup.spawnObjects.Clear();
            SpawnConfig.Logger.LogInfo("Forcibly prevented all spawns!");
            return;
        }
    }

}