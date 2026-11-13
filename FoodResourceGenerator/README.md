# 食物原料生成器

参考固体资源生成器，为《缺氧》新增一座仅靠电力生产食物配方所需物品原料的建筑。独立安装，可与仓库中的其他 Mod 同时启用。

当前版本：**1.1.1**。原料列表从当前游戏已加载的食物配方自动生成，不再使用固定白名单。修复“无选择”项使用错误分类，导致切换到固体等生成器后侧栏刷新时出现重复 `Void` 键崩溃的问题；更新后需完全重启游戏，固体生成器建议同时更新至 1.1.3。

Steam 创意工坊：[订阅食物原料生成器](https://steamcommunity.com/sharedfiles/filedetails/?id=3839009559)。订阅后在游戏 Mod 列表启用并按提示重启；本地版与创意工坊版只启用其中一个。

| 项目 | 行为 |
| --- | --- |
| 建造位置 | 实用（Utilities）菜单，无需额外研究 |
| 占地与外观 | 1×2 格，建在地面上，复用原版氧气扩散器动画 |
| 建造成本 | 200 kg 金属矿石或精炼金属，30 秒 |
| 工作耗电 | 120 W；断电、自动化红灯、手动禁用、无有效选择或零速时待机 |
| 生成速度 | 0～100 kg/s，滑块或数字输入，支持一位小数；默认 1 kg/s |
| 更新频率 | 每 200 ms 按设定速度生产，例如 2.5 kg/s 时每次生成 0.5 kg |
| 输入 | 无需原料、管道或复制人操作 |
| 原料选择 | 原版单选侧栏，只显示食物配方实际使用且满足生成条件的物品原料，无需事先发现或拥有 |
| 设置 | 原料和速度由每台建筑独立保存，支持复制建筑设置 |
| 自动化 | 上方格输入口：绿灯允许生产，红灯停机；未接线时允许生产 |
| 输出 | 真正的原料物品掉落在建筑脚下，以 20°C 生成；后续堆叠、腐败和烹饪遵循原版规则 |
| 环境限制 | 与固体生成器一致，不受淹没或过热限制 |

## 原料范围

列表遍历当前游戏注册的生产配方：只要一个配方会产出食物或烹饪材料，它使用的可食用物品或烹饪材料就会进入生成器。肉、生蛋液、鱼肉、作物等原料会自动出现；熟肉、豆腐等本身又被其他食物配方使用的中间食材也会出现。启用 DLC 或其他 Mod 后，其新增原料只要使用原版配方系统和标准食物组件，也会自动加入。

水、泥土、蔗糖等元素资源不属于本生成器的物品范围，可分别使用液体或固体资源生成器。配方使用的分类标签没有对应单一物品时不会显示，例如蛋壳机接受的可孵化蛋类别；因此生成器提供生蛋液，但不会凭空生成可孵化蛋。最终成品如果不再作为其他食物的配方输入，也不会出现在原料列表中。

列表与实际生产共用预制体检查：物品必须仍然存在、标签匹配、可搬运，并具有有效单位质量。DLC 未加载、其他 Mod 移除配方或预制体异常时，对应选项会隐藏；旧存档已选择的失效原料会安全停机。

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

2026-11-14 修复包已使用本机安装的《缺氧》游戏程序集重新编译，并使用 PowerShell 自带 Roslyn 与 .NET 10 执行独立检查，全部通过；本机未安装 .NET SDK，因此此次没有执行上述项目构建命令。检查直接执行生产组件和资源列表补丁，覆盖从原版及 Mod 食物配方发现原料、排除元素和可孵化蛋、物品 ID、质量与单位换算、资源切换、断电、零速、速度边界和异常值、设置独立与复制、存档字段及读入值校验、无效时间步、无效格子、缺失或异常预制体、菜单作用范围及共享数据隔离。新增侧栏回归检查按当前游戏的分类缓存与回收池行为建模，旧代码可复现重复 `Void` 键异常，修复后跨生成器反复切换和刷新通过；这不替代真实游戏 UI 验收。

**尚未游戏内验收**：这些检查使用替身隔离游戏边界，不执行真实 Harmony 注入、UI 渲染、自动化网络、腐败、堆叠或存档序列化。需在实际游戏中确认菜单与滑块、120 W 耗电、红绿信号启停、当前 DLC 及其他 Mod 配方原料的显示和生产、保存重载以及与其他生成器同时启用的行为。声明最低版本为 718946、API 版本为 2；历史编译引用不能证明当前游戏版本完全兼容。

实现参考历史原版的 [MeatConfig](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/MeatConfig.cs)、[RawEggConfig](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/RawEggConfig.cs)、[ComplexFabricator](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/ComplexFabricator.cs) 与 [PrimaryElement](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/PrimaryElement.cs) 的物品生成及质量换算方式，实际行为以当前游戏为准。
