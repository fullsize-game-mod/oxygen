using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KSerialization;
using UnityEngine;
using Generator = PlantResourceGenerator.PlantResourceGenerator;
using Support = PlantResourceGenerator.PlantResourceSupport;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
static bool Near(float actual, float expected) => Math.Abs(actual - expected) < 0.0001f;
static GameObject Item(string id, float massPerUnit = 1f, bool edible = false, bool ingredient = false)
{
    var prefab = new GameObject();
    var identity = new KPrefabID { PrefabTag = new Tag(id) };
    if (ingredient) identity.Tags.Add(GameTags.CookingIngredient);
    prefab.Components.Add(identity);
    prefab.Components.Add(new PrimaryElement { MassPerUnit = massPerUnit, Units = 1f, Temperature = 300f });
    prefab.Components.Add(new Pickupable());
    if (edible) prefab.Components.Add(new Edible());
    Assets.Prefabs[new Tag(id)] = prefab;
    return prefab;
}
static Crop Plant(string id, string harvest, int count = 1)
{
    var crop = new Crop { cropVal = new Crop.CropVal { cropId = harvest, numProduced = count } };
    var plant = new GameObject();
    plant.Components.Add(crop);
    Assets.Prefabs[new Tag(id)] = plant;
    return crop;
}

Check(Support.GetHarvests().Count == 0, "初始化前没有作物时应为空");
var berry = Item("PrickleFruit", edible: true);
Item("ColdWheatSeed", 0.001f, ingredient: true);
Item("BeanPlantSeed", 0.01f, ingredient: true);
Item("SwampLilyFlower");
Item("WoodLog");
Item("BasicFabric");
Item("Meat", edible: true);
Item("CookedFood", edible: true);
Item("DecorativeSeed");
Item("ModHarvest", ingredient: true);
Item("LiquidSap");
Item("DisabledSolid");
Item("Void");
var berryCrop = Plant("PrickleGrass", "PrickleFruit");
Plant("SecondBerryPlant", "PrickleFruit", 5);
Plant("ColdWheat", "ColdWheatSeed", 18);
Plant("BeanPlant", "BeanPlantSeed", 12);
Plant("SwampLily", "SwampLilyFlower");
Plant("ArborTreeBranch", "WoodLog", 300);
Plant("FiberPlant", "BasicFabric", 2);
Plant("MissingDlcPlant", "MissingDlcHarvest");
Plant("NoHarvest", "");
Plant("NullHarvest", null);
Plant("ZeroYieldPlant", "Meat", 0);
Plant("NegativeYieldPlant", "CookedFood", -1);
Plant("LiquidPlant", "LiquidSap");
Plant("DisabledPlant", "DisabledSolid");
Plant("InvalidPlant", "Void");
ElementLoader.Elements[new Tag("WoodLog")] = new Element { tag = new Tag("WoodLog"), materialCategory = new Tag("Organics") };
ElementLoader.Elements[new Tag("LiquidSap")] = new Element { tag = new Tag("LiquidSap"), IsSolid = false };
ElementLoader.Elements[new Tag("DisabledSolid")] = new Element { tag = new Tag("DisabledSolid"), disabled = true };
// 普通物品可能映射到底层基础元素，不能因此被当作液体元素排除。
ElementLoader.Elements[new Tag("PrickleFruit")] = new Element { tag = new Tag("Water"), IsSolid = false };

var expected = new HashSet<Tag>(new[] { "PrickleFruit", "ColdWheatSeed", "BeanPlantSeed", "SwampLilyFlower", "WoodLog", "BasicFabric" }.Select(id => new Tag(id)));
Check(Support.GetHarvests().SetEquals(expected), "只列出实际加载且可拾取的作物收获物，去重并包含非食用产物及收获种子");
var modCrop = Plant("ModPlant", "ModHarvest");
Check(Support.GetHarvests().Contains(new Tag("ModHarvest")), "初始化后注册的 Mod 作物必须可见");
modCrop.cropVal.cropId = "MissingDlcHarvest";
Check(Support.GetPrefab(new Tag("ModHarvest")) == null, "作物配置移除产物后不能保留过期缓存");
modCrop.cropVal.cropId = "ModHarvest";
expected.Add(new Tag("ModHarvest"));

var operational = new Operational();
var filter = new Filterable { SelectedTag = new Tag("PrickleFruit") };
var generator = new Generator();
generator.Initialize(operational, filter);
filter.Initialize(generator);
generator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == 0 && !operational.IsActive, "断电必须待机");
operational.IsOperational = true;
for (int i = 0; i < 5; i++) generator.Sim200ms(0.2f);
Check(Near(GameUtil.Spawned.Sum(item => item.GetComponent<PrimaryElement>().Mass), 1f) && operational.IsActive,
    "默认每秒必须生产 1 kg");
