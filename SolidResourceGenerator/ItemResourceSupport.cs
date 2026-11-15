using System.Collections.Generic;
using UnityEngine;

namespace SolidResourceGenerator
{
    internal static class ItemResourceSupport
    {
        internal static GameObject GetPrefab(Tag tag)
        {
            if (!tag.IsValid || tag == GameTags.Void)
                return null;

            Element element = ElementLoader.GetElement(tag);
            // 元素仍由原有固体分支处理，不能绕过禁用、物态或掉落物检查。
            if (element != null && element.tag == tag)
                return null;

            GameObject prefab = Assets.TryGetPrefab(tag);
            KPrefabID id = prefab?.GetComponent<KPrefabID>();
            PrimaryElement primary = prefab?.GetComponent<PrimaryElement>();
            // 已加载不代表当前模式可用：本体磁盘在眼冒金星中仍有预制体，
            // 但原版通过 HideFromSpawnTool 隐藏，研究设备也不接受它。
            if (id == null || id.PrefabTag != tag || id.HasTag(GameTags.HideFromSpawnTool) ||
                !DlcManager.IsCorrectDlcSubscribed(id.GetRequiredDlcIds(), id.GetForbiddenDlcIds()) || primary == null ||
                prefab.GetComponent<Pickupable>() == null ||
                !(primary.MassPerUnit > 0f) || float.IsInfinity(primary.MassPerUnit) ||
                prefab.GetComponent<Building>() != null || prefab.GetComponent<CreatureBrain>() != null ||
                prefab.GetComponent<MinionIdentity>() != null || prefab.GetComponent<Uprootable>() != null)
                return null;

            return prefab;
        }

        internal static Dictionary<Tag, HashSet<Tag>> GetOptions()
        {
            // 不缓存初始化中的结果，已加载 DLC 和其他 Mod 后注册的物品也可出现。
            var harvests = new HashSet<Tag>();
            foreach (GameObject plant in Assets.GetPrefabsWithComponent<Crop>())
            {
                Crop crop = plant.GetComponent<Crop>();
                if (crop != null && crop.cropVal.numProduced > 0 && !string.IsNullOrEmpty(crop.cropVal.cropId))
                    harvests.Add(new Tag(crop.cropVal.cropId));
            }

            var options = new Dictionary<Tag, HashSet<Tag>>();
            foreach (GameObject prefab in Assets.GetPrefabsWithComponent<Pickupable>())
            {
                KPrefabID id = prefab?.GetComponent<KPrefabID>();
                if (id == null || GetPrefab(id.PrefabTag) == null)
                    continue;

                // 与食物、植物生成器保持重叠物品的分类一致，其余沿用原版分类。
                Tag category = prefab.GetComponent<Edible>() != null || id.HasTag(GameTags.CookingIngredient)
                    ? GameTags.CookingIngredient
                    : harvests.Contains(id.PrefabTag) ? GameTags.Other : DiscoveredResources.GetCategoryForEntity(id);
                if (!category.IsValid || category == GameTags.Void)
                    category = GameTags.Other;
                if (!options.TryGetValue(category, out var items))
                {
                    items = new HashSet<Tag>();
                    options.Add(category, items);
                }
                items.Add(id.PrefabTag);
            }
            return options;
        }
    }
}
