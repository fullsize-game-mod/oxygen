# 电解器：纯氧 / 纯氢 20°C

选中原版电解器，在建筑右侧点击「切换为纯氢」或「切换为纯氧」。每台电解器分别保存设置。已有电解器和新建电解器默认产纯氧，切换只影响此后生成的气体。

两种模式均以 1 kg/s、20°C 排放气体。排出的气体正常参与游戏换热。电解器维持原版的 1 kg/s 耗水和 120 W 耗电，沿用上一版的超压不停机设定。

## 安装

将 [OxygenOnly20C.zip](../dist/OxygenOnly20C.zip) 解压，把其中的 `OxygenOnly20C` 文件夹放入游戏的 `mods/Local/`，并在游戏的 Mod 列表启用。若已订阅旧版「电解器：仅氧气 20°C」，请先停用旧版，再启用本地版。

## 编译

```sh
dotnet build OxygenOnly20C/OxygenOnly20C.csproj -c Release "-p:GameManagedPath=/absolute/path/to/Managed"
```

编译产物位于 `dist/OxygenOnly20C/`。
