using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using static SpawnConfig.ListManager;

namespace SpawnConfig.ExtendedClasses;

public class ExtendedEnemySetup {

    public string name = "Nameless";
    public bool levelsCompletedCondition = false;
    public int levelsCompletedMax = 10;
    public int levelsCompletedMin = 0;
    public int runsPlayed = 0;
    public string[] spawnObjects = [];
    public int difficulty1Weight = 0;
    public int difficulty2Weight = 0;
    public int difficulty3Weight = 0;
    public bool preventAllOthers = false;
    public double manorWeightModifier = 1.0;
    public double arcticWeightModifier = 1.0;
    public double wizardWeightModifier = 1.0;
    public int biggerGroupChance = 0;
    public int groupIncreaseAmount = 0;
    [JsonIgnore]
    public bool alteredGroupSize = false;
    public ExtendedEnemySetup (){

    }
    public ExtendedEnemySetup (EnemySetup enemySetup, int difficulty) {
        name = enemySetup.name;
        levelsCompletedCondition = enemySetup.levelsCompletedCondition;
        levelsCompletedMax = enemySetup.levelsCompletedMax;
        levelsCompletedMin = enemySetup.levelsCompletedMin;
        runsPlayed = enemySetup.runsPlayed;
        spawnObjects = enemySetup.spawnObjects.Select(obj => obj.name).ToArray();
        difficulty1Weight = (difficulty == 1) ? 100 : 0;
        difficulty2Weight = (difficulty == 2) ? 100 : 0;
        difficulty3Weight = (difficulty == 3) ? 100 : 0;
    }
    public EnemySetup GetEnemySetup(Dictionary<string, GameObject> spawnObjectsDict1){
        EnemySetup en = ScriptableObject.CreateInstance<EnemySetup>();
        en.name = name;
        en.spawnObjects = [];
        en.levelsCompletedCondition = levelsCompletedCondition;
        en.levelsCompletedMin = levelsCompletedMin;
        en.levelsCompletedMax = levelsCompletedMax;
        en.runsPlayed = runsPlayed;

        foreach (string objName in spawnObjects){
            en.spawnObjects.Add(spawnObjectsDict1[objName]);
        }

        return en;
    }
    public void UpdateWithDefaults(ExtendedEnemySetup defaultSetup){

        PropertyInfo[] properties = defaultSetup.GetType().GetProperties();

        foreach (PropertyInfo property in properties) {
            object defaultValue = property.GetValue(defaultSetup);
            object customValue = property.GetValue(this);
            object newDefaultValue = property.GetValue(extendedSetups[defaultSetup.name]);

            if(defaultValue == customValue && newDefaultValue != defaultValue){
                SpawnConfig.Logger.LogInfo(property + " = " + customValue);
                property.SetValue(this, newDefaultValue);
            }
        }
    }
}