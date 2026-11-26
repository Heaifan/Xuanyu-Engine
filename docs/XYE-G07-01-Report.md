# XYE G07-01 施工记录（未完成）

2026-11-26；本记录为中间状态，不是 Lane 或集成验收报告。

- 起始 HEAD：765b3aa7212a6daf699a518daa6e726b881e79ec；原工作树状态保存在 `artifacts/G07-01/status-before.txt`。
- 候选根 `(6,529,199)`：检查真实 N23E120/N23E121 数据，4,198,401 格点无 NoData，最小 -13 m，最大 3928 m；两个 ZIP SHA256 与 G07-00B 记录一致。诊断输出见 `artifacts/G07-01/region-validation.txt`。
- 新增 `src/XuanYu.Map/Tiling/Unified/Grid/GeoUnifiedTile.cs`：整数瓦片层级、原点、父子关系。
- 新增 `src/XuanYu.Map/Tiling/Unified/Topology/GeoUnifiedGridIndices.cs`：33×33 网格固定对角线、16 个边收缩索引变体。
- 新增 `tests/XuanYu.World.Tests/Maps/Tiling/Unified/Grid/test_G07TileAddress.cs`、`Topology/test_G07GridTopology.cs`：候选根地址与父子，16 个 mask 的面积、朝向、边配对和边界步长。未覆盖三维共边、实际重叠栅格化或 GPU。
- 定向测试：17 通过、0 失败；隔离 Release 输出全量测试：638 通过、0 失败、4 跳过（`tests/XuanYu.World.Tests/TestResults/G07-01-partial.trx`）。标准输出目录测试构建曾被运行中的 Editor 进程 PID 1464 锁住，出现 MSB3026 重试并取消；使用 `--artifacts-path artifacts/G07-01/build` 隔离后成功。没有关闭用户进程。
- 5+100：本轮新增生产文件分别 30 和 33 行，位于不同目录，测试文件亦不超过 100 行；此结论仅限本轮新增文件，非全项目验收。
- 未实现：完整统一数据源/异步页及状态、调度/误差/覆盖事务、Vulkan 资源与索引绘制、GPU/CPU 表面与帧身份、统一视口、GUI、性能、真实 GPU 数值验证、真实 HGT 专项。G07-01 未完成，Lane A/B 不得标为 PASS。
- 回退本轮新增源码/测试：仅在用户明确选择回退时移除上述 4 个新文件；不要 reset、clean 或覆盖既有改动。无既有生产文件改动需要备份。


## 编辑器关闭后继续施工（仍未完成）

- 未发现运行中的 Editor 进程；常规 Release 构建恢复：`dotnet build XuanYuEngine.sln -c Release --no-restore -v minimal`，成功、0 警告、0 错误，9.27 秒。
- 新增 `src/XuanYu.Map/Tiling/Unified/Sampling/GeoUnifiedSample.cs`：采样状态、值、数据版本和 EGM96 正高标记；`GeoUnifiedSamples.cs`：按全球整数角秒读取已驻留 HGT、优先处理共享边且优先源未就绪时返回 NotLoaded；`GeoUnifiedHeightPage.cs`：33×33 不同层级页、逐点状态和值。当前尚无异步加载、资源限制、版本变更和冲突诊断完整合同，不能把它当作完整 Lane A。
- 新增 `tests/XuanYu.World.Tests/Maps/Tiling/Unified/Sampling/G07DataFactAttribute.cs`、`test_G07SampleStates.cs`、`test_G07HeightPage.cs`：通过环境变量指定真实两片 HGT，测试共享边驻留状态和页与整数采样一致性；未配置真实源时按既有方式标记跳过。定向 G07 测试使用真实源，19/19 通过。
- 使用真实 G07 数据环境运行全量测试：640 通过、0 失败、4 跳过，`tests/XuanYu.World.Tests/TestResults/G07-01-current.trx`。另设置 `XYE_G06_R04_HGT` 执行 G06 真实 HGT 专项：4 通过、0 失败、0 跳过，`tests/XuanYu.World.Tests/TestResults/G07-01-real-g06.trx`。专项通过不意味着 GUI 或 GPU 验收通过。
- G07 的 GPU 渲染、异步加载、预算、误差与选片、帧/拾取、防穿透及统一视口仍未实现；G07-01 未完成，不能进行最终 GUI 验收。

## 本轮流式读取施工（未完成）

- 新增 `src/XuanYu.Map/Tiling/Unified/Streaming/GeoUnifiedTileLoader.cs`：后台读取、相同请求复用、保守预留及失败释放并允许重试。`test_G07Streaming.cs` 和 `test_G07StreamingFailure.cs` 使用真实数据覆盖去重、预算拒绝和失败重试，3/3 通过。
- 当前每项请求按三个 HGT 字节数作保守预留；该值不是对 ZIP 解压器、托管对象及 GC 内存的实测，不含生成页；成功资源尚不回收，也没有等待队列、版本切换、冲突诊断及渲染接入。不能称为满足 CPU 总预算合同，Lane A 未通过。
- 最终本轮全量测试 `dotnet test XuanYuEngine.sln -c Release --no-build --no-restore`：643 通过、0 失败、4 跳过；结果 `tests/XuanYu.World.Tests/TestResults/G07-01-streaming-retry.trx`。期间因全量测试与下一次测试构建重叠导致 testhost 锁文件，已取消重叠构建并按顺序重跑；未修改或关闭用户进程。
- Lane B 的三维共边、误差、调度及覆盖事务，Lane C/D/E 均未完成；无真实 GPU、GUI 或性能结果。G07-01 未完成。

## 生命周期工作进行中

- `GeoUnifiedTileLoader` 增加预算等待队列、显式释放、页字节预留及释放；保留失败清理和请求去重。`GeoUnifiedSamples.Add` 阻止同一版本替换已发布的瓦片。新增 `test_G07Residency.cs` 和 `test_G07Version.cs`；与流式测试合计 5 项通过。
- 重要未解决问题：`GeoUnifiedSamples` 保留 `NasademTile` 引用，而 `GeoUnifiedTileLoader.Release` 单独减少预算；二者尚未绑定所有权，调用 Release 后采样器仍可能持有数据。因此当前释放/计费 API 不能用作满足整体内存预算合同的证明。页预留也尚未自动绑定实际页生命周期；新版本数据的重装和冲突诊断尚未完成。
- 最新完整单测（启用 G07 真实数据）：645 通过、0 失败、4 跳过；结果 `tests/XuanYu.World.Tests/TestResults/G07-01-lifecycle-wip.trx`。构建与测试按顺序进行。此前真实 GPU、GUI 与性能均未运行。
- Lane A 尚未完成；Lane B/C/D/E 尚未实施到合同验收。不能把本轮进度当作 G07-01 完成。
