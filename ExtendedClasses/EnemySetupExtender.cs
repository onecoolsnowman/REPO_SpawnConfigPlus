using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using SpawnConfig.Patches;

namespace SpawnConfig.ExtendedClasses;

public class ExtendedEnemySetup {

    public string name = "Nameless";
    public bool levelsCompletedCondition = false;
    public int levelsCompletedMax = 10;
    public int levelsCompletedMin = 0;
    public int runsPlayed = 0;
    public string[] spawnObjects = [];
    public int difficultyOneWeight = 0;
    public int difficultyTwoWeight = 0;
    public int difficultyThreeWeight = 0;
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
        spawnObjects = enemySetup.spawnObjects.Select(obj => obj.name.Replace("Enemy - ", "")).ToArray();
        difficultyOneWeight = (difficulty == 1) ? 100 : 0;
        difficultyTwoWeight = (difficulty == 2) ? 100 : 0;
        difficultyThreeWeight = (difficulty == 3) ? 100 : 0;
    }
    public EnemySetup ToVanillaObject(EnemySetup enemySetup){
        return enemySetup;
    }
    public void UpdateWithDefaults(ExtendedEnemySetup defaultSetup){

        PropertyInfo[] properties = defaultSetup.GetType().GetProperties();

        foreach (PropertyInfo property in properties) {
            object defaultValue = property.GetValue(defaultSetup);
            object customValue = property.GetValue(this);
            object newDefaultValue = property.GetValue(EnemyDirectorPatch.extendedSetups[defaultSetup.name]);

            if(defaultValue == customValue && newDefaultValue != defaultValue){
                SpawnConfig.Logger.LogInfo(property + " = " + customValue);
                property.SetValue(this, newDefaultValue);
            }
        }
    }

    public void UpdateUnconditional(object obj){
        PropertyInfo[] customProperties = obj.GetType().GetProperties();
        PropertyInfo[] myProperties = this.GetType().GetProperties();

        foreach (PropertyInfo property in customProperties) {
            if(myProperties.Contains(property)){
                property.SetValue(this, property.GetValue(obj));
            }
        }
    }
}