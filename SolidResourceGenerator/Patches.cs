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
        internal static void Postfix(Filterable __instance, ref Dictionary<Tag, HashSet<Tag>> __result)
        {
            if (__instance.GetComponent<SolidResourceGenerator>() == null)
                return;

            // 单独构建完整元素列表，不把未发现元素写入原版可能共享的发现记录。
            var options = new Dictionary<Tag, HashSet<Tag>>();
            if (__result != null)
                foreach (var entry in __result)
                {
                    // 沿用原版“无选择”项所在的分类，保留清除选择的入口。
                    if (entry.Value.Contains(GameTags.Void))
                        options.Add(entry.Key, new HashSet<Tag> { GameTags.Void });
                }

            foreach (Element element in ElementLoader.elements)
            {
                if (!SolidResourceSupport.IsSupported(element.tag))
                    continue;

                Tag category = element.materialCategory.IsValid ? element.materialCategory : GameTags.Other;
                if (!options.TryGetValue(category, out var resources))
                {
                    resources = new HashSet<Tag>();
                    options.Add(category, resources);
                }
                resources.Add(element.tag);
            }
            __result = options;
        }
    }
}
