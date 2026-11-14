using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace PlantResourceGenerator
{
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    internal static class RegisterGeneratorPatch
    {
        private static void Prefix()
        {
            ModUtil.AddBuildingToPlanScreen("Utilities", PlantResourceGeneratorConfig.ID);
        }
    }

    [HarmonyPatch(typeof(FilterSideScreen), nameof(FilterSideScreen.IsValidForTarget))]
    internal static class GeneratorResourceSelectionPatch
    {
        internal static void Postfix(FilterSideScreen __instance, GameObject target, ref bool __result)
        {
            if (!__instance.isLogicFilter && target != null && target.GetComponent<PlantResourceGenerator>() != null)
                __result = true;
        }
    }

    [HarmonyPatch(typeof(Filterable), nameof(Filterable.GetTagOptions))]
    internal static class GeneratorPlantOptionsPatch
    {
        internal static void Postfix(Filterable __instance, ref Dictionary<Tag, HashSet<Tag>> __result)
        {
            if (__instance.GetComponent<PlantResourceGenerator>() == null)
                return;

            var options = new Dictionary<Tag, HashSet<Tag>>
            {
                // 原版保留分类，不能把无选择行混入普通资源分类。
                [GameTags.Void] = new HashSet<Tag> { GameTags.Void }
            };
            foreach (Tag tag in PlantResourceSupport.GetHarvests())
            {
                Tag category = PlantResourceSupport.GetCategory(tag);
                if (!options.TryGetValue(category, out var resources))
                {
                    resources = new HashSet<Tag>();
                    options.Add(category, resources);
                }
                resources.Add(tag);
            }
            __result = options;
        }
    }
}
