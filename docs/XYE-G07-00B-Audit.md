# XYE G07-00B 前期审计

日期：2026-11-26。范围：只读审计及基线测试；G07-01 未授权、未施工。依据 G07-R02 粘贴合同。以下施工文件名为建议，非已存在 API；接入位置实施时须复核。

## 工作区和验证

唯一工作区 `E:\MyDoc\project-VSCode\XuanYuEngine`；分支 `dev/e5-unified-viewport`；HEAD `765b3aa7212a6daf699a518daa6e726b881e79ec`。审计开始 `git status --porcelain=v1 --untracked-files=all` 有 209 条记录，包含大量既有 G06/E5、测试和 Vulkan WIP；不能将 HEAD 当作本地源码。未切分支、清理、暂存、修改生产源码、提交或推送。测试产生 ignored TestResults；本报告是新增文档。

上一轮 `dotnet build XuanYuEngine.sln -c Release --no-restore`：成功，0 警告 0 错误，14.85 秒；本轮按要求不重复构建。本轮 `dotnet test XuanYuEngine.sln -c Release --no-build --no-restore --logger "trx;LogFileName=G07-00B-baseline.trx"`：退出码 0，621 通过、0 失败、4 跳过，共 625，47 秒；原始结果 `tests/XuanYu.World.Tests/TestResults/G07-00B-baseline.trx`。4 个跳过项是 RealHgt_ReportsVisibleMeshErrors、RealHgt_VisibleL13ImprovesWithoutChangingTopologyOrEdges、RealN23E120_ProducesBoundLabelAndGap、AdjacentRealTiles_NativeOnAndOffBindOnlyMatchingDem；因未设置 `XYE_G06_R04_HGT`，不等于真实 HGT 验收。测试使用既有 Release 二进制，不证明执行期间源码无人修改；GUI、真实 Vulkan 调试/验证层和性能均 Not Run。未执行 run.bat：用户要求 GUI 由本人配合、不得 Computer Use。

## 数据定位

历史 G06 交接指向 `E:\MyDoc\dem-data`，已核实 12 个 ZIP。每个 ZIP 均含同名 `.hgt`（25,934,402 B，3601²×2）及 `.swb`、`.num`（各 12,967,201 B）。下表字节数及 SHA256 指 ZIP 原文件，不是解压后 HGT 哈希。对应 SWB 均为 ZIP 内同名文件，不需另找 sidecar。目录内未发现独立文件。

固定候选：`NASADEM_HGT_n23e120.zip`、`NASADEM_HGT_n23e121.zip`，分别覆盖 23–24°N、120–121°E 与 23–24°N、121–122°E；共享 121°E。直接解压读取大端 int16 比较两侧 3601 样点：双方有效 3601、非零 3601、不相等 0、最大差 0 m；此为源共享边证据，不是渲染接缝证据。两文件的 ZIP/HGT/SWB 尺寸按下表及上述说明。建议实施时再选完整处于覆盖内、跨经线的整数格网根并固定坐标及层级；本审计尚未验证特定根区域全部格点有效。