Check(GameUtil.Spawned.All(item => item.Active && item.GetComponent<KPrefabID>().PrefabTag == new Tag("PrickleFruit") &&
    item.GetComponent<PrimaryElement>().Temperature == 293.15f), "必须生成已激活的 20°C 真正收获物");
Check(berry.GetComponent<PrimaryElement>().Mass == 1f && berry.GetComponent<PrimaryElement>().Temperature == 300f && !berry.Active,
    "生成不能修改或激活原预制体");
generator.SetSliderValue(2.5f, 0);
filter.SelectedTag = new Tag("SwampLilyFlower");
generator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Last().GetComponent<KPrefabID>().PrefabTag == new Tag("SwampLilyFlower") &&
    Near(GameUtil.Spawned.Last().GetComponent<PrimaryElement>().Mass, 0.5f), "可切换到非食用收获物，按质量计产");
Check(Near(GameUtil.Spawned.First().GetComponent<PrimaryElement>().Mass, 0.2f), "切换不影响既有产物");
generator.SetSliderValue(1f, 0);
filter.SelectedTag = new Tag("ColdWheatSeed");
generator.Sim200ms(0.2f);
Check(Near(GameUtil.Spawned.Last().GetComponent<PrimaryElement>().Units, 200f), "谷粒必须使用其自身单位质量换算");
filter.SelectedTag = new Tag("BeanPlantSeed");
generator.Sim200ms(0.2f);
Check(Near(GameUtil.Spawned.Last().GetComponent<PrimaryElement>().Units, 20f), "豆类不能被普通种子排除规则误伤");
filter.SelectedTag = new Tag("WoodLog");
ElementLoader.Elements[filter.SelectedTag].highTemp = 280f;
generator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Last().GetComponent<PrimaryElement>().Temperature == 279f, "低熔点固体产物必须在熔点以下生成");

int baseline = GameUtil.Spawned.Count;
operational.IsOperational = false;
generator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == baseline && !operational.IsActive, "断电或自动化禁止运行必须停止");
operational.IsOperational = true;
generator.SetSliderValue(0f, 0);
generator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == baseline && !operational.IsActive, "零速必须待机");
generator.SetSliderValue(1f, 0);
foreach (float dt in new[] { 0f, -1f, float.NaN, float.PositiveInfinity }) generator.Sim200ms(dt);
Check(GameUtil.Spawned.Count == baseline && !operational.IsActive, "异常时间步不得生成产物");
foreach (string id in new[] { "Void", "MissingDlcHarvest", "Meat", "CookedFood", "DecorativeSeed", "LiquidSap", "DisabledSolid", "ArborTreeBranch" })
{
    filter.SelectedTag = new Tag(id);
    generator.Sim200ms(0.2f);
    Check(GameUtil.Spawned.Count == baseline && !operational.IsActive, "无效或非收获物不能生产：" + id);
}
filter.SelectedTag = new Tag("PrickleFruit");
Grid.Valid = false;
generator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == baseline && !operational.IsActive, "无效格子不能生成");
Grid.Valid = true;
foreach (float mass in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
{
    berry.GetComponent<PrimaryElement>().MassPerUnit = mass;
    generator.Sim200ms(0.2f);
    Check(!Support.GetHarvests().Contains(filter.SelectedTag) && GameUtil.Spawned.Count == baseline && !operational.IsActive,
        "无效单位质量必须同时从菜单和生产排除");
}
berry.GetComponent<PrimaryElement>().MassPerUnit = 1f;
foreach (Type componentType in new[] { typeof(KPrefabID), typeof(PrimaryElement), typeof(Pickupable) })
{
    object component = berry.Components.Single(item => item.GetType() == componentType);
    berry.Components.Remove(component);
    Check(Support.GetPrefab(filter.SelectedTag) == null, "缺失必要组件必须安全排除");
    berry.Components.Add(component);
}
berry.GetComponent<KPrefabID>().PrefabTag = new Tag("Meat");
Check(Support.GetPrefab(filter.SelectedTag) == null, "错配预制体不能生产");
berry.GetComponent<KPrefabID>().PrefabTag = filter.SelectedTag;
Assets.Prefabs.Remove(filter.SelectedTag);
generator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == baseline && !operational.IsActive, "读档后预制体被移除必须停机");
Assets.Prefabs[filter.SelectedTag] = berry;
Check(generator.GetSliderMin(0) == 0f && generator.GetSliderMax(0) == 100f &&
    generator.SliderUnits == "kg/s" && generator.SliderDecimalPlaces(0) == 1, "沿用固体生成器速度范围和精度");
