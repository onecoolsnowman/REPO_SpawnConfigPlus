using System.Collections.Generic;
using HarmonyLib;

namespace SpawnConfig.Patches;

[HarmonyPatch(typeof(EnemyDirector))]

public class EnemyDirectorPatch {

    [HarmonyPatch("AmountSetup")]
	[HarmonyPrefix]
    public static void AmountSetupOverride(EnemyDirector __instance){
        
        int amountCurve3Value = (int)__instance.amountCurve3.Evaluate(SemiFunc.RunGetDifficultyMultiplier());
		int amountCurve2Value = (int)__instance.amountCurve2.Evaluate(SemiFunc.RunGetDifficultyMultiplier());
		int amountCurve1Value = (int)__instance.amountCurve1.Evaluate(SemiFunc.RunGetDifficultyMultiplier());

        SpawnConfig.Logger.LogInfo("Enemy counts before multiplying: " + amountCurve3Value + ", " + amountCurve2Value + ", " + amountCurve1Value);
        amountCurve3Value *= SpawnConfig.configManager.enemyGroupMultiplier.Value;
        amountCurve2Value *= SpawnConfig.configManager.enemyGroupMultiplier.Value;
        amountCurve1Value *= SpawnConfig.configManager.enemyGroupMultiplier.Value;
        SpawnConfig.Logger.LogInfo("Enemy counts after multiplying: " + amountCurve3Value + ", " + amountCurve2Value + ", " + amountCurve1Value);

        SpawnConfig.Logger.LogInfo("Picking difficulty 3 enemies:");
        for (int i = 0; i < amountCurve3Value; i++)
		{
			__instance.PickEnemies(__instance.enemiesDifficulty3);
		}
        SpawnConfig.Logger.LogInfo("Picking difficulty 2 enemies:");
		for (int j = 0; j < amountCurve2Value; j++)
		{
			__instance.PickEnemies(__instance.enemiesDifficulty2);
		}
        SpawnConfig.Logger.LogInfo("Picking difficulty 1 enemies:");
		for (int k = 0; k < amountCurve1Value; k++)
		{
			__instance.PickEnemies(__instance.enemiesDifficulty1);
		}
        __instance.amountCurve3Value = amountCurve3Value;
        __instance.amountCurve2Value = amountCurve2Value;
        __instance.amountCurve1Value = amountCurve1Value;
		__instance.totalAmount = amountCurve1Value + amountCurve2Value + amountCurve3Value;

        SpawnConfig.Logger.LogInfo("totalAmount = " + __instance.totalAmount);
        return;
    } 

    [HarmonyPatch("PickEnemies")]
    [HarmonyPostfix]
    public static void LogPickEnemies(List<EnemySetup> _enemiesList, EnemyDirector __instance){
        SpawnConfig.Logger.LogInfo(__instance.enemyList[__instance.enemyList.Count - 1].name);
    }

}