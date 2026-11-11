# 玄域新引擎 V0.1 架构与质量基线

## 1. 基本规则

1. World 持有权威世界状态；GPU 数据只是展示形式。
2. World 不依赖 Render；游戏模拟与渲染帧率解耦。
3. Runtime 不依赖 Editor 或任何具体图形 API。
4. 当前阶段优先打通真实 Vulkan 渲染路径，不提前设计无消费者的公共接口。

## 2. 程序集职责和依赖

- `XuanYu.Core`：基础类型、数学与诊断，无其他引擎项目依赖。
- `XuanYu.World`：世界状态及模拟，仅依赖 Core。
- `XuanYu.Platform`：平台基础能力，仅依赖 Core。
- `XuanYu.Render`：通用渲染合同归属，当前仅保留程序集标记，依赖 Core。
- `XuanYu.Render.Vulkan`：Silk.NET.Vulkan 实现，依赖 Render。
- `XuanYu.Runtime`：运行时组合与调度，依赖 Core、World、Platform、Render。
- `XuanYu.Editor`：Avalonia 编辑器与 Vulkan 组合入口，依赖 Core、World、Platform、Render、Render.Vulkan、Runtime。

## 3. E1-02A 边界

- 从 Render 原路径迁移 Vulkan 文件，保留命名空间 `XuanYu.Render.Vulkan`。
- `VkContext`、Surface、Swapchain、Frame 等归属于 Render.Vulkan。
- Editor 可以在组合入口创建 `VkContext`，普通 World/Runtime 不引用 Vulkan。
- 本轮不引入 Graphics Pipeline、RenderGraph、Mesh/Material 抽象。
- E1-02 的 GPU 清屏和 Present 行为不得变化。

## 4. 质量与验证

- 5：一级模块最多五个核心职责方向，目录结构保持浅层，单目录不超过五个直接源文件。
- 100：单个生产 C# 文件不超过 100 行；审查公开职责、关键成员、依赖、边界和异常路径。
- 测试文件使用 `test_` 前缀，不得为了让测试通过而删除有意义的测试。
- `test_ArchitectureBaseline` 检查项目引用及程序集边界。
- `test_VkContext` 继续验证实际 Vulkan 设备生命周期。
- Release 测试通过后，用 `run.bat` 人工验收中央视口清屏、调整窗口大小和关闭程序。
