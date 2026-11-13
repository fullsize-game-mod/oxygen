# 空间加热器温控 · Adjustable Space Heater

[返回 Mod 列表](../README.md) · 简体中文 | [English](README.en.md)

为《缺氧》（Oxygen Not Included）新增独立建筑 **“空间温控器”**：**低于目标温度时加热，高于目标温度时主动制冷**。开局即可在“实用”菜单建造，无需研究，复用原版空间加热器外观，不依赖 PLib。

> 当前版本：**1.2.0**。建筑配置与换热计算检查已通过，但尚未完成游戏内验证；使用前请备份存档。英文 README 不代表游戏内文案已英文化，目前 Mod 名称和自定义提示仍为中文。

## 功能

| 项目 | 行为 / 参数 |
| --- | --- |
| 建造入口 | “实用”菜单，开局解锁，无需研究 |
| 占地 / 建材 | 2×2 格，400 kg 金属矿石或精炼金属，地面建造 |
| 目标温度 | −200～200°C，滑块或数字输入，支持一位小数 |
| 默认目标 | 70°C，每台建筑独立保存设置 |
| 双向温控 | 主动加热或制冷；没有可换热介质或全部达到目标后待机 |
| 换热功率 | 整机加热与制冷的绝对热量合计不超过 18 kDTU/s |
| 工作耗电 | 保留原版 120 W 耗电 |
| 调整频率 | 每 200 ms 更新一次 |
| 作用范围 | 同一小行星内、半径 2 格范围中的可达非固体区域 |
| 换热介质 | 气体与液体；不直接修改固体或真空 |
| 基础过热温度 | 1000°C，保留建材耐热加成，不改变材料熔点 |
| 温度单位 | 跟随游戏的摄氏 / 华氏 / 开尔文设置，物理范围始终为 −200～200°C |

独立建筑仅由双向控制器换热，不带原版固定自发热、排热或单向加热状态机，避免产热抵消制冷。供电、自动化、损坏和淹没等 `Operational` 运行条件仍然生效，自然导热仍由游戏处理。自动化绿灯允许工作、红灯停机，未接线时允许工作。原版空间加热器和液体加热器不再被本 Mod 修改。

## 安装

1. 下载仓库中的 [AdjustableSpaceHeater.zip](../dist/AdjustableSpaceHeater.zip) 并解压，也可以直接复制 [dist/AdjustableSpaceHeater/](../dist/AdjustableSpaceHeater/) 文件夹。
2. 将整个 `AdjustableSpaceHeater` 文件夹放入对应系统的本地 Mod 目录；`Local` 不存在时可手动创建。
3. 启动游戏，在 Mod 列表启用 **“空间加热器温控”**，按游戏提示重启。

