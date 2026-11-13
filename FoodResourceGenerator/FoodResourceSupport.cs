using System.Collections.Generic;
using UnityEngine;

namespace FoodResourceGenerator
{
    internal static class FoodResourceSupport
    {
        private static HashSet<Tag> ingredients;

        internal static HashSet<Tag> GetIngredients()
        {
            if (ingredients != null)
                return ingredients;

            var recipes = ComplexRecipeManager.Get()?.recipes;
            if (recipes == null)
                return new HashSet<Tag>();

            var loadedIngredients = new HashSet<Tag>();
            foreach (ComplexRecipe recipe in recipes)
            {
                if (recipe?.ingredients == null || recipe.results == null || !ProducesFood(recipe))
                    continue;

                foreach (ComplexRecipe.RecipeElement ingredient in recipe.ingredients)
                    if (ingredient != null && IsFoodItem(Assets.TryGetPrefab(ingredient.material)))
                        loadedIngredients.Add(ingredient.material);
            }
            // 游戏初始化早期可能尚无食物预制体，空结果不缓存，之后会重新发现。
            if (loadedIngredients.Count > 0)
                ingredients = loadedIngredients;
            return loadedIngredients;
        }

        internal static GameObject GetPrefab(Tag tag)
        {
            if (!GetIngredients().Contains(tag))
                return null;

            GameObject prefab = Assets.TryGetPrefab(tag);
            var primaryElement = prefab?.GetComponent<PrimaryElement>();
            // 列表与生产共用检查，配方或预制体变化后已保存的无效选择会安全停机。
            return prefab != null && prefab.GetComponent<KPrefabID>()?.PrefabTag == tag &&
                IsFoodItem(prefab) && prefab.GetComponent<Pickupable>() != null && primaryElement != null &&
                primaryElement.MassPerUnit > 0f && !float.IsInfinity(primaryElement.MassPerUnit)
                ? prefab : null;
        }

        private static bool ProducesFood(ComplexRecipe recipe)
        {
            foreach (ComplexRecipe.RecipeElement result in recipe.results)
                if (result != null && IsFoodItem(Assets.TryGetPrefab(result.material)))
                    return true;
            return false;
        }

        private static bool IsFoodItem(GameObject prefab)
        {
            var prefabId = prefab?.GetComponent<KPrefabID>();
            return prefabId != null &&
                (prefab.GetComponent<Edible>() != null || prefabId.HasTag(GameTags.CookingIngredient));
        }
    }
}
