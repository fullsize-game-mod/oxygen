# 固体资源生成器

为《缺氧》新增一座仅靠电力生成指定固体元素或可拾取物品的建筑，可与仓库中的其他 Mod 分别启用。

当前版本：**1.2.1**。修复数据磁盘出现两个同名选项的问题：物品筛选现在遵守原版隐藏标记和 DLC 限制，本体与《眼冒金星》各自使用对应的磁盘。支持数据磁盘、装备、食物、种子与蛋等物品生成。固体和物品各有独立速度设置；物品按完整件数生成，小数进度随存档保存。资源无需事先发现或拥有；建筑、活体和生长中的植物不在物品范围内。保留 1.1.3 的共享侧栏分类修复。既有建筑无需重建，更新本地 Mod 文件后完全重启游戏生效。

同时使用食物原料生成器时，请将其一起更新至 **1.1.1 或更高版本**。旧版食物生成器把 `Void` 行放入普通资源分类，会污染共享侧栏缓存，使后续点击或刷新固体生成器时出现 `An item with the same key has already been added. Key: Void`。两个修复版统一使用原版保留的 `Void` 分类；更新后需完全重启游戏以清除旧侧栏缓存。

| 项目 | 行为 |
| --- | --- |
| 建造位置 | 实用（Utilities）菜单，无需额外研究 |
| 占地与外观 | 1×2 格，建在地面上，复用原版氧气扩散器动画 |
| 建造成本 | 200 kg 金属矿石或精炼金属，30 秒 |
| 工作耗电 | 120 W，未选择有效资源、速度为 0 或自动化红灯时待机 |
| 固体生成速度 | 0～100 kg/s，滑块或数字输入，支持一位小数；默认 1 kg/s |
| 物品生成速度 | 0～100 件/s，独立滑块或数字输入，支持一位小数；默认 1 件/s |
| 更新频率 | 每 200 ms 更新；固体按质量生成，物品累计进度后生成完整件数 |
| 输入 | 无需原料、管道或复制人操作 |
| 资源选择 | 原版单选侧栏，列出可生成的固体元素与物品，无需事先发现或拥有；每台建筑独立选择，随存档保存 |
| 速度设置 | 两种速度独立保存并支持复制；旧存档保留原固体速度，物品速度缺省为 1 件/s |
| 自动化控制 | 上方格为输入口：绿灯允许生产，红灯停机；未接线时允许生产 |
| 输出 | 直接掉落在建筑脚下，使用原版掉落物合并机制 |
| 产物温度 | 固体通常为 20°C，低熔点元素在熔点以下生成，最低 1 K；物品保留自身预制体的初始温度 |
| 停机条件 | 断电、自动化红灯、手动禁用等原版运行条件；不受淹没或过热限制 |

选择范围是游戏当前已加载、满足生成条件的**固体元素与可拾取物品**，具体规则见下文。

## 可生成的固体元素

**本 Mod 没有写死元素白名单，也不要求资源事先被发现或拥有。**从 1.1.2 起，生成器使用原版单选侧栏，但自行列出当前游戏已加载、未禁用的固态元素；元素必须具有可用的掉落物预制体和生成数据，且熔点高于 1 K。列表与实际生产共用这组检查，找不到元素或不满足条件时，建筑会待机。仅查看或选择列表不会将资源标记为全局已发现，其他建筑沿用原版筛选规则。

### 资源参考表

