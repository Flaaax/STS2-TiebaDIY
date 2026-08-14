# TiebaDIY 多版本分发

TiebaDIY 使用与 FBE 相同的 JML Dispatch 结构，同时支持：

- `0.107.1`，范围 `[0.107.1, 0.108.0)`
- `0.111.0`，范围 `[0.111.0, 0.112.0)`

发布目录结构：

```text
TiebaDIY/
  TiebaDIY.json
  TiebaDIY.pck
  TiebaDIY.dll
  TiebaDIY.dispatch.json
  runtimes/
    0.107.1/TiebaDIY.Runtime.dll
    0.111.0/TiebaDIY.Runtime.dll
```

`TiebaDIY.dll` 是分派入口，实际模组代码位于对应版本的 Runtime DLL。JML Dispatch 只在构建期使用，不是模组的运行时依赖。

## RitsuLib 版本规则

- 游戏 `0.107.1`：`STS2.RitsuLib.Compat.0.107.1` `0.4.66`
- 游戏 `0.111.0`：`STS2.RitsuLib` `0.5.12`
- manifest 依赖：`STS2-RitsuLib >= 0.5.12`

这些规则与 FBE 保持一致。

## 版本差异代码

项目会自动生成 `STS2_Stable` 或 `STS2_Beta` 条件编译符号：

```csharp
#if STS2_Stable
// 稳定版 ABI
#elif STS2_Beta
// Beta ABI
#endif
```

版本矩阵统一维护在 `TiebaDIY.csproj` 的 `Sts2SupportedVersion` 中；升级 Beta 版本时同步更新 `Sts2BetaVersion`、Beta 上界、依赖与 `build.bat` 的最新版本即可，不需要重命名条件编译宏。

## 打包命令

```text
build              打包最新支持版本
build 0.107.1      只打包指定版本
build all          打包全部支持版本
```

本工作区的 AGENTS.md 禁止代理执行构建；以上命令仅供开发者手动使用。