| 系统 | 本地 Mod 目录 |
| --- | --- |
| Windows | `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\` |
| macOS | `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/Local/` |
| Linux | `~/.config/unity3d/Klei/Oxygen Not Included/mods/Local/` |

Windows 的文档目录若被 OneDrive 等重定向，以实际文档目录为准。安装后的目录层级应为：

```text
mods/Local/AdjustableSpaceHeater/
├── AdjustableSpaceHeater.dll
├── mod.yaml
└── mod_info.yaml
```

不要把 ZIP 文件直接放进 `Local`，也不要多套一层同名文件夹。

## 使用

1. 新建或载入存档，在“实用”菜单选择“空间温控器”，用 400 kg 金属矿石或精炼金属建造，无需研究。
2. 在侧栏通过温度滑块或数字输入框设置目标温度。
3. 例如，在 20°C 环境设定 −200°C 会主动移除热量；改为 200°C 则会主动加热。
4. 接通电力并满足运行条件后开始温控。每台建筑可设置不同目标，设置随存档保存。

### 从 1.1.0 及更早版本升级

旧版修改的是原版空间加热器；1.2.0 改用独立建筑 ID，**不会自动转换已有建筑**。原版空间加热器将恢复原版的加热、产热和过热阈值，旧温控设置不再生效。需要双向温控时，请另建“空间温控器”并重新设置目标。移除本 Mod 前应拆除独立建筑，避免存档缺少建筑定义。

### 工作边界

- **不是瞬间恒温**：换热受整机功率、介质质量和比热容限制；单格单次换热计算不会越过目标，游戏内温度仍会受到自然导热和物相变化影响。
- **制冷直接删除热量**：移除的热量不会转移给建筑本体或其他位置，这不是热量守恒的热泵模型。
- **不直接调节固体温度**：气体冷凝后的液体仍可换热，凝固后的固体不再由控制器直接调温。
- **仍受停机条件限制**：断电、自动化禁用、损坏或淹没等条件可能阻止工作；持续外部热源也可能使目标温度无法达到。
- **过热阈值不等于熔点**：1000°C 是基础过热阈值，不能阻止材料在自身熔点熔化。

## 兼容性与验证状态

- 面向 **Unity 6 / Harmony 2.4.2** 分支，目标框架为 `netstandard2.1`。
- `mod_info.yaml` 声明 `minimumSupportedBuild: 718946`、`APIVersion: 2`。718946 对应项目采用的 Harmony 版本下限，**不代表已在该游戏版本完成实测**。
- `tests/ThermalBehavior` 检查已通过：独立建筑注册、名称、开局建材、耗电和自动化配置、不修改原版建筑与研究，以及加热 / 制冷方向、功率预算、冷热混合格子的绝对热量预算、热容量、不越过目标、目标反转和温度范围边界。
- 已使用 OniStubs 2.0.0 与 Unity 编译引用完成构建；引用包早于声明的最低游戏版本，不能证明当前游戏 API 完全兼容。
- 建筑检查使用替身隔离游戏边界，不执行真实 Harmony 注入或科技树；换热检查只执行独立计算。它们**不覆盖**游戏加载、菜单与侧栏显示、实际物相变化、过热损伤、存档读写或 DLC 组合。游戏内兼容性仍待验证。
- 游戏更新可能改变建筑 API。使用自己的当前游戏程序集重新编译，可以先检查编译层兼容性，但不能替代游戏内验收。

## 从源码编译

需要 [.NET SDK](https://dotnet.microsoft.com/download) 8 或更高版本，以及自己安装的《缺氧》当前游戏程序集。以下命令均在仓库根目录执行。

`GameManagedPath` 必须指向包含以下文件的 `Managed` 目录：

```text
Assembly-CSharp.dll
Assembly-CSharp-firstpass.dll
UnityEngine.dll
UnityEngine.CoreModule.dll
```

Windows / Linux 通常位于游戏安装目录的 `OxygenNotIncluded_Data/Managed`；macOS 需在游戏 `.app` 包内查找 `Managed`。将示例路径替换为实际路径：

```sh
dotnet build AdjustableSpaceHeater/AdjustableSpaceHeater.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
```

也可以通过环境变量 `ONI_MANAGED_PATH` 指定该目录。首次构建需要联网还原 `Lib.Harmony.Ref` 2.4.2 编译引用；游戏负责提供运行时依赖。

构建成功后自动生成 `dist/AdjustableSpaceHeater/`，其中仅包含 Mod DLL 和两个 YAML 文件，不包含游戏、Unity 或 Harmony DLL。构建**不会自动更新 ZIP**；发布前需将生成的整个文件夹重新压缩为 `dist/AdjustableSpaceHeater.zip`。

## 运行独立检查

此检查不需要游戏程序集。测试项目目标为 `net8.0`，需要 .NET 8 SDK，或可构建该目标的更高版本 SDK 配合 .NET 8 运行时。

```sh
dotnet run --project tests/ThermalBehavior/ThermalBehavior.csproj -c Release
```

通过时输出：

```text
PASS: independent building registration, strings, starting materials, power, automation and no vanilla patches.
PASS: heating/cooling direction, power budget, mixed cells, heat capacity, no overshoot, target reversal and range boundaries.
```

### 游戏内验收清单

- 新建未研究任何科技的存档，确认“实用”菜单直接显示“空间温控器”，可用金属矿石建造并通电工作。
- 建造两台空间温控器，分别输入 −200、0、200°C，确认侧栏限制输入范围且两台设置互不影响。
- 在 20°C 环境设定 −200°C，确认实际降温；再设为 200°C，确认实际升温；达到目标后应待机。
- 检查断电、自动化禁用等状态会停止换热；检查冷凝后的液体仍可降温，并留意淹没和凝固导致的限制。
- 设置不同目标后保存并重载，确认设置保留；旧存档也应能直接建造独立温控器。
- 确认自动化口在预览、建造中及完成后可见，红灯停机、绿灯恢复、未接线时允许工作。
- 用无耐热加成的材料检查 1000°C 基础过热阈值，再核对建材加成；原版空间加热器与液体加热器应保持原版行为。

## 源码结构

```text
AdjustableSpaceHeater/
├── README.md                         # 中文说明
├── README.en.md                      # 英文说明
├── AdjustableSpaceHeater.csproj      # 构建与分发配置
├── Mod.cs                            # Mod 入口与提示文案
├── AdjustableSpaceHeaterConfig.cs    # 独立建筑定义
├── Patches.cs                        # 开局建筑菜单注册
├── HeaterTemperatureControl.cs       # 温控界面与模拟更新
├── TemperatureSettings.cs            # 温度、功率与范围参数
├── ThermalMath.cs                    # 纯换热计算
├── mod.yaml                          # Mod 名称与描述
└── mod_info.yaml                     # 版本与游戏 API 要求
```

## 版本记录

- **1.2.0**：改为独立“空间温控器”，在“实用”菜单开局解锁，无需研究；恢复原版空间加热器行为，旧建筑不自动转换。
- **1.1.0**：改为真正的双向温控，支持主动制冷；替换原版固定产热，补充冷热切换与功率边界检查。
- **1.0.0**：仅调整停止加热阈值，不提供制冷；已被 1.1.0 替代。

## 参考资料

- [Klei：Unity 6 分支的 .NET Standard 2.1 与 Harmony 2.4.2 说明](https://forums.kleientertainment.com/forums/topic/170159-game-update-public-testing-718946/)
- [ONIMods：本地 Mod 安装目录](https://github.com/peterhaneve/ONIMods#local-downloads)
- [.NET CLI：dotnet build](https://learn.microsoft.com/dotnet/core/tools/dotnet-build) 与 [dotnet run](https://learn.microsoft.com/dotnet/core/tools/dotnet-run)
- [OniStubs：社区游戏 API 编译参考](https://github.com/richardspence/OniStubs)
- 历史接口参考：[SimMessages](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/SimMessages.cs) 与 [HeatBulb 负热量制冷用法](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/HeatBulb.cs)。历史源码与社区 API 参考不能证明当前游戏版本的内部行为未变。
