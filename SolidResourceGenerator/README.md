# 固体资源生成器

为《缺氧》新增一座仅靠电力生成指定固体元素的建筑，可与仓库中的空间加热器 Mod 分别启用。

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

选择范围是游戏当前可用的**固体元素**，例如砂岩、铁矿、精炼铁、塑料、冰；不包含食物、种子、装备、动物等实体。生成后仍遵循游戏的自然换热、融化、升华、放射性等规则。

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
