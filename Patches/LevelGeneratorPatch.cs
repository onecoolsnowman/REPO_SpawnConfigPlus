using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static SpawnConfig.ListManager;

namespace SpawnConfig.Patches;

[HarmonyPatch(typeof(LevelGenerator))]

public class LevelGeneratorPatch {

    [HarmonyPatch("EnemySpawn")]
    [HarmonyPrefix]
    public static void LogAndModifySpawns(EnemySetup enemySetup, Vector3 position, LevelGenerator __instance){

        // Re-add missing Director enemies
        bool gnomeCheck = false;
        bool bangCheck = false;
        int directorCount = 0;
        foreach (GameObject spawnObject in enemySetup.spawnObjects){
            if(spawnObject.name.Contains("Gnome") && !gnomeCheck){
                gnomeCheck = true;
                directorCount++;
            }else if(spawnObject.name.Contains("Bang") && !bangCheck){
                bangCheck = true;
                directorCount++;
            }
        }
        if(gnomeCheck) enemySetup.spawnObjects.Insert(0, spawnObjectsDict["Gnome Director"]);
        if(bangCheck) enemySetup.spawnObjects.Insert(0, spawnObjectsDict["Bang Director"]);

        // Modify the amount of enemies randomly
        int randRoll = UnityEngine.Random.Range(1, 100);
        if(randRoll <= extendedSetups[enemySetup.name].alterAmountChance && enemySetup.spawnObjects.Count > 0){
            
            // Randomly pick spawnObjects to add or remove more of
            int amountChange = UnityEngine.Random.Range(extendedSetups[enemySetup.name].alterAmountMin, extendedSetups[enemySetup.name].alterAmountMax);
            if(amountChange > 0){
                while(amountChange > 0){
                    enemySetup.spawnObjects.Add(enemySetup.spawnObjects[SpawnConfig.PickNonDirector(enemySetup)]);
                    amountChange--;
                }
            }else if(amountChange < 0){
                while(amountChange < 0 && enemySetup.spawnObjects.Count > 1){
                    enemySetup.spawnObjects.RemoveAt(SpawnConfig.PickNonDirector(enemySetup));
                    amountChange++;
                }
            }
        }

        // Logging
        Dictionary<string, int> spawnObjects = new();
        foreach (GameObject spawnObject in enemySetup.spawnObjects){
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