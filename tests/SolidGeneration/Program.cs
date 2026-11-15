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

// 原版列表只发现了铁，并夹杂食物、种子等非元素选项。
var food = new Tag("CookedMeat");
var egg = new Tag("HatchEgg");
var seed = new Tag("Seed");
var foodCategory = new Tag("Food");
var eggCategory = new Tag("Eggs");
var liquidCategory = new Tag("Liquids");
// 物品即使能映射到基础元素，也不能按该元素生成。
ElementLoader.Elements[food] = iron;
var originalOptions = new Dictionary<Tag, HashSet<Tag>>
{
    [GameTags.Other] = new HashSet<Tag> { GameTags.Void },
    [iron.materialCategory] = new HashSet<Tag> { iron.tag, seed },
    [foodCategory] = new HashSet<Tag> { food },
    [eggCategory] = new HashSet<Tag> { egg },
    [liquidCategory] = new HashSet<Tag> { new Tag("Fluid") }
};
var options = originalOptions;
var regularFilter = new Filterable();
regularFilter.Initialize();
SolidResourceGenerator.GeneratorSolidOptionsPatch.Postfix(regularFilter, ref options);
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
SolidResourceGenerator.GeneratorSolidOptionsPatch.Postfix(filter, ref options);
Check(options[iron.materialCategory].SetEquals(new[] { iron.tag, unknownSolid.tag }),
    "未发现、未拥有过的固体必须与已发现资源一起按原版类别显示");
Check(options[ice.materialCategory].SetEquals(new[] { ice.tag }), "未发现资源的新分类必须出现在列表中");
Check(options[GameTags.Void].SetEquals(new[] { GameTags.Void }) &&
    options[GameTags.Other].SetEquals(new[] { uncategorizedSolid.tag }),
    "无选择项必须使用原版保留分类，缺少分类的 Mod 元素归入其他");
Check(options.Count == 4 && !options.ContainsKey(foodCategory) && !options.ContainsKey(eggCategory) &&
    !options.ContainsKey(liquidCategory), "菜单必须排除食物、蛋、种子、液体及空分类");
foreach (var element in invalidElements)
    foreach (var resources in options.Values)
        Check(!resources.Contains(element.tag), "不能选择无法生成的资源：" + element.tag.Name);
Check(originalOptions.Count == 5 && originalOptions[iron.materialCategory].SetEquals(new[] { iron.tag, seed }) &&
    originalOptions[GameTags.Other].SetEquals(new[] { GameTags.Void }), "不能修改原版字典或共享集合中的发现记录");
options[iron.materialCategory].Clear();
Check(originalOptions[iron.materialCategory].Contains(iron.tag), "返回的每个分类集合必须独立");
Dictionary<Tag, HashSet<Tag>> emptyOptions = null;
SolidResourceGenerator.GeneratorSolidOptionsPatch.Postfix(filter, ref emptyOptions);
Check(emptyOptions[iron.materialCategory].Contains(unknownSolid.tag), "原版列表为空时也必须列出未发现资源");

filter.SelectedTag = unknownSolid.tag;
generator.SetSliderValue(1f, 0);
generator.Sim200ms(0.2f);
Check(Math.Abs(unknownSolid.substance.TotalMass - 0.2f) < 0.0001f && operational.IsActive,
    "未发现且从未持有过的资源必须能直接生产");
filter.SelectedTag = food;
baseline = iron.substance.TotalMass;
generator.Sim200ms(0.2f);
Check(!operational.IsActive && iron.substance.TotalMass == baseline, "已保存的不支持物品选择也不能生成基础元素");

Console.WriteLine("PASS: power, adjustable rate, settings, temperature, invalid inputs, undiscovered solids, solid-only menu, exact element identity, categories, none option and shared-data isolation.");

GameObject Item(string name, float massPerUnit = 1f, params object[] extra)
{
    var prefab = new GameObject();
    prefab.Components.AddRange(new object[] { new KPrefabID { PrefabTag = new Tag(name) },
        new PrimaryElement { MassPerUnit = massPerUnit, Units = 1f, Temperature = 300f }, new Pickupable() });
    prefab.Components.AddRange(extra);
    Assets.Prefabs[new Tag(name)] = prefab;
    return prefab;
}

