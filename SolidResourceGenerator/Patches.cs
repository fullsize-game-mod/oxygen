using HarmonyLib;
using UnityEngine;

namespace SolidResourceGenerator
{
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    internal static class RegisterGeneratorPatch
    {
        private static void Prefix()
        {
            ModUtil.AddBuildingToPlanScreen("Utilities", SolidResourceGeneratorConfig.ID);
        }
    }

    [HarmonyPatch(typeof(FilterSideScreen), nameof(FilterSideScreen.IsValidForTarget))]
    internal static class GeneratorResourceSelectionPatch
    {
        private static void Postfix(FilterSideScreen __instance, GameObject target, ref bool __result)
        {
            // 仅让本建筑使用原版单选资源侧栏，不改动原有筛选器的显示规则。
            if (!__instance.isLogicFilter && target != null && target.GetComponent<SolidResourceGenerator>() != null)
                __result = true;
        }
    }
}
