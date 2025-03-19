using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace SpawnConfig.Patches;

[HarmonyPatch(typeof(LevelGenerator))]

public class LevelGeneratorPatch {

    [HarmonyPatch("EnemySpawn")]
    [HarmonyPrefix]
    public static bool LogAndModifySpawns(EnemySetup enemySetup, Vector3 position, LevelGenerator __instance){

        foreach (GameObject spawnObject in enemySetup.spawnObjects)
		{
			GameObject gameObject = (GameManager.instance.gameMode != 0) ? PhotonNetwork.InstantiateRoomObject(__instance.ResourceEnemies + "/" + spawnObject.name, position, Quaternion.identity, 0) : UnityEngine.Object.Instantiate(spawnObject, position, Quaternion.identity);
			EnemyParent component = gameObject.GetComponent<EnemyParent>();
			if ((bool)component)
			{
				component.SetupDone = true;
				gameObject.GetComponentInChildren<Enemy>().EnemyTeleported(position);
				__instance.EnemiesSpawnTarget++;
				EnemyDirector.instance.FirstSpawnPointAdd(component);
                SpawnConfig.Logger.LogInfo(component.Enemy.Health.health);
                SpawnConfig.Logger.LogInfo(component.Enemy.Health.healthCurrent);
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
            return false;
        }

        return false;
    }

}