var disk = Item("OrbitalResearchDatabank", 2f);
disk.GetComponent<KPrefabID>().Category = new Tag("TechComponents");
disk.GetComponent<KPrefabID>().RequiredDlcIds = new[] { "EXPANSION1_ID" };
var baseDisk = Item("ResearchDatabank");
baseDisk.GetComponent<KPrefabID>().Category = new Tag("TechComponents");
baseDisk.GetComponent<KPrefabID>().Tags.Add(GameTags.HideFromSpawnTool);
Item("HiddenModItem").GetComponent<KPrefabID>().Tags.Add(GameTags.HideFromSpawnTool);
Item("MissingDlcItem").GetComponent<KPrefabID>().RequiredDlcIds = new[] { "MISSING_DLC" };
Item("ForbiddenDlcItem").GetComponent<KPrefabID>().ForbiddenDlcIds = new[] { "EXPANSION1_ID" };
var equipment = Item("AtmoSuit", 200f);
var meat = Item("Meat", 1f, new Edible());
var ingredient = Item("ModIngredient");
ingredient.GetComponent<KPrefabID>().Tags.Add(GameTags.CookingIngredient);
var flower = Item("Flower");
flower.GetComponent<KPrefabID>().Category = new Tag("ModCategory");
var plant = Item("Plant", 1f, new Crop { cropVal = new CropVal { cropId = "Flower", numProduced = 1 } }, new Uprootable());
Item("Egg");
Item("Seed");
Item("Hatch", 1f, new CreatureBrain());
Item("Duplicant", 1f, new MinionIdentity());
Item("Building", 1f, new Building());
Item("BadMass", float.NaN);
Item("ZeroMass", 0f);
Item("InfiniteMass", float.PositiveInfinity);
Item("NoPickup").Components.RemoveAll(value => value is Pickupable);
Item("NoPrimary").Components.RemoveAll(value => value is PrimaryElement);
Item("WrongIdentity").GetComponent<KPrefabID>().PrefabTag = new Tag("SomethingElse");
// 普通物品映射到铁元素时，也必须实例化自己的物品预制体。
ElementLoader.Elements[new Tag("OrbitalResearchDatabank")] = iron;
foreach (string name in new[] { "Hatch", "Duplicant", "Building", "Plant", "BadMass", "ZeroMass", "InfiniteMass", "NoPickup", "NoPrimary", "WrongIdentity", "ResearchDatabank", "HiddenModItem", "MissingDlcItem", "ForbiddenDlcItem" })
    Check(SolidResourceGenerator.ItemResourceSupport.GetPrefab(new Tag(name)) == null, "必须拒绝无效物品或活体：" + name);
Check(SolidResourceGenerator.ItemResourceSupport.GetPrefab(iron.tag) == null &&
    SolidResourceGenerator.ItemResourceSupport.GetPrefab(new Tag("Fluid")) == null,
    "物品分支不能绕过元素的物态和禁用检查");

options = null;
SolidResourceGenerator.GeneratorSolidOptionsPatch.Postfix(filter, ref options);
Check(options[new Tag("TechComponents")].Contains(new Tag("OrbitalResearchDatabank")), "未发现的数据磁盘必须沿用原版科技组件分类");
Check(options[new Tag("TechComponents")].SetEquals(new[] { new Tag("OrbitalResearchDatabank") }), "眼冒金星模式只显示对应磁盘，不能显示被原版隐藏的本体磁盘");
Check(options[GameTags.CookingIngredient].SetEquals(new[] { new Tag("Meat"), new Tag("ModIngredient") }), "食物分类必须兼容食物生成器");
Check(options[GameTags.Other].Contains(new Tag("Flower")) && !options.ContainsKey(new Tag("ModCategory")), "非食用收获物必须兼容植物生成器分类");
Check(options[GameTags.Other].Contains(new Tag("Egg")) && options[GameTags.Other].Contains(new Tag("Seed")), "允许生成蛋和种子物品");
foreach (string name in new[] { "Hatch", "Duplicant", "Building", "Plant", "BadMass", "ZeroMass", "InfiniteMass", "NoPickup", "NoPrimary", "WrongIdentity", "ResearchDatabank", "HiddenModItem", "MissingDlcItem", "ForbiddenDlcItem" })
    foreach (var entries in options.Values)
        Check(!entries.Contains(new Tag(name)), "无效物品不能进入菜单：" + name);

