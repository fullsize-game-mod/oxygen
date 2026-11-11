# 固体资源生成器

为《缺氧》新增一座仅靠电力生成指定固体元素的建筑，可与仓库中的空间加热器 Mod 分别启用。

当前版本：**1.1.1**。资源菜单仅显示当前可生成的固态元素，自动移除食物、小动物蛋、种子、装备、液体等不支持的选项及空分类。既有建筑无需重建，更新本地 Mod 文件后重启游戏生效。

| 项目 | 行为 |
| --- | --- |
| 建造位置 | 实用（Utilities）菜单，无需额外研究 |
| 占地与外观 | 1×2 格，建在地面上，复用原版氧气扩散器动画 |
| 建造成本 | 200 kg 金属矿石或精炼金属，30 秒 |
| 工作耗电 | 120 W，未选择有效资源、速度为 0 或自动化红灯时待机 |
| 生成速度 | 0～100 kg/s，滑块或数字输入，支持一位小数；默认 1 kg/s |
| 更新频率 | 每 200 ms 按所设速度生成，例如 2.5 kg/s 时每次生成 0.5 kg |
| 输入 | 无需原料、管道或复制人操作 |
| 资源选择 | 原版固体元素单选侧栏，每台建筑独立选择，随存档保存 |
| 速度设置 | 每台建筑独立保存，支持复制建筑设置；旧存档缺省为 1 kg/s |
| 自动化控制 | 上方格为输入口：绿灯允许生产，红灯停机；未接线时允许生产 |
| 输出 | 直接掉落在建筑脚下，使用原版掉落物合并机制 |
| 产物温度 | 通常为 20°C；冰等低熔点元素在熔点以下生成，最低 1 K |
| 停机条件 | 断电、自动化红灯、手动禁用等原版运行条件；不受淹没或过热限制 |

选择范围是游戏当前可用的**固体元素**，具体名称与限制见下方清单。

## 可生成的固体元素

**本 Mod 没有写死元素白名单。**资源列表使用原版固体选择侧栏；选定资源还必须是当前游戏已加载、未禁用的固态元素，具有可用的掉落物预制体和生成数据，且熔点高于 1 K，才能生产。找不到元素或不满足条件时，建筑会待机。

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

导出数据还包含 Bitumen（`Bitumen`）、Genetic Ooze（`Creature`）、Pyrite（`FoolsGold`）、Radium（`Radium`）等特殊条目。**数据中存在不等于侧栏可选或可生产**，这些条目同样受禁用状态、资源侧栏和掉落物检查限制，不承诺可用。

### DLC、其他 Mod 与生成边界

- **实际列表随游戏环境变化**：游戏版本、启用的 DLC 及其他 Mod 都可能改变元素、禁用状态或资源侧栏。上表涉及多个 DLC；本 Mod 不解锁未加载的 DLC 元素，也不强制启用隐藏或测试元素。最终以你游戏中的选择列表及运行条件为准。
- **其他 Mod 的元素**：新增固态元素若能进入原版资源选择侧栏，并满足上述生成条件，也可使用，不需要为每个 ID 修改白名单。
- **不生成普通物品实体**：成品食物、种子、蛋、装备、建筑、动物及复制人不属于这里的元素资源。蔗糖、动物脂肪等虽然可以被消耗，但它们在游戏中属于元素，因此与普通食物物品不同。
- **不直接生成气体或液体**：不能选水、氧气、氢气、液态石油等对应形态；但冰、固态氧、固态氢、固态石油属于各自独立的固态元素，满足条件时可以生成。
- **生成后仍遵循游戏物理规则**：冰和其他低熔点资源会在熔点以下生成，放进较热环境仍可能融化或汽化；氧石、漂白石等仍可能释放气体，放射性资源仍遵循原版行为。

## 安装与使用

1. 解压 [SolidResourceGenerator.zip](../dist/SolidResourceGenerator.zip)。
2. 将整个 `SolidResourceGenerator` 文件夹放进游戏的 `mods/Local/` 目录。各系统路径见[仓库安装说明](../README.md#安装)。
3. 启用“固体资源生成器”并按游戏提示重启。
4. 在“实用”菜单建造生成器，连接电线，在资源选择侧栏选择一种固体元素。
5. 在“生成速度”滑块或数字输入框中设置每秒产量，范围为 0～100 kg/s。设为 0 时暂停生产并待机。
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

与现有加热器项目使用相同的 .NET SDK、Harmony 2.4.2 和游戏程序集路径配置，在仓库根目录执行：

```sh
dotnet build SolidResourceGenerator/SolidResourceGenerator.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
dotnet run --project tests/SolidGeneration/SolidGeneration.csproj -c Release
```

构建后自动更新 `dist/SolidResourceGenerator/`；发布时需重新压缩整个文件夹为 ZIP。游戏和 Unity 引用程序集不会随 Mod 分发。

**当前验证边界：**已使用 [OniStubs 2.0.0](https://github.com/richardspence/OniStubs) 和 Unity 编译引用完成构建；生产组件的独立检查覆盖断电、可调生产速率、零速暂停、上下限与异常值、建筑间设置独立、复制速度、存档字段标记及读入值校验，以及资源切换、低熔点温度、病菌参数、异常时间步、无效元素、缺失预制体及无效格子。测试以替身隔离游戏边界，不代表游戏内验收，也不执行真实存档序列化。声明的最低版本为 718946，API 版本为 2；公开引用包早于此版本，不能证明当前游戏 API 完全兼容。

尚需在实际游戏中确认：建筑菜单、资源侧栏与速度滑块正常显示，0、0.1、2.5、100 kg/s 的实际产量和耗电符合预期，两台建筑设置互不影响，复制设置与保存重载保留资源和速度，自动化口在预览、建造中及完成后均正确显示，红灯停机、绿灯恢复、未接线允许生产，以及冰等元素的物相行为。自动化信号由原版控制器处理，独立检查不运行真实自动化网络。

## API 参考

- [OniStubs：建筑、资源筛选与生成接口](https://github.com/richardspence/OniStubs)。
- 历史原版源码：[Filterable](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/Filterable.cs)、[FilterSideScreen](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/FilterSideScreen.cs)、[Substance.SpawnResource](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/Substance.cs)。历史实现仅用于理解接口意图，实际行为以当前游戏为准。
