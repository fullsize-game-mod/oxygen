using HarmonyLib;
using System.Collections.Generic;
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

    [HarmonyPatch(typeof(Filterable), nameof(Filterable.GetTagOptions))]
    internal static class GeneratorSolidOptionsPatch
    {
        private static void Postfix(Filterable __instance, ref Dictionary<Tag, HashSet<Tag>> __result)
        {
            if (__instance.GetComponent<SolidResourceGenerator>() == null)
                return;

            // 原版固体侧栏包含食物、种子等非元素物品；仅保留本建筑实际能生成的元素。
            // 创建新字典和集合，避免影响其他建筑共用的原版资源列表。
            var options = new Dictionary<Tag, HashSet<Tag>>();
            if (__result != null)
                foreach (var entry in __result)
                {
                    var tags = new HashSet<Tag>();
                    foreach (Tag tag in entry.Value)
                        if (SolidResourceSupport.IsSupported(tag))
                            tags.Add(tag);
                    if (tags.Count > 0)
                        options[entry.Key] = tags;
                }
            __result = options;
        }
    }
}
