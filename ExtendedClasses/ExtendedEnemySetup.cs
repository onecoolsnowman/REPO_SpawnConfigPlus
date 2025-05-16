using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SpawnConfig.ListManager;

namespace SpawnConfig.ExtendedClasses;

public class ExtendedEnemySetup {

    public string name = "Nameless";
    public bool levelsCompletedCondition = false;
    public int levelsCompletedMax = 10;
    public int levelsCompletedMin = 0;
    public bool levelRangeCondition = false;
    public int minLevel = 0;
    public int maxLevel = 0;
    public int runsPlayed = 0;
    public List<string> spawnObjects = [];
    public float difficulty1Weight = 0.0f;
    public float difficulty2Weight = 0.0f;
    public float difficulty3Weight = 0.0f;
    public bool thisGroupOnly = false;
    public bool oneAllowed = false;
    //public double manorWeightModifier = 1.0;
    //public double arcticWeightModifier = 1.0;
    //public double wizardWeightModifier = 1.0;
    public double alterAmountChance = 0.0;
    public int alterAmountMin = 0;
    public int alterAmountMax = 0;
    public int levelListType = 0;
    public List<string> levelList = [];
    public ExtendedEnemySetup () {

    }
    public ExtendedEnemySetup (EnemySetup enemySetup, int difficulty) {
        name = enemySetup.name;
        if(enemySetup.levelsCompletedCondition){
            levelRangeCondition = true;
            minLevel = enemySetup.levelsCompletedMin + 1;
            maxLevel = enemySetup.levelsCompletedMax + 1;
        }
        runsPlayed = enemySetup.runsPlayed;
        spawnObjects = enemySetup.spawnObjects.Where(obj => !obj.name.Contains("Director")).Select(obj => obj.name).ToList();
        float baseWeight = 100.0f;
        if((bool)enemySetup.rarityPreset){
            // Adjust weights of tier 3 multi-enemy groups to match the spawn rates of vanilla (margin of error of 0.01% per group)
            if(enemySetup.rarityPreset.chance == 60) baseWeight = 1.5f;
            SpawnConfig.Logger.LogDebug(name + " = " + enemySetup.rarityPreset.chance);
        }
        difficulty1Weight = (difficulty == 1) ? baseWeight : 0.0f;
        difficulty2Weight = (difficulty == 2) ? baseWeight : 0.0f;
        difficulty3Weight = (difficulty == 3) ? baseWeight : 0.0f;
    }
    public EnemySetup GetEnemySetup () {
        EnemySetup es = ScriptableObject.CreateInstance<EnemySetup>();
        es.name = name;
        es.spawnObjects = [];
        es.levelsCompletedCondition = levelRangeCondition;
        // Number of levels completed is 1 lower than the level number the player is on
        es.levelsCompletedMin = minLevel - 1;
        es.levelsCompletedMax = maxLevel - 1;
        es.runsPlayed = runsPlayed;

        foreach (string objName in spawnObjects){
            es.spawnObjects.Add(spawnObjectsDict[objName]);
        }

        return es;
    }

    public float GetWeight(int difficulty, List<EnemySetup> enemyList) {

        float weight = difficulty1Weight;
        if(difficulty == 2) weight = difficulty2Weight;
        else if(difficulty == 3) weight = difficulty3Weight;
        if(enemyList.Select(obj => obj.name).ToList().Contains(name)) {
            weight = (float)(weight * SpawnConfig.configManager.repeatMultiplier.Value);
            if (oneAllowed) weight = 0;
        }
        if(weight < 0) weight = 0;
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

    public void Update () {
        // Migrate legacy values
        if(this.levelsCompletedCondition) {
            this.levelRangeCondition = true;
            this.minLevel = this.levelsCompletedMin;
            this.maxLevel = this.levelsCompletedMax;
            this.levelsCompletedCondition = false;
        }
        if(!this.levelRangeCondition && this.maxLevel == 10) this.maxLevel = 0;
    }
}
