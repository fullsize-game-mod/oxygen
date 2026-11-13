using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KSerialization;
using UnityEngine;
using Generator = FoodResourceGenerator.FoodResourceGenerator;
using Support = FoodResourceGenerator.FoodResourceSupport;

static void Check(bool condition, string message)
{
    if (!condition)
        throw new Exception(message);
}

static bool Near(float actual, float expected) => Math.Abs(actual - expected) < 0.0001f;

static GameObject AddPrefab(string id, float massPerUnit = 1f, bool edible = true, bool cookingIngredient = false)
{
    var prefab = new GameObject();
    var prefabId = new KPrefabID { PrefabTag = new Tag(id) };
    if (cookingIngredient)
        prefabId.Tags.Add(GameTags.CookingIngredient);
    prefab.Components.Add(prefabId);
    prefab.Components.Add(new PrimaryElement { MassPerUnit = massPerUnit, Units = 1f, Temperature = 300f });
    prefab.Components.Add(new Pickupable());
    if (edible)
        prefab.Components.Add(new Edible());
    Assets.Prefabs[new Tag(id)] = prefab;
    return prefab;
}

var meat = AddPrefab("Meat");
var egg = AddPrefab("RawEgg");
var wheat = AddPrefab("ColdWheatSeed", 0.001f);
AddPrefab("CookedMeat");
AddPrefab("CookedEgg");
AddPrefab("ColdWheatBread");
AddPrefab("SurfAndTurf");
AddPrefab("ModMeal");
AddPrefab("FinalFood");
AddPrefab("OrphanFood");
AddPrefab("ModSpice", edible: false, cookingIngredient: true);
AddPrefab("HatchEgg", edible: false);
AddPrefab("Water", edible: false);
AddPrefab("Plastic", edible: false);

Check(Support.GetIngredients().Count == 0, "配方尚未注册时不能永久缓存空列表");
ComplexRecipeManager.Get().recipes.AddRange(new[]
{
    new ComplexRecipe(new[] { "Meat" }, new[] { "CookedMeat" }),
    new ComplexRecipe(new[] { "RawEgg" }, new[] { "CookedEgg" }),
    new ComplexRecipe(new[] { "ColdWheatSeed", "Water" }, new[] { "ColdWheatBread" }),
    new ComplexRecipe(new[] { "CookedMeat", "FishMeat" }, new[] { "SurfAndTurf" }),
    new ComplexRecipe(new[] { "ModSpice" }, new[] { "ModMeal" }),
    new ComplexRecipe(new[] { "HatchEgg" }, new[] { "RawEgg" }),
    new ComplexRecipe(new[] { "Meat" }, new[] { "Plastic" }),
    new ComplexRecipe(Array.Empty<string>(), new[] { "FinalFood" }),
    new ComplexRecipe(null, new[] { "FinalFood" }),
    new ComplexRecipe(new[] { "Meat" }, null),
    null
});

var operational = new Operational();
var filter = new Filterable { SelectedTag = new Tag("Meat") };
var generator = new Generator();
generator.Initialize(operational, filter);
filter.Initialize(generator);

generator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == 0 && !operational.IsActive, "断电时必须待机");
operational.IsOperational = true;
for (int tick = 0; tick < 5; tick++)
    generator.Sim200ms(0.2f);
Check(Near(GameUtil.Spawned.Sum(food => food.GetComponent<PrimaryElement>().Mass), 1f) && operational.IsActive,
    "默认通电一秒必须生产 1 kg");
Check(GameUtil.Spawned.All(food => food.GetComponent<KPrefabID>().PrefabTag == new Tag("Meat") &&
    food.Active && food.GetComponent<PrimaryElement>().Temperature == 293.15f &&
    food.GetComponent<PrimaryElement>().DiseaseCount == 0), "必须生成已激活、20°C、无菌的真正肉物品");
Check(meat.GetComponent<PrimaryElement>().Mass == 1f && meat.GetComponent<PrimaryElement>().Temperature == 300f &&
    !meat.Active, "生产不能修改或激活资源预制体");

filter.SelectedTag = new Tag("RawEgg");
generator.SetSliderValue(2.5f, 0);
generator.Sim200ms(0.2f);
var output = GameUtil.Spawned.Last();
Check(output.GetComponent<KPrefabID>().PrefabTag == new Tag("RawEgg") &&
    Near(output.GetComponent<PrimaryElement>().Mass, 0.5f), "切换到生蛋液后只生产生蛋液");