下表按用途整理了公开的 [740622 版本运行时元素导出数据](https://github.com/jastice/oni-material-graph/blob/main/data/catalogs/oni-740622-v11/materials.json)中的固态元素，括号内为游戏内部 ID，便于对照不同语言的名称。**这是参考元素表，不是逐项游戏内实测通过的名单，也不代表未安装 DLC 时全部可用。**部分新元素保留导出数据中的英文名称。

| 类别 | 元素名称（内部 ID） |
| --- | --- |
| 岩石与建材 | 砂岩（`SandStone`）、火成岩（`IgneousRock`）、沉积岩（`SedimentaryRock`）、花岗岩（`Granite`）、黑曜石（`Obsidian`）、镁铁质岩（`MaficRock`）、化石（`Fossil`）、陶瓷（`Ceramic`）、碎石（`CrushedRock`） |
| 土壤与过滤介质 | 泥土（`Dirt`）、粘土（`Clay`）、沙子（`Sand`）、浮土（`Regolith`）、污染土（`ToxicSand`）、泥巴（`Mud`）、污染泥（`ToxicMud`） |
| 金属矿石 | 铜矿（`Cuprite`）、铁矿（`IronOre`）、金汞齐（`GoldAmalgam`）、铝矿（`AluminumOre`）、黑钨矿（`Wolframite`）、钴矿（`Cobaltite`）、朱砂矿（`Cinnabar`）、镍矿（`NickelOre`）、锌矿（`ZincOre`）、方铅矿（`Galena`） |
| 精炼金属 | 铜（`Copper`）、铁（`Iron`）、金（`Gold`）、铝（`Aluminum`）、钨（`Tungsten`）、铅（`Lead`）、钴（`Cobalt`）、铌（`Niobium`）、镍（`Nickel`）、铱（`Iridium`）、锌（`Zinc`）、固态汞（`SolidMercury`） |
| 工业与农业原料 | 煤炭（`Carbon`）、精炼碳（`RefinedCarbon`）、石灰（`Lime`）、磷矿（`Phosphorite`）、精炼磷（`Phosphorus`）、肥料（`Fertilizer`）、盐（`Salt`）、铁锈（`Rust`）、硫（`Sulfur`）、蔗糖（`Sucrose`） |
| 有机物与可释放气体的资源 | 藻类（`Algae`）、菌泥（`SlimeMold`）、木材（`WoodLog`）、胶合板 / Plywood（`FabricatedWood`）、氧石（`OxyRock`）、漂白石（`BleachStone`）、咸乳蜡（`MilkFat`）、动物脂肪 / Tallow（`Tallow`） |
| 制造材料与稀有材料 | 钢（`Steel`）、塑料（`Polypropylene`）、塑料质（`HardPolypropylene`）、玻璃（`Glass`）、钻石（`Diamond`）、石墨（`Graphite`）、深渊晶石（`Katairite`）、富勒烯（`Fullerene`）、异构树液（`Isoresin`）、隔热质（`SuperInsulator`）、导热质（`TempConductorSolid`） |
| 冰雪 | 冰（`Ice`）、污染冰（`DirtyIce`）、盐水冰（`BrineIce`）、污染盐水冰（`MurkyBrineIce`）、雪（`Snow`）、碎冰（`CrushedIce`）、压实雪 / Packed Snow（`StableSnow`） |
| 其他低温固态资源 | 固态二氧化碳（`SolidCarbonDioxide`）、固态氯（`SolidChlorine`）、固态氢（`SolidHydrogen`）、固态氧（`SolidOxygen`）、固态甲烷（`SolidMethane`）、固态丙烷（`SolidPropane`）、固态乙醇（`SolidEthanol`）、固态原油（`SolidCrudeOil`）、固态石油（`SolidPetroleum`）、固态石脑油（`SolidNaphtha`）、固态超级冷却剂（`SolidSuperCoolant`）、固态粘性凝胶（`SolidViscoGel`） |
| 其他凝固产物 | 固态树液 / Solid Sap（`SolidResin`）、固态树脂 / Solid Resin（`NaturalSolidResin`）、Solid Gunk（`Gunk`）、Frozen Brackene（`MilkIce`）、Frozen Phyto Oil（`FrozenPhytoOil`）、Frozen Ovolene（`FishMilkIce`）、Frozen Squid Ink（`FrozenInk`）、Frozen Mucin（`SolidMucus`） |
| 核工业资源 | 铀矿（`UraniumOre`）、浓缩铀（`EnrichedUranium`）、贫铀（`DepletedUranium`）、固态核废料（`SolidNuclearWaste`）、Corium（`Corium`） |
| 其他版本或 DLC 资源 | 琥珀（`Amber`）、泥炭（`Peat`）、页岩（`Shale`）、玄武岩（`Basalt`）、粉砂岩（`SiltStone`）、Coquina（`Coquina`）、Corallium（`Corallium`）、Gum Wood（`PalmWood`）、珍珠（`Pearl`）、橡胶（`Rubber`） |

导出数据还包含 Bitumen（`Bitumen`）、Genetic Ooze（`Creature`）、Pyrite（`FoolsGold`）、Radium（`Radium`）等特殊条目。**数据中存在不等于侧栏可选或可生产**，这些条目同样受加载状态、禁用状态和掉落物等检查限制，不承诺可用。

### DLC、其他 Mod 与生成边界

- **实际列表随游戏环境变化**：游戏版本、启用的 DLC 及其他 Mod 都可能改变元素、禁用状态或资源侧栏。上表涉及多个 DLC；本 Mod 不解锁未加载的 DLC 元素，也不强制启用隐藏或测试元素。最终以你游戏中的选择列表及运行条件为准。
- **其他 Mod 的元素**：新增固态元素只要已加载并满足上述生成条件，也会进入生成器列表，不需要先发现或为每个 ID 修改白名单。
- **固体元素与物品分别处理**：蔗糖、动物脂肪等属于元素，按 kg/s 生成；成品食物、种子、蛋、装备和数据磁盘等属于物品，按件/s 生成。建筑、动物、复制人和生长中的植物不能生成。
- **不直接生成气体或液体**：不能选水、氧气、氢气、液态石油等对应形态；但冰、固态氧、固态氢、固态石油属于各自独立的固态元素，满足条件时可以生成。
- **生成后仍遵循游戏物理规则**：冰和其他低熔点资源会在熔点以下生成，放进较热环境仍可能融化或汽化；氧石、漂白石等仍可能释放气体，放射性资源仍遵循原版行为。

## 可生成的物品

1.2.0 起自动扫描已加载的可拾取预制体，不需要物品白名单或发现记录。1.2.1 起排除带有 `HideFromSpawnTool` 标记或不符合当前 DLC 限制的物品。数据磁盘可在科技组件分类中查找，也可用侧栏搜索：本体使用 `ResearchDatabank`，《眼冒金星》使用 `OrbitalResearchDatabank`。本体磁盘虽然在《眼冒金星》中仍保留预制体，但已被原版标记为隐藏，不能用于该模式的研究设备，因此不会再列出或生成。没有加载对应 DLC 或物品的环境不会强行解锁它们。

从 1.2.0 更新后，如果建筑之前选中了不适用的磁盘，会待机，需要重新选择列表中的有效磁盘。已经生成的物品不会被自动删除或转换。

- **范围**：数据磁盘、装备、成品食物、食物原料、种子、蛋及其他满足条件的可拾取物品。其他 Mod 注册的物品也按相同规则筛选。
- **检查**：物品必须有匹配自身 ID 的预制体、`Pickupable` 与 `PrimaryElement`，且 `MassPerUnit` 为正的有限值；不得带原版隐藏标记，必须符合其要求与禁止的 DLC 限制。建筑、带小动物或复制人身份的对象及生长中的植物被排除。元素条目不能借物品分支绕过固体元素检查。
- **按件生产**：每次实例化物品自身并设置为完整的 1 单位，保留物品自身组件、初始质量与温度。例如 1 件/s 每秒产生一个数据磁盘；2.5 件/s 在两秒内产生五个；0.1 件/s 每十秒产生一个。装备逐件生成，不通过增大一件装备的质量来充数。
- **进度与停机**：不足一件的进度会累计。断电、自动化红灯、手动禁用或物品速度为 0 时暂停且不增加进度。累计期间属于工作状态，消耗 120 W。切换资源清除原物品进度；速度和同一资源的进度随存档保存。复制建筑设置复制资源和两种速度，接收建筑的物品进度清零。
- **侧栏分类**：物品沿用原版分类；与食物生成器重叠的食物归入烹饪原料，与植物生成器重叠的非食用收获物归入其他，避免共享侧栏缓存同一物品的重复行。

资源侧栏仍为单选，一次只生成选中的一种资源。固体使用“固体生成速度”，物品使用“物品生成速度”，另一项速度不影响当前产物。单次更新最多实例化 100 件物品，异常长时间步的剩余生产进度留待后续更新。

## 安装与使用

1. 解压 [SolidResourceGenerator.zip](../dist/SolidResourceGenerator.zip)。
2. 将整个 `SolidResourceGenerator` 文件夹放进游戏的 `mods/Local/` 目录。各系统路径见[仓库安装说明](../README.md#安装)。
3. 启用“固体资源生成器”并按游戏提示重启。
4. 在“实用”菜单建造生成器，连接电线，在资源选择侧栏选择一种固体元素或物品；从未采集、生产或持有过的有效资源也可直接选择。
5. 生成固体时调节“固体生成速度”（0～100 kg/s）；生成数据磁盘等物品时调节“物品生成速度”（0～100 件/s）。将当前类型对应的速度设为 0 时暂停生产并待机。
6. 通电后按设定速度自动生产，断电即停止。切换资源只改变后续产物，已经生成的资源保留。
7. 需要自动启停时，将传感器或开关的自动化线接到建筑上方的输入口。绿灯按设定速度生产，红灯停机；速度为 0 时即使绿灯也不生产。无需自动化时可不接线。

安装后的目录应为：

```text
mods/Local/SolidResourceGenerator/
├── SolidResourceGenerator.dll
├── mod.yaml
└── mod_info.yaml
```

已有存档可建造此建筑。包含本建筑的存档应保持本 Mod 启用；卸载前先拆除新增建筑。

## 编译与检查

2026-11-15 的 1.2.1 包已使用本机当前游戏程序集编译。固体与物品的独立生产检查、植物生成器检查及跨生成器侧栏回收池回归检查通过。新增检查覆盖本体与《眼冒金星》各自的磁盘列表及生产、隐藏物品、要求与禁止的 DLC 限制，以及原先保存过的不适用磁盘选择会待机。原有检查覆盖数据磁盘自身预制体、完整件数、装备质量、小数进度、停机、切换资源、两种速度复制、存档字段与异常值、无效预制体及单次生成上限。回归检查使用游戏边界替身，不代表已完成真实游戏 UI 或存档验收。

本机未安装 .NET SDK，实际构建与检查使用 PowerShell 附带的 Roslyn：发布 DLL 引用本机游戏程序集，独立检查引用本机 .NET 运行时。下方为安装 SDK 后的标准命令。

与现有加热器项目使用相同的 .NET SDK、Harmony 2.4.2 和游戏程序集路径配置，在仓库根目录执行：

```sh
dotnet build SolidResourceGenerator/SolidResourceGenerator.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
dotnet run --project tests/SolidGeneration/SolidGeneration.csproj -c Release
```

构建后自动更新 `dist/SolidResourceGenerator/`；发布时需重新压缩整个文件夹为 ZIP。游戏和 Unity 引用程序集不会随 Mod 分发。

**当前验证边界：**已使用 [OniStubs 2.0.0](https://github.com/richardspence/OniStubs) 和 Unity 编译引用完成构建；生产组件的独立检查覆盖断电、可调生产速率、零速暂停、上下限与异常值、建筑间设置独立、复制速度、存档字段标记及读入值校验，以及资源切换、低熔点温度、病菌参数、异常时间步、无效元素、缺失预制体及无效格子。资源列表补丁的独立检查还覆盖未发现资源的选择与生产、分类与无选择项、无效资源排除，以及普通筛选器和原版共享数据不被修改。测试直接调用补丁，以替身隔离游戏边界，不执行真实 Harmony 注入、UI 渲染或存档序列化，不代表游戏内验收。声明的最低版本为 718946，API 版本为 2；公开引用包早于此版本，不能证明当前游戏 API 完全兼容。

尚需在实际游戏中确认：建筑菜单、资源侧栏与两个速度滑块正常显示，新存档中未发现或未拥有过的资源可直接选择并生产，数据磁盘可被研究设备正常消耗，装备、种子和蛋保留原版用途，普通筛选器仍遵循原版规则，实际产量和耗电符合预期，两台建筑设置互不影响，复制设置与保存重载保留资源、两种速度和相应进度，自动化口在预览、建造中及完成后均正确显示，红灯停机、绿灯恢复、未接线允许生产，以及冰等元素的物相行为。自动化信号由原版控制器处理，独立检查不运行真实自动化网络。

## API 参考

- [OniStubs：建筑、资源筛选与生成接口](https://github.com/richardspence/OniStubs)。
- 历史原版源码：[Filterable](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/Filterable.cs)、[FilterSideScreen](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/FilterSideScreen.cs)、[Substance.SpawnResource](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/Substance.cs)。历史实现仅用于理解接口意图，实际行为以当前游戏为准。