| ZIP 文件 | 字节 | SHA256 |
|---|---:|---|
| NASADEM_HGT_n21e120.zip | 188238 | 2EFC8F50848B330295E653EC65BA3EF71F00FA0950F42E5EF214F0DEE3D434A7 |
| NASADEM_HGT_n21e121.zip | 58558 | CBB6CB5313C77593E44DC35CB00512E5474DB372E48F9FBE11D15BA79E525372 |
| NASADEM_HGT_n22e120.zip | 8494419 | 903752A44A185FE3406531CF4E86126C566E3BA7603218AFE6A1D6005B3B068E |
| NASADEM_HGT_n22e121.zip | 1394114 | 8403B01E08594FEDAE3CFB82BA1BFD767D3E8046586B18FE224016A09D583BEF |
| NASADEM_HGT_n23e119.zip | 222978 | 62A05A6AE6620B6ED0426FD9D4A698ACA5365CD8C2B383FAD5CDC7DF6AAFCD14 |
| NASADEM_HGT_n23e120.zip | 12553424 | B5DFDD890FFF01EE85FBCD9553DE92BB28F756E5775DC9A774536EC544C618F4 |
| NASADEM_HGT_n23e121.zip | 10055374 | BFEE189271D725AACC93D70F556B05031832697616F4D3D193F2AFBC69C393C9 |
| NASADEM_HGT_n24e120.zip | 4801705 | 4CA7F5C5FA6D92B2A8BFE70FDB68FDAEA7A50D436200CB9204917A6BB35C0E71 |
| NASADEM_HGT_n24e121.zip | 14649577 | 4FAD3EC397874E3F08C6FB22C53789B742FB237DBA9317D7F9B041AA6ACCD23E |
| NASADEM_HGT_n24e122.zip | 76304 | 40E110DEDE73690B2ACC253D77B859D77C6C715836C166C9073E3AA2892CBF8A |
| NASADEM_HGT_n25e121.zip | 2051087 | 5D09454E72A6ACC55891A0C6560E8A609935C43ADB650045CC3B84AFF4213860 |
| NASADEM_HGT_n25e122.zip | 55355 | 90A89BD77067874FCE0D7C93E10683F2F81F123D5B697ED62307B82FB1260C68 |

## 生产调用链及实际能力

