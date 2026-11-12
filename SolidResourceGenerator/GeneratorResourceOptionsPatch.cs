using System.Collections.Generic;
using HarmonyLib;

namespace SolidResourceGenerator
{
    [HarmonyPatch(typeof(Filterable), nameof(Filterable.GetTagOptions))]
    internal static class GeneratorResourceOptionsPatch
    {
        internal static void Postfix(Filterable __instance, ref Dictionary<Tag, HashSet<Tag>> __result)
        {
            if (__instance.GetComponent<SolidResourceGenerator>() == null)
                return;

            // 单独构建列表，不把未发现元素写入原版可能共享的发现记录。
            var options = new Dictionary<Tag, HashSet<Tag>>();
            foreach (var category in __result)
            {
                // 沿用原版“无选择”项所在的分类，保留清除选择的入口。
                if (category.Value.Contains(GameTags.Void))
                    options.Add(category.Key, new HashSet<Tag> { GameTags.Void });
            }

            foreach (Element element in ElementLoader.elements)
            {
                if (!SolidResourceGenerator.CanGenerate(element))
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