Check(Near(GameUtil.Spawned.First().GetComponent<PrimaryElement>().Mass, 0.2f), "切换原料不能影响已有产物");

filter.SelectedTag = new Tag("ColdWheatSeed");
generator.SetSliderValue(1f, 0);
generator.Sim200ms(0.2f);
var grain = GameUtil.Spawned.Last().GetComponent<PrimaryElement>();
Check(Near(grain.Mass, 0.2f) && Near(grain.Units, 200f), "谷粒必须按自身单位质量换算，不能把千克误当单位数");

int baseline = GameUtil.Spawned.Count;
operational.IsOperational = false;
generator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == baseline && !operational.IsActive, "运行中断电或自动化禁止运行必须立即停止");
operational.IsOperational = true;
generator.SetSliderValue(0f, 0);
generator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == baseline && !operational.IsActive, "零速必须待机");
generator.SetSliderValue(1f, 0);
foreach (float dt in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
    generator.Sim200ms(dt);
Check(GameUtil.Spawned.Count == baseline && !operational.IsActive, "异常时间步不得生成食物");
foreach (string id in new[] { "Void", "MissingDlcIngredient", "FishMeat", "HatchEgg", "Water", "OrphanFood", "FinalFood" })
{
    filter.SelectedTag = new Tag(id);
    generator.Sim200ms(0.2f);
    Check(GameUtil.Spawned.Count == baseline && !operational.IsActive, "无效、未加载或非生原料选项必须停机：" + id);
}
filter.SelectedTag = new Tag("Meat");
Grid.Valid = false;
generator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == baseline && !operational.IsActive, "无效格子不能生成食物");
Grid.Valid = true;