1. **导入→采样**：`src/XuanYu.Editor.UI/Top/Terrain/TerrainFileMenu.cs` 的 `Import` 通过 Avalonia `OpenFilePickerAsync` 多选 HGT/ZIP/XYT，`TryGetLocalPath` → `Workbench.ImportTerrainFiles`（`src/XuanYu.Editor.Core/Pages/Terrain/Multi/Workbench.TerrainMulti.cs`）。单文件走 `Workbench.ImportTerrain` → `NasademHgtFile.Load`；多文件构造 `NasademTileCatalog`，创建 `GeoTerrainRenderSession(catalog, cacheCapacity:16, useOverview:true, streamRaw:true)` 并读取 SWB。`src/XuanYu.Map/Import/NasademHgtFile.cs` 解码大端 short，`short.MinValue` 为无效值；`NasademWaterFile.cs` 支持 ZIP 内同名 SWB 或 HGT sidecar。`NasademElevationSource.Sample` 依次尝试所属瓦片和边界邻片，返回 nullable double；`SampleResident` 仅读驻留。`NasademTileCache.Prefetch.cs` 用一个 Task 后台读取、Pump 收尾，但 `NasademTileCache.Get` 可同步等待或解码；不可将现有所有采样当作非阻塞。当前 nullable 不区分缺页、无效和覆盖外；存在源瓦片但高度无效时 `TryRead` 可结束边界尝试，不能直接满足有效优先规则。
2. **坐标与相机**：`XuanYu.Core/Geodesy/Wgs84.cs` 的 `ToEcef`/`ToGeo`、`EnuFrame.cs` 的 `ToEnu`/`ToEcef`；`GeoTerrainRenderSession.Source.cs` 以覆盖中心建 ENU anchor，`GlobalCamera` 保存 ECEF 相机。`Global/Camera/GeoTerrainRenderSession.Camera.cs` 将眼点和目标转共同 ENU，再转 float 建 view/projection；`GeoTerrainMeshBuilder.BuildCore` 将零椭球高坐标转换到 anchor，再把 DEM 高程加到 ENU Up。该区域显示约定不能当作 EGM96→椭球高转换。
3. **选片→绘制**：`GeoTerrainRenderSession.Draws` → `GeoTileSelector.Select`（`XuanYu.Map/Tiling/Selection/GeoTileSelector.cs`；SSE 估计、数量预算及可选焦点优先）→ `UpdateVisibleCover` → `StitchVisibleTiles` → `UpdateNativeDetails`/`UpdateRawGeometry` → `CreateSelectionDraws`/`BindResidentDem` → `RenderDraw`。`GeoTerrainMeshBuilder.cs` 将每三角顶点展开；`Global/Coverage`、`Global/Native`、`Seams` 实现旧覆盖及拼边。现有 selector 的级别体系、误差估计和焦点优先不能直接视作 G07 合同。
4. **Vulkan 地形资源**：共享路径 `ViewportSharedHost.RenderAsync` → `VkSharedColorProbe.DrawScene` → `VkMeshScene.SyncMeshes/SyncDem/SyncWater` → `VkFrameCommands.Record/DrawMesh`。`VkDemTexture.Image.cs` 创建 device-local R16_SINT；`Staging.cs` 建 staging；`Transfer.cs` 提交复制、转布局并逐次 `QueueWaitIdle`，目标 barrier 为 fragment 阶段。`VkDemDescriptors` 的 binding 0 仅 FragmentBit、pool 14 sets；`VkMeshScene.DemBinding.BindDem` 绑定旧贴图；`VkFrameCommands.DrawMesh` 调用 `CmdDraw` 而非索引。`SyncDem` 按 HGT 坐标缓存最多 12，非活动资源经 `DestroyDem` 释放描述符和纹理。共享帧 `DrawScene` 在提交后等待 fence；`Dispose` 等设备 idle。不能直接将此上传/描述符和回收视为 G07 的非阻塞顶点采样方案；顶点访问需 stage/layout/barrier 合同和退休机制。另一 swapchain 路径见 `VkFrameRenderer.PresentFrame`。
5. **Shader**：`Vulkan/Pipeline/Shaders/triangle.vert`、`triangle.frag`、`water.glsl` 与现有 `.spv`；`XuanYu.Render.Vulkan.csproj` 嵌入 `*.spv`，`VkTriangleShaders.Create` 加载编译产物。仓库范围搜索未发现自动生成命令或构建 target；本机存在 `E:\MyApp\sdk-vulkan\Bin\glslc.exe` 和 glslangValidator，不能据此认定已知的可复现生成参数或现有二进制与源码一致。实施前须还原包含展开、目标环境、参数、产物验证的生成流程，不覆盖现有 WIP shader。
6. **拾取/HUD/碰撞**：`ViewportSharedHost.GeoHud.cs` → `Workbench.PickTerrainHud` → `GeoTerrainRenderSession.PickHudPoint` → `GeoTerrainCursorPicker.TryPick`，先 mesh 包围盒再扫描三角形，命中 ENU 转 ECEF/地理坐标。缩放锚点 `Collision/GeoTerrainRenderSession.Navigation.cs` 先拾取可见网格，失败再采样原始源迭代。`Source.cs` 给 `GlobalCamera.SetTerrainCollision` 注入 `SampleGroundHeight`、`SampleFocusHeight`、`SweepZoom`；`Collision.cs` 将原始/概览与可见 mesh 高度取最大，并在无结果时回退 0。当前不是提交帧 ID 绑定，也不是单一渲染表面合同。
7. **统一视口**：`EditorWindow.cs` 构造一个中央视口；`Viewport/Integration/EditorWindow.Viewport.cs:CreateViewport` 在 Windows + Vulkan 创建 `ViewportSharedHost`，关闭经 `StopAsync`，否则回退控件/交换链。`Workbench.ViewportDraws` 选择 runtime snapshot 或 `ComposeEditorDraws`（`Pages/Environment/Workbench.Environment.Draws.cs`），该方法依据当前工作区及 geo/local 状态组合场景、网格及地形；`ViewportSharedHost.RenderAsync` 获取 draws → 后台 `probe.DrawScene` → Avalonia keyed mutex 显示。实验入口应只对地形测试路径选择，不改变世界/本地地形共享视口。

## KEEP / ADAPT / NEW / LEGACY

