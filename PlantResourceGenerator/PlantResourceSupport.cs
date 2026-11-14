using System.Collections.Generic;
using UnityEngine;

namespace PlantResourceGenerator
{
    internal static class PlantResourceSupport
    {
        internal static HashSet<Tag> GetHarvests()
        {
            // 从实际加载的作物（包括树枝等子预制体）读取产物，不依赖食物配方或发现记录。
            // 不缓存初始化中间结果，后注册、修改或移除的 Mod 作物也能及时反映。
            var harvests = new HashSet<Tag>();
            foreach (GameObject plant in Assets.GetPrefabsWithComponent<Crop>())
            {
                Crop crop = plant.GetComponent<Crop>();
                if (crop == null || string.IsNullOrEmpty(crop.cropVal.cropId) || crop.cropVal.numProduced <= 0)
                    continue;

                var tag = new Tag(crop.cropVal.cropId);
                if (GetSupportedPrefab(tag) != null)
                    harvests.Add(tag);
            }
            return harvests;
        }

        internal static GameObject GetPrefab(Tag tag)
        {
            GameObject prefab = GetSupportedPrefab(tag);
            if (prefab == null)
                return null;

            // 生产时只验证所选产物，避免每台建筑每 200 ms 构建完整的收获物集合。
            foreach (GameObject plant in Assets.GetPrefabsWithComponent<Crop>())
            {
                Crop crop = plant.GetComponent<Crop>();
                if (crop != null && crop.cropVal.numProduced > 0 &&
                    !string.IsNullOrEmpty(crop.cropVal.cropId) && new Tag(crop.cropVal.cropId) == tag)
                    return prefab;
            }
            return null;
        }

        private static GameObject GetSupportedPrefab(Tag tag)
        {
            GameObject prefab = Assets.TryGetPrefab(tag);
            var primaryElement = prefab?.GetComponent<PrimaryElement>();
            // 仅生成可拾取的收获物；保留谷粒、豆类等本身就是收获物的种子。
            if (tag == GameTags.Void || prefab?.GetComponent<KPrefabID>()?.PrefabTag != tag ||
                prefab.GetComponent<Pickupable>() == null || primaryElement == null ||
                !(primaryElement.MassPerUnit > 0f) || float.IsInfinity(primaryElement.MassPerUnit))
                return null;

            Element element = ElementLoader.GetElement(tag);
            // 元素产物只支持有掉落物的固体，液体和气体需要不同的输出机制。
            if (element != null && element.tag == tag &&
                (!element.IsSolid || element.disabled || !(element.highTemp > 1f)))
                return null;
            return prefab;
        }

        internal static Tag GetCategory(Tag tag)
        {
            // 与固体、食物生成器使用相同分类，避免共享侧栏缓存同一资源的多行。
            Element element = ElementLoader.GetElement(tag);
            if (element != null && element.tag == tag)
                return element.materialCategory.IsValid ? element.materialCategory : GameTags.Other;

            GameObject prefab = Assets.TryGetPrefab(tag);
            return prefab != null && (prefab.GetComponent<Edible>() != null ||
                prefab.GetComponent<KPrefabID>().HasTag(GameTags.CookingIngredient))
                ? GameTags.CookingIngredient : GameTags.Other;
        }
    }
}
