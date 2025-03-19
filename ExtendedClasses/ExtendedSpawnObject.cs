using Newtonsoft.Json;
using UnityEngine;
using static SpawnConfig.ListManager;

namespace SpawnConfig.ExtendedClasses;

public class ExtendedSpawnObject {
    
    public string name = "Nameless";
    public bool disabled = false;
    public int biggerGroupChance = 0;
    public int groupIncreaseAmount = 0;
    [JsonIgnore]
    public bool alteredGroupSize = false;

    public ExtendedSpawnObject(GameObject spawnObject){
        name = spawnObject.name;
    }
    public GameObject GetSpawnObject(){
        return spawnObjectsDict[name];
    }

}