var itemOperational = new Operational { IsOperational = true };
var itemFilter = new Filterable { SelectedTag = new Tag("OrbitalResearchDatabank") };
var itemGenerator = new Generator();
itemGenerator.Initialize(itemOperational, itemFilter);
var itemRate = itemGenerator.sliderControls[1];
Check(itemGenerator.sliderControls.Length == 2 && ReferenceEquals(itemGenerator.sliderControls[0], itemGenerator) &&
    itemRate.SliderUnits == "件/s" && itemRate.GetSliderValue(0) == 1f && itemRate.GetSliderMax(0) == 100f,
    "元素与物品必须提供独立速度控件，新物品默认 1 件/s");
baseline = iron.substance.TotalMass;
for (int tick = 0; tick < 4; tick++) itemGenerator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == 0 && itemOperational.IsActive, "不足一件时只能累计进度，工作中应耗电");
itemOperational.IsOperational = false;
itemGenerator.Sim200ms(1f);
Check(GameUtil.Spawned.Count == 0 && !itemOperational.IsActive, "断电期间不能累计物品");
itemOperational.IsOperational = true;
itemRate.SetSliderValue(0f, 0);
itemGenerator.Sim200ms(1f);
Check(GameUtil.Spawned.Count == 0 && !itemOperational.IsActive, "物品速度为零时暂停累计");
itemRate.SetSliderValue(1f, 0);
itemGenerator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == 1 && GameUtil.LastPrefab == disk && iron.substance.TotalMass == baseline,
    "累计一秒必须生成磁盘自身，不能生成基础元素");
var spawnedPrimary = GameUtil.Spawned[0].GetComponent<PrimaryElement>();
Check(spawnedPrimary.Units == 1f && spawnedPrimary.Mass == 2f && spawnedPrimary.Temperature == 300f && GameUtil.Spawned[0].Active,
    "每件保持完整单位、预制体质量和温度，配置完成后才激活");
Check(disk.GetComponent<PrimaryElement>().Units == 1f, "实例生产不能改动预制体");

itemRate.SetSliderValue(2.5f, 0);
for (int tick = 0; tick < 10; tick++) itemGenerator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == 6, "2.5 件/s 两秒必须生成 5 件完整物品");
itemRate.SetSliderValue(0.1f, 0);
for (int tick = 0; tick < 50; tick++) itemGenerator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == 7, "0.1 件/s 十秒必须生成一件，不能丢失小数进度");

itemRate.SetSliderValue(1f, 0);
itemGenerator.Sim200ms(0.8f);
itemFilter.SelectedTag = iron.tag;
itemFilter.SelectedTag = new Tag("AtmoSuit");
itemGenerator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == 7, "切换资源必须清除旧物品进度");
for (int tick = 0; tick < 4; tick++) itemGenerator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == 8 && GameUtil.LastPrefab == equipment && GameUtil.Spawned[7].GetComponent<PrimaryElement>().Mass == 200f,
    "装备按件生成，不能套用 kg/s 改质量");

itemGenerator.Sim200ms(0.8f);
var progressField = typeof(Generator).GetField("pendingItems", BindingFlags.Instance | BindingFlags.NonPublic);
var tagField = typeof(Generator).GetField("pendingItemTag", BindingFlags.Instance | BindingFlags.NonPublic);
var itemRateField = typeof(Generator).GetField("itemsPerSecond", BindingFlags.Instance | BindingFlags.NonPublic);
Check(progressField.IsDefined(typeof(Serialize)) && tagField.IsDefined(typeof(Serialize)) && itemRateField.IsDefined(typeof(Serialize)), "物品速度、进度与对应资源必须保存");
itemGenerator.Spawn();
itemGenerator.Sim200ms(0.2f);
Check(GameUtil.Spawned.Count == 9, "读入同一资源时必须保留物品小数进度");
itemRateField.SetValue(itemGenerator, float.NaN);
progressField.SetValue(itemGenerator, double.NaN);
itemGenerator.Spawn();
Check(itemRate.GetSliderValue(0) == 1f && (double)progressField.GetValue(itemGenerator) == 0d, "读档必须修复无效物品速度与进度");

itemGenerator.Sim200ms(0.8f);
rateGenerator.sliderControls[1].SetSliderValue(3.5f, 0);
itemGenerator.Trigger((int)GameHashes.CopySettings, rateGenerator.gameObject);
Check(itemRate.GetSliderValue(0) == 3.5f && itemGenerator.GetSliderValue(0) == rateGenerator.GetSliderValue(0) &&
    (double)progressField.GetValue(itemGenerator) == 0d, "复制设置必须包含两种速度，但不能复制生产进度");
