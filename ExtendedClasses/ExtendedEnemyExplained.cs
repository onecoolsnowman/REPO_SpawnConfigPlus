namespace SpawnConfig.ExtendedClasses;

public class ExtendedEnemyExplained {

    public string name = "Name of the enemy group. Must be unique";
    public string levelRangeCondition = "Whether to apply the minLevel and maxLevel conditions to this enemy group (see fields below). Accepted values for this are false and true";
    public string minLevel = "If levelRangeCondition is true then this group can not spawn on levels lower than this number";
    public string maxLevel = "If levelRangeCondition is true then this group can not spawn on levels higher than this number";
    public string runsPlayed = "The group can only spawn if the host player's total sum of runs played is larger than this number";
    public string[] spawnObjects = ["The name of an enemy that should be spawned when this group is picked", "Another one", "You can put as many as you want", "See the mod's description for a list of enemy names. Here's one valid example entry:", "Enemy - Hunter"];
    public string difficulty1Weight = "The weight for the group to spawn in difficulty tier 1. This value divided by the [sum of tier 1 weights of all groups] is the chance for this group to spawn in a tier 1 slot";
    public string difficulty2Weight = "The weight for the group to spawn in difficulty tier 2. This value divided by the [sum of tier 2 weights of all groups] is the chance for this group to spawn in a tier 2 slot";
    public string difficulty3Weight = "The weight for the group to spawn in difficulty tier 3. This value divided by the [sum of tier 3 weights of all groups] is the chance for this group to spawn in a tier 3 slot";
    public string thisGroupOnly = "If this is set to true then this group being selected for spawning will prevent any other groups from spawning on the level";
    public string oneAllowed = "If this is set to true then only one of this group is allowed to spawn (other groups can still spawn)";
    public string alterAmountChance = "A value between 0 and 1. The chance for the group to have more or less enemies in it on a given level. A value of 0.5 is equivalent to 50% while 1.0 is 100% for example. Set to 0 to disable. If this chance triggers then the game will pick a random number between alterAmountMin and alterAmountMax (see below). The result determines how many enemies will be added to the group or how many will be removed from it (if the number is negative) for the duration of the current level. If your group has multiple types of enemies then it is random which of them you may get more or less of";
    public string alterAmountMin = "Minimum number of enemies to add if alterAmountChance triggers. Make it a negative value to remove enemies";
    public string alterAmountMax = "Maximum number of enemies to add if alterAmountChance triggers. Make it a negative value to remove enemies";
    public string levelListType = "Whether to blacklist/whitelist levels using levelList. Accepted values for this are 0 (blacklist) and 1 (whitelist).";
    public string[] levelList = ["List of levels to be blacklisted/whitelisted for this group.", "Separate each item with a comma.", "Here's one valid example entry:", "Level - Arctic"];
    public ExtendedEnemyExplained (){

    }
}