- KEEP：HGT/ZIP 解码、目录索引、SWB 读取，WGS84/ECEF/ENU 基础数学，现有共享视口及本地 E5 工作流、原有普通模型绘制；保留 G06 测试与既有视觉。
- ADAPT：非阻塞原始页读取/共享边来源及状态、相机使用已提交帧的投影与碰撞接口、`RenderDraw` 到地形专用绘制的最小分流、Vulkan 场景接入与 fence 生命周期、顶点可见描述符及 shader 构建流程。既有 `VkDemTexture` 的 R16_SINT 能力可参考，但其同步构造路径不直接复用为异步上传。
- NEW：角秒地址和页状态、固定格网/16 mask 索引、区域选片和原子帧覆盖、异步页准备及字节预算、地形专用 GPU 上传和延迟回收、地形 pipeline/索引 draw、不可变已提交帧及精确三角面查询、诊断测试。
- LEGACY：`GeoTileSelector`、`GeoTerrainRenderSession`、Footprint/Native/缓存/拼边及旧 picker、旧 DEM 纹理路径保留可运行；联合验收后按授权处理，不提前删改或长期宣称双内核方案完成。

## G07-01 最小施工文件清单（建议，具体新文件需再按职责拆至 5+100）

以下为闭环所需的首批边界，不是保证只需这些文件；不得依此重构全引擎。

| 分类 | 路径 | 单文件职责 |
|---|---|---|
| NEW | `src/XuanYu.Map/Tiling/Unified/GeoSampleAddress.cs` | 全球整数角秒身份与瓦片父子关系 |
| NEW | `src/XuanYu.Map/Tiling/Unified/GeoHeightResult.cs` | Valid/NoData/NotLoaded/OutsideCoverage、版本及基准 |
| NEW | `src/XuanYu.Map/Tiling/Unified/GeoHeightSource.cs` | 适配 catalog、确定性重复边、有效优先，非阻塞页请求 |
| NEW | `src/XuanYu.Map/Tiling/Unified/GeoHeightPage.cs` | 33×33 页及有效性、源版本 |
| NEW | `src/XuanYu.Map/Tiling/Unified/GeoPageLoader.cs` | 有界后台 IO、请求身份和 CPU 缓存预算 |
| NEW | `src/XuanYu.Map/Tiling/Unified/Topology/GeoGridIndices.cs` | 固定三角化与 16 个边 mask |
| NEW | `src/XuanYu.Map/Tiling/Unified/Selection/GeoUnifiedSelector.cs` | 区域 SSE、2:1 平衡和父子准备事务 |
| NEW | `src/XuanYu.Map/Tiling/Unified/Selection/GeoGeometricError.cs` | 保守误差、边约束及包围体计算 |
| NEW | `src/XuanYu.Editor.Core/Preview/Terrain/Unified/GeoUnifiedFrame.cs` | 不可变帧 ID、视投影、页版本、叶和 mask |
| NEW | `src/XuanYu.Editor.Core/Preview/Terrain/Unified/GeoUnifiedSurface.cs` | 同帧格元三角形拾取及可视地面查询 |
| NEW | `src/XuanYu.Editor.Core/Preview/Terrain/Unified/GeoUnifiedSession.cs` | 区域实验路径、选片、页就绪与帧提交 |
| NEW | `src/XuanYu.Render/Scene/Terrain/RenderTerrainTile.cs` | 地形专用页身份、变换、mask、版本的绘制数据合同 |
| NEW | `src/XuanYu.Render.Vulkan/Vulkan/Terrain/VkTerrainUpload.cs` | staging 提交、完成 fence、在途字节与延迟回收 |
| NEW | `src/XuanYu.Render.Vulkan/Vulkan/Terrain/VkTerrainPage.cs` | 页纹理/视图及真实分配尺寸 |
| NEW | `src/XuanYu.Render.Vulkan/Vulkan/Terrain/VkTerrainIndexBuffers.cs` | 共用 16 个索引模板资源 |
| NEW | `src/XuanYu.Render.Vulkan/Vulkan/Terrain/VkTerrainDescriptors.cs` | 顶点阶段采样页的描述符生命周期 |
| NEW | `src/XuanYu.Render.Vulkan/Vulkan/Terrain/VkTerrainPipeline.cs` | 地形专用 pipeline，保持普通模型路径 |
| NEW | `src/XuanYu.Render.Vulkan/Vulkan/Terrain/VkTerrainDraw.cs` | 绑定地形资源并执行 CmdDrawIndexed |
| NEW | `src/XuanYu.Render.Vulkan/Vulkan/Pipeline/TerrainShaders/terrain.vert`、`terrain.frag` | 整数 texel 顶点采样和简单调试着色；对应 SPIR-V 通过已核实流程生成 |
| ADAPT | `src/XuanYu.Render/Scene/RenderDraw.cs` | 最小地形 draw 分流/引用，保持普通 draw 合同 |
| ADAPT | `src/XuanYu.Render.Vulkan/Vulkan/Scene/VkMeshScene.cs`、`src/XuanYu.Render.Vulkan/Vulkan/Frame/VkFrameCommands.cs` | 生命周期与条件索引绘制接入；按现有 partial 拆分避免挤占目录 |
| ADAPT | `src/XuanYu.Render.Vulkan/Vulkan/Interop/VkSharedColorProbe.Scene.cs` | 帧 fence 与上传就绪/退休对接；不重构其他渲染功能 |
| ADAPT | `src/XuanYu.Editor.Core/Pages/Terrain/Workbench.TerrainGeo.cs`、`src/XuanYu.Editor.Core/Pages/Environment/Workbench.Environment.Draws.cs` | 受控实验入口和绘制选择；旧路径保留 |
| ADAPT | `src/XuanYu.Editor.UI/Viewport/Terrain/ViewportSharedHost.GeoHud.cs`、`src/XuanYu.Editor.Core/Camera/Global/GlobalCamera.TerrainCollision.cs` | 当前实际显示帧的拾取和碰撞接入，旧路径保持 |
| NEW | `tests/XuanYu.World.Tests/Maps/Tiling/Unified/` 下分职责的 `test_*.cs` | 格网、共享边、全部 mask 的覆盖/朝向/无重叠、页状态、预算、帧查询与事务；真实数据通过显式配置 |
| NEW | 项目既有 Vulkan 真实运行测试位置下地形测试及日志脚本 | 实际 GPU 读回、资源退休、跨帧身份与性能，不用 CPU 模拟代替 GPU |

