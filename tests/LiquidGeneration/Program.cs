using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KSerialization;
using UnityEngine;
using Generator = LiquidResourceGenerator.LiquidResourceGenerator;
using Support = LiquidResourceGenerator.LiquidResourceSupport;

static void Check(bool condition, string message)
{
    if (!condition)
        throw new Exception(message);
}

var water = new Element { tag = new Tag("Water"), id = SimHashes.Water, materialCategory = new Tag("Liquids"), IsLiquid = true, lowTemp = 273.15f, highTemp = 373.15f };
var oxygen = new Element { tag = new Tag("LiquidOxygen"), id = SimHashes.LiquidOxygen, materialCategory = new Tag("Liquids"), IsLiquid = true, lowTemp = 54.36f, highTemp = 90.19f };
var metal = new Element { tag = new Tag("MoltenIron"), id = SimHashes.MoltenIron, materialCategory = new Tag("Metals"), IsLiquid = true, lowTemp = 1811f, highTemp = 3134f };
foreach (var element in new[] { water, oxygen, metal })
    ElementLoader.Elements.Add(element.tag, element);

var operational = new Operational();
var filter = new Filterable { SelectedTag = water.tag };
var storage = new Storage { capacityKg = Generator.StorageCapacityKg };
var dispenser = new ConduitDispenser { IsConnected = true };
var generator = new Generator();
generator.Initialize(operational, filter, storage, dispenser);
generator.Sim200ms(0.2f);
Check(storage.MassStored() == 0f && !operational.IsActive, "断电必须停止生产");
operational.IsOperational = true;
for (int tick = 0; tick < 5; tick++)
    generator.Sim200ms(0.2f);
Check(Math.Abs(storage.MassStored() - 1f) < 0.0001f && operational.IsActive, "通电一秒必须生成 1 kg 到管道缓冲");
Check(storage.Outputs[SimHashes.Water].Temperature == 293.15f, "水应以 20°C 生成");
Check(storage.Outputs[SimHashes.Water].DiseaseIndex == byte.MaxValue && storage.Outputs[SimHashes.Water].DiseaseCount == 0, "产物不能凭空携带病菌");

dispenser.IsConnected = false;
generator.Sim200ms(0.2f);
Check(storage.MassStored() == 1f && !operational.IsActive, "未接液体管道时不生成，也不能丢弃缓冲");
dispenser.IsConnected = true;
dispenser.blocked = true;
generator.Sim200ms(0.2f);
Check(storage.MassStored() == 1f && !operational.IsActive, "管道堵塞时必须停产并保留液体");
dispenser.blocked = false;
generator.Sim200ms(0.2f);
Check(Math.Abs(storage.MassStored() - 1.2f) < 0.0001f && operational.IsActive, "管道恢复后必须继续生产");

filter.SelectedTag = oxygen.tag;
generator.Sim200ms(0.2f);
Check(storage.Outputs[SimHashes.LiquidOxygen].Mass == 0.2f && Math.Abs(storage.Outputs[SimHashes.Water].Mass - 1.2f) < 0.0001f, "切换资源只能改变新产物，不能删除旧液体");
Check(storage.Outputs[SimHashes.LiquidOxygen].Temperature < oxygen.highTemp && storage.Outputs[SimHashes.LiquidOxygen].Temperature > oxygen.lowTemp, "液氧必须在液态范围内生成");
filter.SelectedTag = metal.tag;
generator.Sim200ms(0.2f);
Check(storage.Outputs[SimHashes.MoltenIron].Temperature > metal.lowTemp && storage.Outputs[SimHashes.MoltenIron].Temperature < metal.highTemp, "熔融金属必须在液态范围内生成");

