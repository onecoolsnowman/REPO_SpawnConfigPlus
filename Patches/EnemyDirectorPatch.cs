using HarmonyLib;

namespace SpawnConfig.Patches;

[HarmonyPatch(typeof(EnemyDirector))]

public class EnemyDirectorPatch {

    [HarmonyPatch("AmountSetup")]
	[HarmonyPostfix]
    public static void totalAmountOverride(EnemyDirector __instance){
        if(SpawnConfig.configManager.totalAmount.Value >= 0){
            __instance.totalAmount = SpawnConfig.configManager.totalAmount.Value;
        }
    } 
}