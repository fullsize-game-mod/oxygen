# 食物原料生成器

参考固体资源生成器，为《缺氧》新增一座仅靠电力生产肉、生蛋液、鱼肉和作物原料的建筑。独立安装，可与仓库中的其他 Mod 同时启用。

当前版本：**1.0.0**。

| 项目 | 行为 |
| --- | --- |
| 建造位置 | 实用（Utilities）菜单，无需额外研究 |
| 占地与外观 | 1×2 格，建在地面上，复用原版氧气扩散器动画 |
| 建造成本 | 200 kg 金属矿石或精炼金属，30 秒 |
| 工作耗电 | 120 W；断电、自动化红灯、手动禁用、无有效选择或零速时待机 |
| 生成速度 | 0～100 kg/s，滑块或数字输入，支持一位小数；默认 1 kg/s |
| 更新频率 | 每 200 ms 按设定速度生产，例如 2.5 kg/s 时每次生成 0.5 kg |
| 输入 | 无需原料、管道或复制人操作 |
| 原料选择 | 原版单选侧栏，只显示已加载且满足生成条件的原料，无需事先发现或拥有 |
| 设置 | 原料和速度由每台建筑独立保存，支持复制建筑设置 |
| 自动化 | 上方格输入口：绿灯允许生产，红灯停机；未接线时允许生产 |
| 输出 | 真正的原料物品掉落在建筑脚下，以 20°C 生成；后续堆叠、腐败和烹饪遵循原版规则 |
| 环境限制 | 与固体生成器一致，不受淹没或过热限制 |

## 原料范围

本版提供以下 12 个原料 ID。菜单只显示当前游戏实际已加载、具有正确物品标识、可搬运组件和有效单位质量的预制体；缺失的 DLC 原料会自动隐藏，读档后选中的原料不可用时停止生产。

| 原料 | 游戏内部 ID |
| --- | --- |
| 肉 | `Meat` |
| 生蛋液 | `RawEgg` |
| 鱼肉 | `FishMeat` |
| 贝肉 | `ShellfishMeat` |
| 米虱 | `BasicPlantFood` |
| 蘑菇 | `Mushroom` |
| 海生菜 | `Lettuce` |
| 毛刺浆果 | `PrickleFruit` |
| 冰霜小麦粒 | `ColdWheatSeed` |
| 火椒粒 | `SpiceNut` |
| 小吃豆 | `BeanPlantSeed` |
| 沼泽果冻 | `SwampFruit` |

“蛋”指可用于烹饪的**生蛋液**，不包含能孵化的小动物蛋。本版使用上述固定原料列表，不自动加入其他 Mod 的新原料，也不生产熟食或成品菜肴。

速度始终按**千克/秒**计量。谷粒等按单位显示的原料会根据各自预制体的单位质量换算数量，因此 1 kg 不一定等于 1 粒。产物保留原版的食用、烹饪、种植及腐败属性，不会用同名基础元素替代物品。易腐原料仍需妥善储存。

## 安装与使用

1. 解压 [FoodResourceGenerator.zip](../dist/FoodResourceGenerator.zip)。
2. 将整个 `FoodResourceGenerator` 文件夹放入游戏的 `mods/Local/` 目录，各系统路径见[仓库安装说明](../README.md#安装)。
3. 启用“食物原料生成器”，按游戏提示重启。
4. 在“实用”菜单建造生成器并连接电线。
5. 在侧栏选择原料，设置“生成速度”，通电后开始生产。设为 0 时暂停；切换原料不会改变已生成的物品。
6. 需要自动化控制时连接上方输入口；无需自动化时可不接线。

安装后的目录应为：

```text
mods/Local/FoodResourceGenerator/
├── FoodResourceGenerator.dll
├── mod.yaml
└── mod_info.yaml
```

已有存档可建造本建筑。卸载 Mod 前请先拆除新增建筑。

## 编译与检查

在仓库根目录执行：

```sh
dotnet build FoodResourceGenerator/FoodResourceGenerator.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
dotnet run --project tests/FoodGeneration/FoodGeneration.csproj -c Release
```

构建后自动更新 `dist/FoodResourceGenerator/`；ZIP 安装包需重新压缩整个文件夹。安装包只包含本 Mod 的 DLL 和两个 YAML 文件。

已使用 [OniStubs 2.0.0](https://github.com/richardspence/OniStubs) 与 Unity 编译引用完成构建，独立检查通过。检查直接执行生产组件和资源列表补丁，覆盖物品 ID、质量与单位换算、资源切换、断电、零速、速度边界和异常值、设置独立与复制、存档字段及读入值校验、无效时间步、无效格子、缺失或异常预制体、未发现原料、菜单作用范围及共享数据隔离。

**尚未游戏内验收**：这些检查使用替身隔离游戏边界，不执行真实 Harmony 注入、UI 渲染、自动化网络、腐败、堆叠或存档序列化。需在实际游戏中确认菜单与滑块、120 W 耗电、红绿信号启停、12 种原料各自的生产与烹饪、保存重载和与其他生成器同时启用的行为。声明最低版本为 718946、API 版本为 2；历史编译引用不能证明当前游戏版本完全兼容。

实现参考历史原版的 [MeatConfig](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/MeatConfig.cs)、[RawEggConfig](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/RawEggConfig.cs)、[ComplexFabricator](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/ComplexFabricator.cs) 与 [PrimaryElement](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/PrimaryElement.cs) 的物品生成及质量换算方式，实际行为以当前游戏为准。
