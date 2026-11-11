# 空间加热器温控 Mod

修改《缺氧》原版空间加热器，不新增建筑，不依赖 PLib。

- 选中空间加热器后，通过温度滑块或数字输入框设置 **−200～200°C**，支持一位小数。
- 每台建筑独立保存目标温度；新建筑默认 **70°C**。
- **基础过热温度为 1000°C**，原版建材耐热加成继续生效；熔点不变。
- **双向温控**：环境高于目标时主动制冷，低于目标时主动加热。例如环境 20°C、设定 −200°C，会持续移除热量，而不是仅仅关闭加热。
- 整机冷热换热量合计上限为 **18 kDTU/s**，工作时保留原版 **120 W** 耗电；无可换热介质或全部达到目标后待机。
- 每 200 ms 调整同一小行星内、半径 2 的可达非固体区域中的气体与液体。冷热格子的热量按热容量分配，每格单次换热不越过目标；固体、真空不直接修改。
- 原版固定自发热/排热和单向加热状态机已由双向控制器替换，防止产热抵消制冷。保留供电、自动化、损坏和淹没等 `Operational` 运行条件；自然导热仍由游戏处理。
- 制冷直接移除热量，不向本体或其他位置转移热量；不会瞬间把整间房变为设定温度。气体冷凝为液体后仍可换热，凝固后的固体不直接调温；持续外界热源、淹没停机或凝固都可能影响达温。
- 温度输入和单位跟随游戏的摄氏/华氏/开尔文设置；物理范围始终是 −200～200°C。
- 不修改液体加热器。

## 安装

安装包：`dist/AdjustableSpaceHeater.zip`。解压后将整个 `AdjustableSpaceHeater` 文件夹放入游戏的本地 Mod 目录：

| 系统 | 本地 Mod 目录 |
| --- | --- |
| Windows | `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local\` |
| macOS | `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/Local/` |
| Linux | `~/.config/unity3d/Klei/Oxygen Not Included/mods/Local/` |

Windows 文档目录若被 OneDrive 等重定向，以实际文档目录为准。`Local` 不存在时可以创建。

安装后的层级：

```text
mods/Local/AdjustableSpaceHeater/
  AdjustableSpaceHeater.dll
  mod.yaml
  mod_info.yaml
```

也可以直接复制 `dist/AdjustableSpaceHeater/`，不要把 ZIP 文件直接放入 `Local`。启动游戏，在 Mod 列表启用“空间加热器温控”，按游戏提示重启。先备份存档；避免同时启用其他修改空间加热器温控的 Mod。禁用本 Mod 后，自定义目标温度不再生效。

## 从源码编译

安装 .NET 8 或更高版本 SDK，使用**自己的当前游戏程序集**：

```sh
dotnet build AdjustableSpaceHeater/AdjustableSpaceHeater.csproj -c Release "-p:GameManagedPath=/完整路径/Managed"
```

`GameManagedPath` 指向包含以下 DLL 的目录：

- `Assembly-CSharp.dll`
- `Assembly-CSharp-firstpass.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`

也可以用环境变量 `ONI_MANAGED_PATH` 指定该目录。Windows/Linux 通常位于游戏安装目录的 `OxygenNotIncluded_Data/Managed`；macOS 在游戏 `.app` 包内部查找 `Managed`。首次构建需联网还原 Harmony 编译引用。

目标框架为 `netstandard2.1`，Harmony 编译引用为 `Lib.Harmony.Ref` 2.4.2；游戏提供运行时依赖。构建后自动生成 `dist/AdjustableSpaceHeater/`，不包含游戏、Unity 或 Harmony DLL。需要 ZIP 时压缩该文件夹即可。

## 验证状态与版本边界

- 已使用 **OniStubs 2.0.0（2024 年社区 API 参考）和 UnityEngine.Modules 2021.3.33** 完成 Release 编译：0 错误、0 警告。这些仅为本地编译参考，不随 Mod 分发，项目也不会自动使用旧参考代替游戏程序集。
- 已通过 `tests/ThermalBehavior` 的确定性检查：制冷/加热方向、目标反转、热容量、冷热混合绝对热量预算、单步不越过目标和温度范围边界。
- 已通过临时命令行程序直接执行 **1.1.0 成品 DLL** 的换热计算，在单格固定热容量模型中完成 **20→−200→200→0°C**，确认正负热量与功率限幅有效。该模型不包含实际物相变化或游戏导热，不能推算游戏内达温时间。
- **本机未安装《缺氧》，没有完成游戏内加载、侧栏显示、实际加热/制冷、物相变化、过热损伤、存档读写或 DLC 组合验证。** 编译与独立热模型检查不等于实机兼容性验证。
- `minimumSupportedBuild: 718946` 表示 Harmony 2.4.2 的版本下限，**不是已实测的游戏版本声明**。该 Mod 面向 Unity 6 / Harmony 2.4.2 分支；未来更新仍可能改变建筑 API。使用当前游戏程序集重新构建，可先检查编译层兼容性。

不依赖游戏程序集的计算检查：

```sh
dotnet run --project tests/ThermalBehavior/ThermalBehavior.csproj -c Release
```

### 游戏内验收

1. 建造两台空间加热器，确认侧栏可分别输入 −200、0、200°C，且无法输入范围外温度。
2. 通电后在 20°C 环境设定 −200°C，确认实际降温；改为 200°C 确认升温。达到目标后待机；断电或自动化禁用时不应换热。核对冷凝后的液体仍可降温，并留意淹没/凝固引起的运行限制。
3. 两台设置不同目标后保存并重载，确认各自设置保留；旧存档建筑应获得温控界面。
4. 用无耐热加成材料核对基础过热阈值 1000°C，再核对建材加成、断电、自动化禁用及液体加热器不受影响。

## 实现依据

- [Klei：Unity 6 分支推荐 .NET Standard 2.1，并升级 Harmony 至 2.4.2](https://forums.kleientertainment.com/forums/topic/170159-game-update-public-testing-718946/)。
- [社区游戏 API 编译参考 OniStubs](https://github.com/richardspence/OniStubs)。
- [历史 SimMessages 热量接口](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/SimMessages.cs)与 [HeatBulb 的负热量制冷用法](https://github.com/Jesus05/Oxygen_Not_Included_disasemble_ILSpy/blob/7c5b4165977fa80a81d71bb16d46fe5ba620811a/main/HeatBulb.cs)。历史 2019 年源码与 2024 年 API 参考不能证明当前版本内部行为未变。
- [维护中 Mod 的本地安装目录说明](https://github.com/peterhaneve/ONIMods#local-downloads)。

## 版本记录

- **1.1.0**：改为真正的双向温控，设定 −200°C 可主动制冷；替换原版固定产热，补充冷热切换与功率边界检查。
- **1.0.0**：仅可调整停止加热阈值，不提供制冷；已由 1.1.0 替代。
