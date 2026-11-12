using System;
using System.Collections.Generic;
using System.Reflection;
using KSerialization;
using UnityEngine;
using Generator = SolidResourceGenerator.SolidResourceGenerator;

static void Check(bool condition, string message)
{
    if (!condition)
        throw new Exception(message);
}

var operational = new Operational();
var filter = new Filterable();
var generator = new Generator();
generator.Initialize(operational, filter);
var iron = new Element { tag = new Tag("Iron"), materialCategory = new Tag("RefinedMetal"), IsSolid = true, highTemp = 1811f };
var ice = new Element { tag = new Tag("Ice"), materialCategory = new Tag("Liquifiable"), IsSolid = true, highTemp = 273.15f };
ElementLoader.Elements.Add(iron.tag, iron);
ElementLoader.Elements.Add(ice.tag, ice);

filter.SelectedTag = iron.tag;
generator.Sim200ms(0.2f);
Check(iron.substance.TotalMass == 0f && !operational.IsActive, "断电必须停止生产");

operational.IsOperational = true;
for (int tick = 0; tick < 5; tick++)
    generator.Sim200ms(0.2f);
Check(Math.Abs(iron.substance.TotalMass - 1f) < 0.0001f && operational.IsActive, "通电一秒必须生成 1 kg");
Check(iron.substance.Temperature == 293.15f, "普通固体以 20°C 生成");
Check(iron.substance.DiseaseIndex == byte.MaxValue && iron.substance.DiseaseCount == 0, "产物不能凭空携带病菌");

filter.SelectedTag = ice.tag;
generator.Sim200ms(0.2f);
Check(ice.substance.TotalMass == 0.2f && iron.substance.TotalMass == 1f, "切换后只能生成新选资源");
Check(ice.substance.Temperature < ice.highTemp, "冰不能在熔点以上生成");

