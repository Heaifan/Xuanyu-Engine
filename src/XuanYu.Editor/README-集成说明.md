# XYE-E1-03B-R1 · Right 共享图标版

## 本版结论

- Right 复用用户上传的 `Assets/Left/...` **原始 10 个 SVG**；字节不改、路径不改。它们虽然位于 `Left/` 目录，但当前 Left 和 Right 可以共用同一份资源。**不创建 `Assets/Right`，不复制绘制另一套同义 SVG。**
- 5 个 Right C# 文件，均不超过 100 行；同一目录共 5 个生产 C# 文件。
- 删除旧的 RightObjectHeader 中硬编码的 mesh/cube/object 几何 path。分组箭头支持复用 Left 的 `chevron-down/right` SVG。
- Right 不新增 SVG 解析器、NuGet 依赖，也不改变 Left 任何 C# 代码和 SVG 资源路径。

## 图标 key → 已有 SVG

| key | SVG 路径（相对于 `src/XuanYu.Editor/`） | 用途 |
| --- | --- | --- |
| `mesh` | `Assets/Left/Nodes/mesh.svg` | 三角面、立方体及通用网格 |
| `model` | `Assets/Left/Types/model.svg` | GLB 等模型资产 |
| `scene` | `Assets/Left/Nodes/scene.svg` | World/场景对象 |
| `file` | `Assets/Left/Nodes/file.svg` | 一般文件 |
| `folder` | `Assets/Left/Nodes/folder.svg` | 文件夹 |
| `data` | `Assets/Left/Types/data.svg` | 数据 |
| `image` | `Assets/Left/Types/image.svg` | 图片 |
| `code` | `Assets/Left/Types/code.svg` | 代码 |
| `chevron-down` | `Assets/Left/Controls/chevron-down.svg` | 折叠组展开状态 |
| `chevron-right` | `Assets/Left/Controls/chevron-right.svg` | 折叠组收起状态 |

先前提议的新 `cube.svg` 使用现有 `mesh.svg` 取代，`object.svg` 使用现有 `scene.svg` 或 `model.svg` 取代。此包没有重复的 `Right/*.svg`。

## CV 规则

工作区：`E:\MyDoc\project-VSCode\XuanYuEngine`

1. 将包内 `src/XuanYu.Editor/UI/Right/*.cs` 粘贴至同名路径。若你已 CV 上一包，请覆盖这 5 个文件。
2. 将 `src/XuanYu.Editor/Assets/Left/**/*.svg` 合并至原有 `Assets/Left/` 路径：若本地这些 SVG 已存在且内容相同，直接跳过，无须覆盖。
3. **删除你从旧包新增的 `src/XuanYu.Editor/Assets/Right/{mesh,cube,object}.svg`**（仅限这三个旧包新建的冗余文件）。不要删除任何 Left SVG。
4. 暂不删除旧 `PreviewInspector.cs`，不覆盖 `EditorWindow.cs`，不修改 `LeftSvgIcon.cs`。

## 图标接线合同（不是假定已完成的代码）

`RightPreviewInspector` 第三个可选参数是 `Func<string,int,Control>? iconFactory`；可在现有 EditorWindow/Composition 接线时传入已有 `LeftSvgIcon.Create` 工厂（须先核对其可访问性和 key 映射）。

- 传入的工厂应支持上表 key，返回可复用的 *新控件实例*，不要重复把同一个 Control 放入两个父容器。
- 如果现有 `LeftSvgIcon` 仅接受其他 key，应在集成入口**对 key 做一次映射**，而不是复制 SVG 或再编写解析器。
- 没有注入工厂时，面板显示中性菱形和文字符号作为回退，不会假装已经加载 SVG。
- `RightInspectorSnapshot.IconKey` 只能采用受支持的语义 key；三角面和立方体传 `mesh`，GLB 传 `model`，World 实体传 `scene`。
- 选择状态只从当前 `PreviewScene.Selected` 读取；可见性通过现有 `PreviewScene.SetVisible()` 回写。**本包未接入真实本地接口**。

## 工程状态

- 已完成：共享 SVG 资源归并、Right UI 工厂接口、默认安全回退、静态行数和 SVG XML 验证。
- 未完成：对本地 `LeftSvgIcon.cs` / `PreviewScene` / `EditorWindow.cs` 的实际接线、Release Build、GUI 验收。远端旧分支没有今天的本地最新版，不能捏造这部分类型签名或宣称 PASS。
- 本包是完整代码文件，不是覆盖当前三栏 Editor 的全量替换补丁。