generator.SetSliderValue(-1f, 0);
Check(generator.GetSliderValue(0) == 0f, "负速度钳制为 0");
generator.SetSliderValue(1000f, 0);
generator.Sim200ms(0.2f);
Check(generator.GetSliderValue(0) == 100f && Near(GameUtil.Spawned.Last().GetComponent<PrimaryElement>().Mass, 20f), "上限 100 kg/s");
foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
{
    generator.SetSliderValue(value, 0);
    Check(generator.GetSliderValue(0) == 1f, "异常速度回退默认值");
}
generator.SetSliderValue(0.1f, 0);
generator.Sim200ms(0.2f);
Check(Near(GameUtil.Spawned.Last().GetComponent<PrimaryElement>().Mass, 0.02f), "小数速度必须精确生产");
var second = new Generator();
second.Initialize(new Operational(), new Filterable());
second.Trigger((int)GameHashes.CopySettings, generator.gameObject);
Check(second.GetSliderValue(0) == 0.1f, "复制建筑设置包含速度");
second.SetSliderValue(4f, 0);
Check(generator.GetSliderValue(0) == 0.1f, "建筑间设置独立");
var rateField = typeof(Generator).GetField("kilogramsPerSecond", BindingFlags.Instance | BindingFlags.NonPublic);
Check(rateField.IsDefined(typeof(Serialize)), "速度必须标记为存档字段");
rateField.SetValue(second, 7.5f);
second.Spawn();
Check(second.GetSliderValue(0) == 7.5f, "读档后保留有效速度");
rateField.SetValue(second, float.NaN);
second.Spawn();
Check(second.GetSliderValue(0) == 1f, "读档后修复异常速度");

var original = new Dictionary<Tag, HashSet<Tag>> { [GameTags.Other] = new HashSet<Tag> { GameTags.Void, new Tag("Meat") } };
var options = original;
var regular = new Filterable();
regular.Initialize();
PlantResourceGenerator.GeneratorPlantOptionsPatch.Postfix(regular, ref options);
Check(ReferenceEquals(options, original), "不能影响普通筛选器");
PlantResourceGenerator.GeneratorPlantOptionsPatch.Postfix(filter, ref options);
Check(options.Values.SelectMany(tags => tags).Where(tag => tag != GameTags.Void).ToHashSet().SetEquals(expected), "侧栏包含全部有效收获物");
Check(options[GameTags.Void].SetEquals(new[] { GameTags.Void }) && !options[GameTags.Other].Contains(GameTags.Void), "无选择行只能位于保留分类");
Check(options[GameTags.CookingIngredient].Contains(new Tag("PrickleFruit")) && options[new Tag("Organics")].Contains(new Tag("WoodLog")),
    "重叠收获物分类必须与食物、固体生成器一致");
options[GameTags.Void].Clear();
Check(original[GameTags.Other].Count == 2, "菜单不能修改原版共享集合");
Dictionary<Tag, HashSet<Tag>> fromNull = null;
PlantResourceGenerator.GeneratorPlantOptionsPatch.Postfix(filter, ref fromNull);
Check(fromNull.ContainsKey(GameTags.Void), "原列表为空时仍可生成菜单");
var screen = new FilterSideScreen();
bool valid = false;
PlantResourceGenerator.GeneratorResourceSelectionPatch.Postfix(screen, generator.gameObject, ref valid);
Check(valid, "生成器应显示单选侧栏");
screen.isLogicFilter = true;
valid = false;
PlantResourceGenerator.GeneratorResourceSelectionPatch.Postfix(screen, generator.gameObject, ref valid);
Check(!valid, "不能启用逻辑筛选侧栏");
screen.isLogicFilter = false;
PlantResourceGenerator.GeneratorResourceSelectionPatch.Postfix(screen, null, ref valid);
PlantResourceGenerator.GeneratorResourceSelectionPatch.Postfix(screen, new GameObject(), ref valid);
Check(!valid, "不影响空目标和普通建筑");
Assets.Prefabs.Clear();
PlantResourceGenerator.GeneratorPlantOptionsPatch.Postfix(filter, ref fromNull);
Check(fromNull.Count == 1 && fromNull.ContainsKey(GameTags.Void), "无作物时不显示空资源分类");
Console.WriteLine("PASS: harvest discovery and live changes; food, nonfood, branches and harvest seeds; exclusions; identity and mass conversion; rate and operation; copy/save fields; menu isolation and matching categories.");

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
public readonly record struct Tag(string Name) { public bool IsValid => !string.IsNullOrEmpty(Name); }
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
    public static List<GameObject> GetPrefabsWithComponent<T>() => Prefabs.Values.Where(prefab => prefab.GetComponent<T>() != null).ToList();
}
public class Crop
{
    public CropVal cropVal;
    public struct CropVal { public string cropId; public int numProduced; }
}
public class Element { public Tag tag, materialCategory; public bool IsSolid = true, disabled; public float highTemp = 1000f; }
public static class ElementLoader
{
    public static readonly Dictionary<Tag, Element> Elements = new Dictionary<Tag, Element>();
    public static Element GetElement(Tag tag) => Elements.TryGetValue(tag, out var element) ? element : null;
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
namespace PlantResourceGenerator { public static class PlantResourceGeneratorConfig { public const string ID = "PlantResourceGenerator"; } }
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
