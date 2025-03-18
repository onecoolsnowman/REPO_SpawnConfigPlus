using Newtonsoft.Json;
using UnityEngine;

namespace SpawnConfig.ExtendedClasses;

public class ExtendedSpawnObject (GameObject spawnObject) {
    
    public string name = spawnObject.name;
    public bool disabled = false;
    public int biggerGroupChance = 0;
    public int groupIncreaseAmount = 0;
    [JsonIgnore]
    public bool alteredGroupSize = false;

}