foreach (float value in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
{
    meat.GetComponent<PrimaryElement>().MassPerUnit = value;
    generator.Sim200ms(0.2f);
    Check(Support.GetPrefab(new Tag("Meat")) == null && GameUtil.Spawned.Count == baseline && !operational.IsActive,
        "异常单位质量必须同时从菜单和生产中排除");
}
meat.GetComponent<PrimaryElement>().MassPerUnit = 1f;
meat.GetComponent<KPrefabID>().PrefabTag = new Tag("Iron");
Check(Support.GetPrefab(new Tag("Meat")) == null, "不能生成错配的预制体");
meat.GetComponent<KPrefabID>().PrefabTag = new Tag("Meat");
foreach (Type componentType in new[] { typeof(KPrefabID), typeof(PrimaryElement), typeof(Pickupable), typeof(Edible) })
{
    object component = meat.Components.Single(item => item.GetType() == componentType);
    meat.Components.Remove(component);
    Check(Support.GetPrefab(new Tag("Meat")) == null, "缺少必要组件的物品不能生成");
    meat.Components.Add(component);
}

Check(generator.GetSliderMin(0) == 0f && generator.GetSliderMax(0) == 100f &&
    generator.SliderUnits == "kg/s" && generator.SliderDecimalPlaces(0) == 1, "滑块范围、单位与精度必须沿用固体生成器");
generator.SetSliderValue(-1f, 0);
Check(generator.GetSliderValue(0) == 0f, "负速度必须钳制到 0");
generator.SetSliderValue(1000f, 0);
generator.Sim200ms(0.2f);
Check(generator.GetSliderValue(0) == 100f && Near(GameUtil.Spawned.Last().GetComponent<PrimaryElement>().Mass, 20f),
    "最大速度必须限制为 100 kg/s");
foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
{
    generator.SetSliderValue(value, 0);
    Check(generator.GetSliderValue(0) == 1f, "异常速度必须回退为默认值");
}
generator.SetSliderValue(0.1f, 0);
generator.Sim200ms(0.2f);
Check(Near(GameUtil.Spawned.Last().GetComponent<PrimaryElement>().Mass, 0.02f), "必须支持小数速度");
var second = new Generator();
second.Initialize(new Operational(), new Filterable());
second.Trigger((int)GameHashes.CopySettings, generator.gameObject);
Check(second.GetSliderValue(0) == 0.1f, "复制设置必须包含速度");
second.SetSliderValue(4f, 0);
Check(generator.GetSliderValue(0) == 0.1f, "建筑设置必须独立");
var rateField = typeof(Generator).GetField("kilogramsPerSecond", BindingFlags.Instance | BindingFlags.NonPublic);
Check(rateField.IsDefined(typeof(Serialize)), "速度必须标记为存档字段");
rateField.SetValue(second, 7.5f);
second.Spawn();
Check(second.GetSliderValue(0) == 7.5f, "读档后保留有效速度");
rateField.SetValue(second, float.NaN);
second.Spawn();
Check(second.GetSliderValue(0) == 1f, "读档后修复异常速度");

var originalOptions = new Dictionary<Tag, HashSet<Tag>>
{
    [GameTags.Other] = new HashSet<Tag> { GameTags.Void, new Tag("Water"), new Tag("CookedMeat") }
};
var options = originalOptions;
var regularFilter = new Filterable();
regularFilter.Initialize();
FoodResourceGenerator.GeneratorFoodOptionsPatch.Postfix(regularFilter, ref options);
Check(ReferenceEquals(options, originalOptions), "普通筛选器不受影响");
FoodResourceGenerator.GeneratorFoodOptionsPatch.Postfix(filter, ref options);
Check(options[GameTags.CookingIngredient].SetEquals(new[]
    {
        new Tag("Meat"), new Tag("RawEgg"), new Tag("ColdWheatSeed"), new Tag("CookedMeat"), new Tag("ModSpice")
    }) &&
    options[GameTags.Other].SetEquals(new[] { GameTags.Void }) && options.Count == 2,
    "必须从原版和 Mod 食物配方发现物品原料，包含中间食材，排除元素、动物蛋、最终成品和缺失 DLC");
options[GameTags.Other].Clear();
Check(originalOptions[GameTags.Other].Count == 3, "菜单不能修改原版共享的分类集合");
Dictionary<Tag, HashSet<Tag>> emptyOptions = null;
FoodResourceGenerator.GeneratorFoodOptionsPatch.Postfix(filter, ref emptyOptions);
Check(emptyOptions[GameTags.Other].Contains(GameTags.Void) && emptyOptions[GameTags.CookingIngredient].Contains(new Tag("Meat")),
    "原版列表为空时也要提供清空入口和未发现原料");

bool validTarget = false;
var screen = new FilterSideScreen();
FoodResourceGenerator.GeneratorResourceSelectionPatch.Postfix(screen, generator.gameObject, ref validTarget);
Check(validTarget, "本建筑必须显示单选侧栏");
validTarget = false;
screen.isLogicFilter = true;
FoodResourceGenerator.GeneratorResourceSelectionPatch.Postfix(screen, generator.gameObject, ref validTarget);
Check(!validTarget, "不能为本建筑启用逻辑过滤侧栏");
screen.isLogicFilter = false;
FoodResourceGenerator.GeneratorResourceSelectionPatch.Postfix(screen, regularFilter.gameObject, ref validTarget);
Check(!validTarget, "不能改动其他建筑的侧栏规则");
Assets.Prefabs.Clear();
FoodResourceGenerator.GeneratorFoodOptionsPatch.Postfix(filter, ref emptyOptions);
Check(emptyOptions.Count == 1 && emptyOptions[GameTags.Other].Contains(GameTags.Void), "没有可用原料时不能留下空分类");

Console.WriteLine("PASS: recipe discovery, mod ingredients, item filtering, food identity, kg/unit conversion, power, rates, settings, save validation, invalid prefabs, scoped sidebar and shared-data isolation.");

// 仅替代游戏边界，执行正式生产组件和补丁；不模拟腐败、物理或真实存档系统。
public class KMonoBehaviour
{
    private object[] components;
    private readonly Dictionary<int, Action<object>> handlers = new Dictionary<int, Action<object>>();
    public Transform transform = new Transform();
    public GameObject gameObject = new GameObject();
    protected virtual void OnPrefabInit() { }
    protected virtual void OnSpawn() { }
    public T GetComponent<T>() => (T)Array.Find(components, item => item is T);
    public void Subscribe(int hash, Action<object> handler) => handlers[hash] = handler;
    public void Trigger(int hash, object data) => handlers[hash](data);
    public void Spawn() => OnSpawn();
    public void Initialize(params object[] values)
    {
        components = values; gameObject.Components.Add(this); OnPrefabInit(); OnSpawn();
    }
}
public interface ISim200ms { void Sim200ms(float dt); }
public interface ISingleSliderControl
{
    string SliderTitleKey { get; }
    string SliderUnits { get; }
    int SliderDecimalPlaces(int index);
    float GetSliderMin(int index);
    float GetSliderMax(int index);
    float GetSliderValue(int index);
    void SetSliderValue(float value, int index);
    string GetSliderTooltipKey(int index);
    string GetSliderTooltip(int index);
}
public enum GameHashes { CopySettings }
public class Operational
{
    public bool IsOperational, IsActive;
    public void SetActive(bool value) => IsActive = value;
}
public readonly record struct Tag(string Name);
public static class GameTags
{
    public static readonly Tag Void = new Tag("Void"), Other = new Tag("Other"), CookingIngredient = new Tag("CookingIngredient");
}
public class Filterable : KMonoBehaviour
{
    public Tag SelectedTag;
    public Dictionary<Tag, HashSet<Tag>> GetTagOptions() => throw new NotImplementedException();
}
public class KPrefabID
{
    public Tag PrefabTag;
    public readonly HashSet<Tag> Tags = new HashSet<Tag>();
    public bool HasTag(Tag tag) => Tags.Contains(tag);
}
public class Pickupable { }
public class Edible { }
public class PrimaryElement
{
    public float MassPerUnit = 1f, Units, Temperature;
    public int DiseaseCount;
    public float Mass { get => Units * MassPerUnit; set => Units = value / MassPerUnit; }
}
public static class Assets
{
    public static readonly Dictionary<Tag, GameObject> Prefabs = new Dictionary<Tag, GameObject>();
    public static GameObject TryGetPrefab(Tag tag) => Prefabs.TryGetValue(tag, out var prefab) ? prefab : null;
}
public class ComplexRecipe
{
    public RecipeElement[] ingredients, results;
    public ComplexRecipe(string[] ingredientIds, string[] resultIds)
    {
        ingredients = ingredientIds?.Select(id => new RecipeElement(new Tag(id))).ToArray();
        results = resultIds?.Select(id => new RecipeElement(new Tag(id))).ToArray();
    }
    public class RecipeElement
    {
        public Tag material;
        public RecipeElement(Tag material) => this.material = material;
    }
}
public class ComplexRecipeManager
{
    private static readonly ComplexRecipeManager Instance = new ComplexRecipeManager();
    public readonly List<ComplexRecipe> recipes = new List<ComplexRecipe>();
    public static ComplexRecipeManager Get() => Instance;
}
public static class GameUtil
{
    public static readonly List<GameObject> Spawned = new List<GameObject>();
    public static GameObject KInstantiate(GameObject prefab, Vector3 position, Grid.SceneLayer layer)
    {
        var source = prefab.GetComponent<PrimaryElement>();
        var food = new GameObject();
        food.Components.Add(new KPrefabID { PrefabTag = prefab.GetComponent<KPrefabID>().PrefabTag });
        food.Components.Add(new PrimaryElement { MassPerUnit = source.MassPerUnit, Units = source.Units,
            Temperature = source.Temperature, DiseaseCount = source.DiseaseCount });
        food.Components.Add(new Pickupable());
        Spawned.Add(food);
        return food;
    }
}
public static class Grid
{
    public enum SceneLayer { Ore }
    public static bool Valid = true;
    public static int PosToCell(Vector3 position) => 0;
    public static bool IsValidCell(int cell) => Valid;
}
public static class GeneratedBuildings { public static void LoadGeneratedBuildings() { } }
public static class ModUtil { public static void AddBuildingToPlanScreen(string category, string id) { } }
public class FilterSideScreen
{
    public bool isLogicFilter;
    public bool IsValidForTarget(GameObject target) => false;
}
namespace FoodResourceGenerator { public static class FoodResourceGeneratorConfig { public const string ID = "FoodResourceGenerator"; } }
namespace UnityEngine
{
    public class GameObject
    {
        public readonly List<object> Components = new List<object>();
        public bool Active;
        public T GetComponent<T>() => (T)Components.Find(item => item is T);
        public void SetActive(bool value) => Active = value;
    }
    public class Transform { public Vector3 position; }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
}
namespace KSerialization
{
    public enum MemberSerialization { OptIn }
    public sealed class SerializationConfig : Attribute
    {
        public SerializationConfig(MemberSerialization memberSerialization) { }
    }
    public sealed class Serialize : Attribute { }
}
namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HarmonyPatch : Attribute
    {
        public HarmonyPatch(Type type, string methodName) { }
    }
}
