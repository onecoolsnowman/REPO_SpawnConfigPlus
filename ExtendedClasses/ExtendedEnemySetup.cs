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
    public List<string> spawnObjects = [];
    public int difficulty1Weight = 0;
    public int difficulty2Weight = 0;
    public int difficulty3Weight = 0;
    public bool thisGroupOnly = false;
    //public double manorWeightModifier = 1.0;
    //public double arcticWeightModifier = 1.0;
    //public double wizardWeightModifier = 1.0;
    [JsonIgnore]
    public double alterAmountChance = 0.0;
    public int alterAmountMin = 0;
    public int alterAmountMax = 0;
    public ExtendedEnemySetup () {

    }
    public ExtendedEnemySetup (EnemySetup enemySetup, int difficulty) {
        name = enemySetup.name;
        levelsCompletedCondition = enemySetup.levelsCompletedCondition;
        levelsCompletedMax = enemySetup.levelsCompletedMax;
        levelsCompletedMin = enemySetup.levelsCompletedMin;
        runsPlayed = enemySetup.runsPlayed;
        spawnObjects = enemySetup.spawnObjects.Where(obj => !obj.name.Contains("Director")).Select(obj => obj.name).ToList();
        difficulty1Weight = (difficulty == 1) ? 100 : 0;
        difficulty2Weight = (difficulty == 2) ? 100 : 0;
        difficulty3Weight = (difficulty == 3) ? 100 : 0;
    }
    public EnemySetup GetEnemySetup () {
        EnemySetup en = ScriptableObject.CreateInstance<EnemySetup>();
        en.name = name;
        en.spawnObjects = [];
        en.levelsCompletedCondition = levelsCompletedCondition;
        en.levelsCompletedMin = levelsCompletedMin;
        en.levelsCompletedMax = levelsCompletedMax;
        en.runsPlayed = runsPlayed;

        foreach (string objName in spawnObjects){
            en.spawnObjects.Add(spawnObjectsDict[objName]);
        }

        return en;
    }

    public int GetWeight(int difficulty) {

        int weight = difficulty1Weight;
        if(difficulty == 2) weight = difficulty2Weight;
        else if(difficulty == 3) weight = difficulty3Weight;
        return weight;

    }

    public void UpdateWithDefaults (ExtendedEnemySetup defaultSetup) {

        PropertyInfo[] properties = defaultSetup.GetType().GetProperties();

        foreach (PropertyInfo property in properties) {
            object defaultValue = property.GetValue(defaultSetup);
            object customValue = property.GetValue(this);
            object newDefaultValue = property.GetValue(extendedSetups[defaultSetup.name]);

            if(defaultValue == customValue && newDefaultValue != defaultValue){
                SpawnConfig.Logger.LogInfo("Updating unmodified property " + property + ": " + defaultValue + " => " + newDefaultValue);
                property.SetValue(this, newDefaultValue);
            }
        }
    }
}