float baseline = storage.MassStored();
foreach (float dt in new[] { 0f, -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
    generator.Sim200ms(dt);
Check(storage.MassStored() == baseline && !operational.IsActive, "无效时间步不得产生液体");
filter.SelectedTag = new Tag("MissingModLiquid");
generator.Sim200ms(0.2f);
Check(storage.MassStored() == baseline && !operational.IsActive, "元素被移除后必须待机");

filter.SelectedTag = water.tag;
Check(generator.GetSliderValue(0) == 1f && generator.GetSliderMin(0) == 0f && generator.GetSliderMax(0) == 100f &&
    generator.SliderDecimalPlaces(0) == 1 && generator.SliderUnits == "kg/s", "滑块必须沿用默认速度、范围、精度和单位");
generator.SetSliderValue(2.5f, 0);
generator.Sim200ms(0.2f);
Check(Math.Abs(storage.MassStored() - baseline - 0.5f) < 0.0001f, "2.5 kg/s 必须在 200 ms 生成 0.5 kg");
baseline = storage.MassStored();
generator.SetSliderValue(0f, 0);
generator.Sim200ms(0.2f);
Check(storage.MassStored() == baseline && !operational.IsActive, "速度为 0 时必须暂停，不清空液体");
generator.SetSliderValue(-10f, 0);
Check(generator.GetSliderValue(0) == 0f, "速度不能低于 0");
generator.SetSliderValue(1000f, 0);
generator.Sim200ms(0.2f);
Check(generator.GetSliderValue(0) == 100f && storage.MassStored() == Generator.StorageCapacityKg, "最大速度不能超过缓冲剩余容量");
generator.Sim200ms(0.2f);
Check(storage.MassStored() == Generator.StorageCapacityKg && !operational.IsActive, "缓冲已满时必须停产");
// 用排空缓冲替代管道消耗，不模拟真实流体调度。
storage.Outputs.Clear();
generator.Sim200ms(0.2f);
Check(storage.MassStored() == 20f && operational.IsActive, "100 kg/s 应在 200 ms 生成 20 kg");
storage.Outputs.Clear();
foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
{
    generator.SetSliderValue(value, 0);
    Check(generator.GetSliderValue(0) == 1f, "无效速度必须回退到默认值");
}
generator.SetSliderValue(0.1f, 0);
generator.Sim200ms(0.2f);
Check(Math.Abs(storage.MassStored() - 0.02f) < 0.0001f, "必须支持小数速度");

var other = new Generator();
other.Initialize(new Operational(), new Filterable(), new Storage(), new ConduitDispenser());
Check(other.GetSliderValue(0) == 1f, "每台建筑的速度必须独立");
other.Trigger((int)GameHashes.CopySettings, generator.gameObject);
Check(other.GetSliderValue(0) == 0.1f, "复制设置必须包含速度");
other.SetSliderValue(4f, 0);
Check(generator.GetSliderValue(0) == 0.1f, "复制后必须仍可独立调速");
var rateField = typeof(Generator).GetField("kilogramsPerSecond", BindingFlags.Instance | BindingFlags.NonPublic);
Check(rateField != null && rateField.IsDefined(typeof(Serialize)), "速度字段必须标记保存");
rateField.SetValue(other, 7.5f);
other.Spawn();
Check(other.GetSliderValue(0) == 7.5f, "读档应保留有效速度");
rateField.SetValue(other, float.NaN);
other.Spawn();
Check(other.GetSliderValue(0) == 1f, "读档必须校验异常速度");

var unknown = new Element { tag = new Tag("UnknownLiquid"), id = SimHashes.UnknownLiquid, IsLiquid = true, lowTemp = 0f, highTemp = 500f };
ElementLoader.Elements.Add(unknown.tag, unknown);
var food = new Tag("FoodItem");
ElementLoader.Elements[food] = water;
var invalidElements = new[]
{
    new Element { tag = new Tag("Solid"), lowTemp = 1f, highTemp = 1000f },
    new Element { tag = new Tag("Disabled"), IsLiquid = true, disabled = true, highTemp = 1000f },
    new Element { tag = new Tag("NoSubstance"), IsLiquid = true, highTemp = 1000f, substance = null },
    new Element { tag = new Tag("NoPrefab"), IsLiquid = true, highTemp = 1000f },
    new Element { tag = new Tag("NarrowRange"), IsLiquid = true, lowTemp = 10f, highTemp = 11f },
    new Element { tag = new Tag("BelowAbsoluteZero"), IsLiquid = true, lowTemp = -10f, highTemp = 1f },
    new Element { tag = new Tag("NaNLow"), IsLiquid = true, lowTemp = float.NaN, highTemp = 1000f },
    new Element { tag = new Tag("NaNHigh"), IsLiquid = true, highTemp = float.NaN },
    new Element { tag = new Tag("InfiniteLow"), IsLiquid = true, lowTemp = float.NegativeInfinity, highTemp = 1000f },
    new Element { tag = new Tag("InfiniteHigh"), IsLiquid = true, highTemp = float.PositiveInfinity }
};
foreach (var element in invalidElements)
    ElementLoader.Elements.Add(element.tag, element);
Assets.MissingPrefabs.Add(new Tag("NoPrefab"));
foreach (Tag tag in invalidElements.Select(element => element.tag).Append(food))
{
    filter.SelectedTag = tag;
    baseline = storage.MassStored();
    generator.Sim200ms(0.2f);
    Check(!Support.IsSupported(tag) && !operational.IsActive && storage.MassStored() == baseline, "必须拒绝无效资源：" + tag.Name);
}

var originalOptions = new Dictionary<Tag, HashSet<Tag>>
{
    [GameTags.Other] = new HashSet<Tag> { GameTags.Void, food },
    [water.materialCategory] = new HashSet<Tag> { water.tag }
};
var options = originalOptions;
var regularFilter = new Filterable();
regularFilter.Initialize();
LiquidResourceGenerator.GeneratorLiquidOptionsPatch.Postfix(regularFilter, ref options);
Check(ReferenceEquals(options, originalOptions), "不能改变其他建筑的资源列表");
filter.Initialize(generator);
LiquidResourceGenerator.GeneratorLiquidOptionsPatch.Postfix(filter, ref options);
Check(options[water.materialCategory].SetEquals(new[] { water.tag, oxygen.tag }) &&
    options[metal.materialCategory].SetEquals(new[] { metal.tag }) &&
    options[GameTags.Other].SetEquals(new[] { GameTags.Void, unknown.tag }) && options.Count == 3, "必须列出未发现的有效液体、正确分类并保留无选择项");
Check(originalOptions[GameTags.Other].Contains(food) && originalOptions[water.materialCategory].Count == 1, "不能修改原版字典或共享集合");
options[water.materialCategory].Clear();
Check(originalOptions[water.materialCategory].Contains(water.tag), "新列表必须与发现记录独立");
Dictionary<Tag, HashSet<Tag>> emptyOptions = null;
LiquidResourceGenerator.GeneratorLiquidOptionsPatch.Postfix(filter, ref emptyOptions);
Check(emptyOptions[GameTags.Other].Contains(unknown.tag), "原列表为空时也应列出未发现液体");
filter.SelectedTag = unknown.tag;
generator.Sim200ms(0.2f);
Check(storage.Outputs[SimHashes.UnknownLiquid].Mass > 0f && operational.IsActive, "未发现液体必须可以直接生产");
Check(Support.GetOutputTemperature(unknown) >= 1f, "输出温度不能低于 1 K");

Console.WriteLine("PASS: power, pipe connection/blockage, bounded buffer, rates, settings, liquid temperatures, invalid inputs, undiscovered liquids, categories and shared-data isolation.");

// 仅替代游戏边界，执行正式组件和列表补丁；不模拟原版管网、UI 或存档序列化。
public class KMonoBehaviour
{
    private object[] components;
    private readonly Dictionary<int, Action<object>> handlers = new Dictionary<int, Action<object>>();
    public GameObject gameObject = new GameObject();
    protected virtual void OnPrefabInit() { }
    protected virtual void OnSpawn() { }
    public T GetComponent<T>() => (T)Array.Find(components, item => item is T);
    public void Subscribe(int hash, Action<object> handler) => handlers[hash] = handler;
    public void Trigger(int hash, object data) => handlers[hash](data);
    public void Spawn() => OnSpawn();
    public void Initialize(params object[] values)
    {
        components = values; gameObject.Component = this; OnPrefabInit(); OnSpawn();
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
    public bool IsOperational;
    public bool IsActive;
    public void SetActive(bool value) => IsActive = value;
}
public readonly record struct Tag(string Name)
{
    public bool IsValid => !string.IsNullOrEmpty(Name);
}
public static class GameTags
{
    public static readonly Tag Void = new Tag("Void");
    public static readonly Tag Other = new Tag("Other");
}
public class Filterable : KMonoBehaviour
{
    public Tag SelectedTag;
    public Dictionary<Tag, HashSet<Tag>> GetTagOptions() => throw new NotImplementedException();
}
public enum SimHashes { Water, LiquidOxygen, MoltenIron, UnknownLiquid }
public class Element
{
    public Tag tag;
    public SimHashes id;
    public Tag materialCategory;
    public bool IsLiquid;
    public bool disabled;
    public float lowTemp, highTemp;
    public object substance = new object();
}
public static class ElementLoader
{
    public static readonly Dictionary<Tag, Element> Elements = new Dictionary<Tag, Element>();
    public static IEnumerable<Element> elements => Elements.Values;
    public static Element GetElement(Tag tag) => Elements.TryGetValue(tag, out var element) ? element : null;
}
public class Storage
{
    public float capacityKg;
    public readonly Dictionary<SimHashes, Output> Outputs = new Dictionary<SimHashes, Output>();
    public float MassStored() => Outputs.Values.Sum(output => output.Mass);
    public void AddLiquid(SimHashes element, float mass, float temperature, byte disease_idx, int disease_count)
    {
        if (!Outputs.TryGetValue(element, out var output))
            Outputs.Add(element, output = new Output());
        output.Mass += mass; output.Temperature = temperature; output.DiseaseIndex = disease_idx; output.DiseaseCount = disease_count;
    }
    public class Output
    {
        public float Mass, Temperature;
        public byte DiseaseIndex;
        public int DiseaseCount;
    }
}
public class ConduitDispenser
{
    public bool IsConnected;
    public bool blocked;
}
public static class Assets
{
    public static readonly HashSet<Tag> MissingPrefabs = new HashSet<Tag>();
    public static GameObject TryGetPrefab(Tag tag) => MissingPrefabs.Contains(tag) ? null : new GameObject();
}
public static class GeneratedBuildings { public static void LoadGeneratedBuildings() { } }
public static class ModUtil { public static void AddBuildingToPlanScreen(string category, string id) { } }
public class FilterSideScreen
{
    public bool isLogicFilter;
    public bool IsValidForTarget(GameObject target) => false;
}
namespace LiquidResourceGenerator { public static class LiquidResourceGeneratorConfig { public const string ID = "LiquidResourceGenerator"; } }
namespace UnityEngine
{
    public class GameObject
    {
        public object Component;
        public T GetComponent<T>() => Component is T value ? value : default;
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
