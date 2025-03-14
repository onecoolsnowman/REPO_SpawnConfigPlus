using HarmonyLib;
using UnityEngine;

namespace SpawnConfig.Patches;

[HarmonyPatch(typeof(LevelGenerator))]

public class LevelGeneratorPatch {

    [HarmonyPatch("EnemySpawn")]
    [HarmonyPrefix]
    public static void LogAndModifySpawns(EnemySetup enemySetup, Vector3 position, LevelGenerator __instance){

        // Multiply spawnObjects
        int spawnGroupSize = enemySetup.spawnObjects.Count;
        int index = 0;
        if(enemySetup.spawnObjects[0].name.Contains("Director")){
            // Gnome and Bang edge case since their first object is not an actual enemy
            index = 1;
            spawnGroupSize--;
        }
        int objectsToAdd = (SpawnConfig.configManager.enemyCountMultiplier.Value - 1) * spawnGroupSize;
        if(spawnGroupSize > 10){objectsToAdd = 0;}
        SpawnConfig.Logger.LogInfo("Adding " + objectsToAdd + " extra enemies to group");
        for(int i = 0; i < objectsToAdd; i++){
            enemySetup.spawnObjects.Add(enemySetup.spawnObjects[index]);
        }

        SpawnConfig.Logger.LogInfo("Attempting to spawn: " + enemySetup.name + " x " + enemySetup.spawnObjects.Count);
        if(SpawnConfig.configManager.preventSpawns.Value){
            // Had to do it this way because there seem to always be two fallback enemies getting added to EnemyDirector.enemyList and idk what causes it
            SpawnConfig.Logger.LogInfo("Forcibly preventing all spawns");
            enemySetup.spawnObjects.Clear();
            return;
        }
    }

}