实现时还需识别独立 pipeline 接入、实际 GPU 读回和绑定接口的具体文件；以上并非现有接口已支持的承诺，也不是完整逐行施工设计。

## 5+100 预检和剩余问题

规则见 `docs/architecture_v0.1.md`：每目录不超过五个直接源文件、生产 C# 文件不超过 100 行。当前 `src/XuanYu.Render.Vulkan/Vulkan/Scene`、`src/XuanYu.Editor.Core/Pages/Terrain` 均已有 5 个直接文件；不得往其根目录直接新增。新功能按上表子目录拆分，但 `Unified` 建议首批即有 5 个文件，继续增长应再拆分；表内 `Vulkan/Terrain` 已列 5 个 C# 文件，同理需规划子目录。`Pipeline/Shaders` 已有 5 个直接文件（含 GLSL/SPIR-V），建议独立子目录并更新资源嵌入规则，核查资源名，不能直接往该目录添加。既有已修改文件的体积和目录状态在实施前逐项复核，新增/修改文件须分别满足单文件限制；本轮没有新生产 C# 文件，不能提前声称实现通过 5+100。

待解决：1) 确定实际跨边完整整数根并固定源哈希；2) 16 mask 含双边角的覆盖、朝向和相邻曲线数值证明；3) 描述符顶点可见、transfer→vertex barrier、页上传与最后使用 fence 的所有权/回收；4) CPU/GPU 坐标与三角面读回误差、图像对应帧 ID；5) 高程无效/重复边、粗父级有效性及保守误差；6) 项目 shader 编译参数和水体 include 可重复生成；7) 真实 Vulkan 验证层、G06/E5 GUI 与性能均未测。

## 结论

数据和基本源码入口已定位，Release 构建及默认全量单测基线可用，四项 P0 接口的实现边界已识别；**具备开始 G07-01 受控原型开发的前提，尚不具备 G07-01 验收通过的证据**。具体测试根、shader 生成流程和 GPU 同步合同是实施首批待解决事项，不应伪装成现成能力。本轮遵照明确停点，不进入生产施工。
