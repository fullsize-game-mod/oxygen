using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace FoodResourceGenerator
{
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    internal static class RegisterGeneratorPatch
    {
        private static void Prefix()
        {
            ModUtil.AddBuildingToPlanScreen("Utilities", FoodResourceGeneratorConfig.ID);
        }
    }

    [HarmonyPatch(typeof(FilterSideScreen), nameof(FilterSideScreen.IsValidForTarget))]
    internal static class GeneratorResourceSelectionPatch
    {
        internal static void Postfix(FilterSideScreen __instance, GameObject target, ref bool __result)
        {
            if (!__instance.isLogicFilter && target != null && target.GetComponent<FoodResourceGenerator>() != null)
                __result = true;
        }
    }

    [HarmonyPatch(typeof(Filterable), nameof(Filterable.GetTagOptions))]
    internal static class GeneratorFoodOptionsPatch
    {
        internal static void Postfix(Filterable __instance, ref Dictionary<Tag, HashSet<Tag>> __result)
        {
            if (__instance.GetComponent<FoodResourceGenerator>() == null)
                return;

            // 独立列出已加载的原料，无需先发现；不修改原版共享的资源发现记录。
            var options = new Dictionary<Tag, HashSet<Tag>>
            {
                // FilterSideScreen 单独维护 voidRow，必须与原版使用同一保留分类。
                // 放进 Other 会让共享侧栏缓存两个 Void 行，刷新时重复加入回收池。
                [GameTags.Void] = new HashSet<Tag> { GameTags.Void }
            };
            var ingredients = new HashSet<Tag>();
            foreach (Tag tag in FoodResourceSupport.GetIngredients())
                if (FoodResourceSupport.GetPrefab(tag) != null)
                    ingredients.Add(tag);

            if (ingredients.Count > 0)
                options.Add(GameTags.CookingIngredient, ingredients);
            __result = options;
        }
    }
}
