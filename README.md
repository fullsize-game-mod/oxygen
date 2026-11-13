# 缺氧 Mod 集合

简体中文 | [English](README.en.md)

本仓库用于开发和分发《缺氧》（Oxygen Not Included）的 Mod。每个 Mod 使用独立的源码目录和安装包，可按需分别安装、启用。

## Mod 列表

| Mod | 功能简介 | 详细说明 | 下载 |
| --- | --- | --- | --- |
| 空间加热器温控 | 为原版空间加热器添加 −200～200°C 双向温控，低于目标加热、高于目标制冷 | [使用、参数与限制](AdjustableSpaceHeater/README.md) | [AdjustableSpaceHeater.zip](dist/AdjustableSpaceHeater.zip) |
| 固体资源生成器 | 新增通电生成指定固体元素的建筑，速度可调 0～100 kg/s，支持自动化启停 | [使用、资源范围与限制](SolidResourceGenerator/README.md) | [SolidResourceGenerator.zip](dist/SolidResourceGenerator.zip) |
| 液体资源生成器 | 新增通电生成指定液体元素并经液体管道输出的建筑，速度可调 0～100 kg/s，支持自动化启停 | [使用、管道输出与限制](LiquidResourceGenerator/README.md) | [LiquidResourceGenerator.zip](dist/LiquidResourceGenerator.zip) |
| 食物原料生成器 | 新增通电生成肉、生蛋液、鱼肉与作物等原料的建筑，速度可调 0～100 kg/s，支持自动化启停 | [使用、原料范围与限制](FoodResourceGenerator/README.md) | [FoodResourceGenerator.zip](dist/FoodResourceGenerator.zip) |

各 Mod 的版本、配置、使用方式和验证范围以对应目录中的说明为准。目前尚未完成游戏内验收，使用前请备份存档。

## 安装

1. 从上表下载需要的 Mod 安装包并解压，也可以复制 [dist/](dist/) 中对应的 Mod 文件夹。
2. 将整个 Mod 文件夹放入对应系统的本地 Mod 目录；`Local` 不存在时可手动创建。
3. 启动游戏，在 Mod 列表启用对应 Mod，按游戏提示重启。

| 系统 | 本地 Mod 目录 |
| --- | --- |
| Windows | `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\` |
| macOS | `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/Local/` |
| Linux | `~/.config/unity3d/Klei/Oxygen Not Included/mods/Local/` |

Windows 的文档目录若被 OneDrive 等重定向，以实际文档目录为准。以同时安装四个 Mod 为例，目录层级应为：

```text
mods/Local/
├── AdjustableSpaceHeater/
│   ├── AdjustableSpaceHeater.dll
│   ├── mod.yaml
│   └── mod_info.yaml
├── SolidResourceGenerator/
│   ├── SolidResourceGenerator.dll
│   ├── mod.yaml
│   └── mod_info.yaml
├── LiquidResourceGenerator/
│   ├── LiquidResourceGenerator.dll
│   ├── mod.yaml
│   └── mod_info.yaml
└── FoodResourceGenerator/
    ├── FoodResourceGenerator.dll
    ├── mod.yaml
    └── mod_info.yaml
```

不要把 ZIP 文件直接放进 `Local`，也不要多套一层同名文件夹。

## 从源码编译

需要 [.NET SDK](https://dotnet.microsoft.com/download) 8 或更高版本，以及自己安装的《缺氧》游戏程序集。以下命令均在仓库根目录执行。

`GameManagedPath` 必须指向包含以下文件的 `Managed` 目录：

```text
Assembly-CSharp.dll
Assembly-CSharp-firstpass.dll
UnityEngine.dll
UnityEngine.CoreModule.dll
```

Windows / Linux 通常位于游戏安装目录的 `OxygenNotIncluded_Data/Managed`；macOS 需在游戏 `.app` 包内查找 `Managed`。将示例路径替换为实际路径，按需编译对应 Mod：

```sh
dotnet build AdjustableSpaceHeater/AdjustableSpaceHeater.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
dotnet build SolidResourceGenerator/SolidResourceGenerator.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
dotnet build LiquidResourceGenerator/LiquidResourceGenerator.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
dotnet build FoodResourceGenerator/FoodResourceGenerator.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
```

也可以通过环境变量 `ONI_MANAGED_PATH` 指定该目录。首次构建需要联网还原 `Lib.Harmony.Ref` 2.4.2 编译引用；游戏负责提供运行时依赖。

构建成功后自动更新 `dist/<Mod 名称>/`，其中仅包含对应 Mod 的 DLL 和两个 YAML 文件，不包含游戏、Unity 或 Harmony DLL。构建**不会自动更新 ZIP**；发布前需将生成的整个 Mod 文件夹重新压缩为对应安装包。

## 运行独立检查

以下检查不需要游戏程序集。测试项目目标为 `net8.0`，需要 .NET 8 SDK，或可构建该目标的更高版本 SDK 配合 .NET 8 运行时。

```sh
dotnet run --project tests/ThermalBehavior/ThermalBehavior.csproj -c Release
dotnet run --project tests/SolidGeneration/SolidGeneration.csproj -c Release
dotnet run --project tests/LiquidGeneration/LiquidGeneration.csproj -c Release
dotnet run --project tests/FoodGeneration/FoodGeneration.csproj -c Release
```

- `ThermalBehavior`：空间加热器的换热方向、功率预算、热容量与温度边界等计算检查。
- `SolidGeneration`：固体资源生成器的生产速率、运行条件、设置与无效输入等独立检查，使用替身隔离游戏边界。
- `LiquidGeneration`：液体资源生成器的生产速率、管道连接与堵塞、缓冲容量、液态温度、设置与资源列表检查，使用替身隔离游戏边界。
- `FoodGeneration`：食物原料生成器的物品类型、质量与单位换算、生产速率、设置、缺失原料和资源列表检查，使用替身隔离游戏边界。

这些检查不能替代游戏内验收；侧栏显示、自动化网络、真实存档读写和 DLC 组合等仍需在游戏中确认。各 Mod 的详细验证范围见对应说明。

## 兼容性

当前四个 Mod 均以 `netstandard2.1` 为目标框架，引用 Harmony 2.4.2；各自的 `mod_info.yaml` 声明 `minimumSupportedBuild: 718946`、`APIVersion: 2`。这些配置不代表已在对应游戏版本完成实测。游戏更新后，应使用当前游戏程序集重新编译并进行游戏内验证。

## 项目结构

```text
.
├── README.md                         # 仓库中文总览
├── README.en.md                      # 仓库英文总览
├── AdjustableSpaceHeater/            # 空间加热器温控源码与详细说明
├── SolidResourceGenerator/           # 固体资源生成器源码与详细说明
├── LiquidResourceGenerator/          # 液体资源生成器源码与详细说明
├── FoodResourceGenerator/            # 食物原料生成器源码与详细说明
├── tests/
│   ├── ThermalBehavior/              # 换热计算检查
│   ├── SolidGeneration/              # 固体资源生产检查
│   ├── LiquidGeneration/             # 液体资源与管道边界检查
│   └── FoodGeneration/               # 食物原料生产与物品边界检查
└── dist/                             # 各 Mod 的可安装文件夹与 ZIP 包
```