operational.IsOperational = false;
generator.Sim200ms(0.2f);
Check(ice.substance.TotalMass == 0.2f && !operational.IsActive, "运行中断电必须停止");
operational.IsOperational = true;
foreach (float dt in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
    generator.Sim200ms(dt);
Check(ice.substance.TotalMass == 0.2f && !operational.IsActive, "无效时间步不得产生资源");

filter.SelectedTag = new Tag("MissingModElement");
generator.Sim200ms(0.2f);
Check(!operational.IsActive, "未选择或存档元素缺失时必须待机");
filter.SelectedTag = iron.tag;
iron.IsSolid = false;
generator.Sim200ms(0.2f);
Check(iron.substance.TotalMass == 1f && !operational.IsActive, "不能生成气体或液体");
iron.IsSolid = true;
iron.disabled = true;
generator.Sim200ms(0.2f);
Check(iron.substance.TotalMass == 1f && !operational.IsActive, "不能生成已禁用的元素");
iron.disabled = false;
Assets.HasPrefab = false;
generator.Sim200ms(0.2f);
Check(iron.substance.TotalMass == 1f && !operational.IsActive, "缺失预制体时必须待机");
Assets.HasPrefab = true;
Grid.Valid = false;
generator.Sim200ms(0.2f);
Check(iron.substance.TotalMass == 1f && !operational.IsActive, "不能向无效格子生成资源");

Grid.Valid = true;
var rateOperational = new Operational { IsOperational = true };
var rateGenerator = new Generator();
rateGenerator.Initialize(rateOperational, new Filterable { SelectedTag = iron.tag });
Check(rateGenerator.GetSliderValue(0) == 1f, "新建筑和旧存档缺省速度必须为 1 kg/s");
Check(rateGenerator.GetSliderMin(0) == 0f && rateGenerator.GetSliderMax(0) == 100f &&
    rateGenerator.SliderDecimalPlaces(0) == 1 && rateGenerator.SliderUnits == "kg/s", "滑块必须提供正确范围、精度和单位");
float baseline = iron.substance.TotalMass;
rateGenerator.SetSliderValue(2.5f, 0);
rateGenerator.Sim200ms(0.2f);
Check(Math.Abs(iron.substance.TotalMass - baseline - 0.5f) < 0.0001f && rateOperational.IsActive,
    "2.5 kg/s 必须在 200 ms 生成 0.5 kg");
Check(generator.GetSliderValue(0) == 1f, "修改速度不能影响其他建筑");

rateGenerator.SetSliderValue(0f, 0);
baseline = iron.substance.TotalMass;
rateGenerator.Sim200ms(0.2f);
Check(iron.substance.TotalMass == baseline && !rateOperational.IsActive, "速度为 0 时必须停产并待机");
rateGenerator.SetSliderValue(-10f, 0);
Check(rateGenerator.GetSliderValue(0) == 0f, "速度不能低于 0");
rateGenerator.SetSliderValue(1000f, 0);
rateGenerator.Sim200ms(0.2f);
Check(rateGenerator.GetSliderValue(0) == 100f && Math.Abs(iron.substance.TotalMass - baseline - 20f) < 0.0001f,
    "超出上限时必须按 100 kg/s 生产");
foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
{
    rateGenerator.SetSliderValue(value, 0);
    Check(rateGenerator.GetSliderValue(0) == 1f, "非有限速度必须回退到默认值");
}

rateGenerator.SetSliderValue(0.1f, 0);
baseline = iron.substance.TotalMass;
rateGenerator.Sim200ms(0.2f);
Check(Math.Abs(iron.substance.TotalMass - baseline - 0.02f) < 0.0001f, "必须支持小数速度");
generator.Trigger((int)GameHashes.CopySettings, rateGenerator.gameObject);
Check(generator.GetSliderValue(0) == 0.1f, "复制建筑设置必须包含速度");
generator.SetSliderValue(4f, 0);
Check(rateGenerator.GetSliderValue(0) == 0.1f, "复制设置后仍须独立调节");

var rateField = typeof(Generator).GetField("kilogramsPerSecond", BindingFlags.Instance | BindingFlags.NonPublic);
Check(rateField != null && rateField.IsDefined(typeof(Serialize)), "速度字段必须标记为随存档保存");
rateField.SetValue(rateGenerator, 7.5f);
rateGenerator.Spawn();
Check(rateGenerator.GetSliderValue(0) == 7.5f, "生成实例时必须保留读入的有效速度");
rateField.SetValue(rateGenerator, float.NaN);
rateGenerator.Spawn();
Check(rateGenerator.GetSliderValue(0) == 1f, "读档时必须修复无效速度");

// 原版列表只提供已发现的铁和“无选择”；其他元素没有发现或库存记录。
var originalOptions = new Dictionary<Tag, HashSet<Tag>>
{
    [GameTags.Other] = new HashSet<Tag> { GameTags.Void },
    [iron.materialCategory] = new HashSet<Tag> { iron.tag }
};
var options = originalOptions;
var regularFilter = new Filterable();
regularFilter.Initialize();
SolidResourceGenerator.GeneratorResourceOptionsPatch.Postfix(regularFilter, ref options);
Check(ReferenceEquals(options, originalOptions), "普通筛选器必须保留原版列表");

var unknownSolid = new Element { tag = new Tag("UnknownSolid"), materialCategory = iron.materialCategory, IsSolid = true, highTemp = 2000f };
var uncategorizedSolid = new Element { tag = new Tag("ModSolid"), IsSolid = true, highTemp = 1000f };
ElementLoader.Elements.Add(unknownSolid.tag, unknownSolid);
ElementLoader.Elements.Add(uncategorizedSolid.tag, uncategorizedSolid);
var invalidElements = new[]
{
    new Element { tag = new Tag("DisabledSolid"), IsSolid = true, highTemp = 1000f, disabled = true },
    new Element { tag = new Tag("Fluid"), IsSolid = false, highTemp = 1000f },
    new Element { tag = new Tag("NoSubstance"), IsSolid = true, highTemp = 1000f, substance = null },
    new Element { tag = new Tag("NoPrefab"), IsSolid = true, highTemp = 1000f },
    new Element { tag = new Tag("LowMeltingPoint"), IsSolid = true, highTemp = 1f },
    new Element { tag = new Tag("InvalidMeltingPoint"), IsSolid = true, highTemp = float.NaN }
};
foreach (var element in invalidElements)
    ElementLoader.Elements.Add(element.tag, element);
Assets.MissingPrefabs.Add(new Tag("NoPrefab"));

filter.Initialize(generator);
SolidResourceGenerator.GeneratorResourceOptionsPatch.Postfix(filter, ref options);
Check(options[iron.materialCategory].SetEquals(new[] { iron.tag, unknownSolid.tag }),
    "未发现、未拥有过的固体必须与已发现资源一起按原版类别显示");
Check(options[ice.materialCategory].SetEquals(new[] { ice.tag }), "未发现资源的新分类必须出现在列表中");
Check(options[GameTags.Other].SetEquals(new[] { GameTags.Void, uncategorizedSolid.tag }),
    "必须保留原版无选择项，缺少分类的 Mod 元素归入其他");
Check(options.Count == 3, "无效资源不能产生额外分类");
foreach (var element in invalidElements)
    foreach (var resources in options.Values)
        Check(!resources.Contains(element.tag), "不能选择无法生成的资源：" + element.tag.Name);
Check(originalOptions.Count == 2 && originalOptions[iron.materialCategory].SetEquals(new[] { iron.tag }) &&
    originalOptions[GameTags.Other].SetEquals(new[] { GameTags.Void }), "不能修改原版字典或共享集合中的发现记录");

filter.SelectedTag = unknownSolid.tag;
generator.SetSliderValue(1f, 0);
generator.Sim200ms(0.2f);
Check(Math.Abs(unknownSolid.substance.TotalMass - 0.2f) < 0.0001f && operational.IsActive,
    "未发现且从未持有过的资源必须能直接生产");

Console.WriteLine("PASS: power, adjustable rate, settings, temperature, invalid inputs, undiscovered solid selection and generation, categories, none option, vanilla filter isolation and shared-data isolation.");

// 只替代游戏边界，执行的是正式生产组件；不模拟 UI、存档和物理系统。
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
public class Element
{
    public Tag tag;
    public Tag materialCategory;
    public bool IsSolid;
    public bool disabled;
    public float highTemp;
    public Substance substance = new Substance();
}
public static class ElementLoader
{
    public static readonly Dictionary<Tag, Element> Elements = new Dictionary<Tag, Element>();
    public static IEnumerable<Element> elements => Elements.Values;
    public static Element GetElement(Tag tag) => Elements.TryGetValue(tag, out var element) ? element : null;
}
public class Substance
{
    public float TotalMass, Temperature;
    public byte DiseaseIndex;
    public int DiseaseCount;
    public GameObject SpawnResource(Vector3 position, float mass, float temperature, byte disease_idx, int disease_count)
    {
        TotalMass += mass; Temperature = temperature; DiseaseIndex = disease_idx; DiseaseCount = disease_count;
        return new GameObject();
    }
}
public static class Assets
{
    public static bool HasPrefab = true;
    public static readonly HashSet<Tag> MissingPrefabs = new HashSet<Tag>();
    public static GameObject TryGetPrefab(Tag tag) => HasPrefab && !MissingPrefabs.Contains(tag) ? new GameObject() : null;
}
public static class Grid
{
    public static bool Valid = true;
    public static int PosToCell(Vector3 position) => 0;
    public static bool IsValidCell(int cell) => Valid;
}
namespace UnityEngine
{
    public class GameObject
    {
        public object Component;
        public T GetComponent<T>() => Component is T value ? value : default;
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
