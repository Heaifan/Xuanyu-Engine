# 玄域新引擎 (XuanYuEngine) V0.1 架构与质量基线

## 1. 核心架构原则
1. **World 是游戏世界数据的权威所有者**：GPU 数据只是世界数据的呈现形式。
2. **Graphics 与 World 解耦**：World 不依赖 Render。
3. **Runtime 与 Editor 分离**：独立运行的游戏仅依赖 Core、World、Platform、Render、Runtime。
4. **先正确，再优化**：M0 优先验证无头 World 模拟（30 Hz 固定步长，米制真实地形）。

## 2. 程序集职责与单向依赖
- `XuanYu.Core`：基础类型、数学、标识符、诊断（零内部引擎依赖）。
- `XuanYu.World`：实体、空间变换、真实地形、模拟（仅依赖 Core）。
- `XuanYu.Platform`：窗口、输入、文件系统隔离（仅依赖 Core）。
- `XuanYu.Render`：图形与 Vulkan 渲染实现（依赖 Core、World、Platform）。
- `XuanYu.Runtime`：应用生命周期与系统调度（依赖 Core、World、Platform、Render）。
- `XuanYu.Editor`：编辑器界面与工具协作（依赖 Core、World、Platform、Render、Runtime）。

## 3. 5+100 质量审查标准
- **5**：一级模块最多五个核心职责方向，目录组织最多四层。
- **100**：对模块全部公开职责、关键成员、依赖、边界和异常路径进行 100% 审查。
- **测试规范**：测试文件统一使用 `test_` 前缀，严禁为通过测试而删除有意义的测试。