itemRate.SetSliderValue(1000f, 0);
Check(rateGenerator.sliderControls[1].GetSliderValue(0) == 3.5f, "复制后物品速度仍需各建筑独立保存");
itemGenerator.Sim200ms(0.2f);
Check(itemRate.GetSliderValue(0) == 100f && GameUtil.Spawned.Count == 29, "物品上限 100 件/s 时每 200 ms 生成 20 个实体");
itemRate.SetSliderValue(-1f, 0);
Check(itemRate.GetSliderValue(0) == 0f, "物品速度不能为负");
foreach (float rate in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
{
    itemRate.SetSliderValue(rate, 0);
    Check(itemRate.GetSliderValue(0) == 1f, "异常物品速度必须恢复默认");
}
int before = GameUtil.Spawned.Count;
foreach (float dt in new[] { 0f, -1f, float.NaN, float.PositiveInfinity }) itemGenerator.Sim200ms(dt);
Grid.Valid = false;
itemGenerator.Sim200ms(1f);
Grid.Valid = true;
Assets.Prefabs.Remove(new Tag("AtmoSuit"));
itemGenerator.Sim200ms(1f);
Check(GameUtil.Spawned.Count == before && !itemOperational.IsActive, "无效时间步、格子或缺失物品必须停机，不能累计进度");
itemFilter.SelectedTag = new Tag("OrbitalResearchDatabank");
itemGenerator.SetSliderValue(0f, 0);
itemGenerator.Sim200ms(1f);
Check(GameUtil.Spawned.Count == before + 1, "固体速度为 0 不应影响物品生产");
itemFilter.SelectedTag = iron.tag;
itemRate.SetSliderValue(0f, 0);
itemGenerator.SetSliderValue(1f, 0);
baseline = iron.substance.TotalMass;
itemGenerator.Sim200ms(0.2f);
Check(Math.Abs(iron.substance.TotalMass - baseline - 0.2f) < 0.0001f, "物品速度为 0 不应影响固体生产");
itemFilter.SelectedTag = new Tag("OrbitalResearchDatabank");
itemRate.SetSliderValue(1f, 0);
before = GameUtil.Spawned.Count;
itemGenerator.Sim200ms(float.MaxValue);
Check(GameUtil.Spawned.Count - before == Generator.MaxItemsPerTick, "异常长时间步必须限制单次物品实例数量");

// 模拟切回没有眼冒金星的本体：原版不会再给本体磁盘添加隐藏标记。
DlcManager.ActiveDlcIds.Clear();
baseDisk.GetComponent<KPrefabID>().Tags.Remove(GameTags.HideFromSpawnTool);
options = null;
SolidResourceGenerator.GeneratorSolidOptionsPatch.Postfix(filter, ref options);
Check(options[new Tag("TechComponents")].SetEquals(new[] { new Tag("ResearchDatabank") }), "无 DLC 时必须保留本体磁盘并排除需要眼冒金星的磁盘");
var baseFilter = new Filterable { SelectedTag = new Tag("ResearchDatabank") };
var baseOperational = new Operational { IsOperational = true };
var baseGenerator = new Generator();
baseGenerator.Initialize(baseOperational, baseFilter);
before = GameUtil.Spawned.Count;
baseGenerator.Sim200ms(1f);
Check(GameUtil.Spawned.Count == before + 1 && GameUtil.LastPrefab == baseDisk, "无 DLC 时必须能生成本体磁盘自身");
baseFilter.SelectedTag = new Tag("OrbitalResearchDatabank");
baseGenerator.Sim200ms(1f);
Check(GameUtil.Spawned.Count == before + 1 && !baseOperational.IsActive, "保存过的 DLC 物品在无 DLC 模式下也不能继续生成");
DlcManager.ActiveDlcIds.Add("EXPANSION1_ID");
baseDisk.GetComponent<KPrefabID>().Tags.Add(GameTags.HideFromSpawnTool);
baseFilter.SelectedTag = new Tag("ResearchDatabank");
baseGenerator.Sim200ms(1f);
Check(GameUtil.Spawned.Count == before + 1 && !baseOperational.IsActive, "保存过的隐藏本体磁盘在 DLC 模式下也必须停机");
Console.WriteLine("PASS: vanilla and Spaced Out databank selection/production, hidden item exclusion and required/forbidden DLC restrictions.");
Console.WriteLine("PASS: item discovery, databank identity, whole units, equipment mass, fractional rates, power, switching, settings, save fields, invalid prefabs, bounded spawning and shared categories.");

// 只替代游戏边界，执行的是正式生产组件；不模拟 UI、存档和物理系统。
public class KMonoBehaviour
{
    private object[] components;
    private readonly Dictionary<int, Action<object>> handlers = new Dictionary<int, Action<object>>();
    public Transform transform = new Transform();
    public GameObject gameObject = new GameObject();
    protected virtual void OnPrefabInit() { }
    protected virtual void OnSpawn() { }
    protected virtual void OnCleanUp() { }
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
public interface ISliderControl
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
public interface IMultiSliderControl
{
    string SidescreenTitleKey { get; }
    bool SidescreenEnabled();
    ISliderControl[] sliderControls { get; }
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
    public static readonly Tag CookingIngredient = new Tag("CookingIngredient");
    public static readonly Tag HideFromSpawnTool = new Tag("HideFromSpawnTool");
}
public class Filterable : KMonoBehaviour
{
    private Tag selectedTag;
    public event Action<Tag> onFilterChanged;
    public Tag SelectedTag { get => selectedTag; set { selectedTag = value; onFilterChanged?.Invoke(value); } }
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
    public static readonly Dictionary<Tag, GameObject> Prefabs = new Dictionary<Tag, GameObject>();
    public static GameObject TryGetPrefab(Tag tag) => HasPrefab && !MissingPrefabs.Contains(tag)
        ? Prefabs.TryGetValue(tag, out var prefab) ? prefab : new GameObject() : null;
    public static IEnumerable<GameObject> GetPrefabsWithComponent<T>()
    {
        foreach (var prefab in Prefabs.Values)
            if (prefab.GetComponent<T>() != null) yield return prefab;
    }
}
public class PrimaryElement
{
    public float MassPerUnit, Units, Temperature;
    public float Mass => MassPerUnit * Units;
}
public class KPrefabID
{
    public Tag PrefabTag, Category;
    public string[] RequiredDlcIds, ForbiddenDlcIds;
    public string[] GetRequiredDlcIds() => RequiredDlcIds;
    public string[] GetForbiddenDlcIds() => ForbiddenDlcIds;
    public HashSet<Tag> Tags = new HashSet<Tag>();
    public bool HasTag(Tag tag) => Tags.Contains(tag);
}
public static class DlcManager
{
    public static readonly HashSet<string> ActiveDlcIds = new HashSet<string> { "EXPANSION1_ID" };
    public static bool IsCorrectDlcSubscribed(string[] required, string[] forbidden) =>
        (required == null || Array.TrueForAll(required, id => ActiveDlcIds.Contains(id))) &&
        (forbidden == null || Array.TrueForAll(forbidden, id => !ActiveDlcIds.Contains(id)));
}
public class Pickupable { }
public class Building { }
public class CreatureBrain { }
public class MinionIdentity { }
public class Uprootable { }
public class Edible { }
public class Crop { public CropVal cropVal; }
public class CropVal { public string cropId; public int numProduced; }
public static class DiscoveredResources
{
    public static Tag GetCategoryForEntity(KPrefabID id) => id.Category;
}
public static class GameUtil
{
    public static List<GameObject> Spawned = new List<GameObject>();
    public static GameObject LastPrefab;
    public static GameObject KInstantiate(GameObject prefab, Vector3 position, Grid.SceneLayer layer)
    {
        LastPrefab = prefab;
        var item = new GameObject();
        var primary = prefab.GetComponent<PrimaryElement>();
        item.Components.Add(new PrimaryElement { MassPerUnit = primary.MassPerUnit, Units = primary.Units, Temperature = primary.Temperature });
        Spawned.Add(item);
        return item;
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
namespace SolidResourceGenerator { public static class SolidResourceGeneratorConfig { public const string ID = "SolidResourceGenerator"; } }
namespace UnityEngine
{
    public class GameObject
    {
        public object Component;
        public readonly List<object> Components = new List<object>();
        public bool Active;
        public void SetActive(bool value) => Active = value;
        public T GetComponent<T>() => Component is T value ? value : (T)Components.Find(item => item is T);
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
