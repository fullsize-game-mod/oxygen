using System;
using UnityEngine;

namespace FoodResourceGenerator
{
    internal static class FoodResourceSupport
    {
        // 明确限定生原料；成品菜肴和可孵化的动物蛋不在生产范围内。
        internal static readonly Tag[] Ingredients =
        {
            new Tag("Meat"), new Tag("FishMeat"), new Tag("ShellfishMeat"), new Tag("RawEgg"),
            new Tag("BasicPlantFood"), new Tag("Mushroom"), new Tag("Lettuce"), new Tag("PrickleFruit"),
            new Tag("ColdWheatSeed"), new Tag("SpiceNut"), new Tag("BeanPlantSeed"), new Tag("SwampFruit")
        };

        internal static GameObject GetPrefab(Tag tag)
        {
            if (Array.IndexOf(Ingredients, tag) < 0)
                return null;

            GameObject prefab = Assets.TryGetPrefab(tag);
            var primaryElement = prefab?.GetComponent<PrimaryElement>();
            // 列表与生产共用检查，缺失 DLC、错配预制体或异常单位质量时均拒绝生成。
            return prefab != null && prefab.GetComponent<KPrefabID>()?.PrefabTag == tag &&
                prefab.GetComponent<Pickupable>() != null && primaryElement != null &&
                primaryElement.MassPerUnit > 0f && !float.IsInfinity(primaryElement.MassPerUnit)
                ? prefab : null;
        }
    }
}
