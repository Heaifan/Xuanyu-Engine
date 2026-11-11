using XuanYu.Editor.Preview;
using XuanYu.Editor.UI.Left;

namespace XuanYu.Editor.UI.Right;

public sealed class RightPreviewInspector : RightWorkspacePanel
{
    private readonly PreviewScene _scene;

    public RightPreviewInspector(PreviewScene scene)
        : base((key, size) => LeftSvgIcon.Create(key, size))
    {
        _scene = scene;
        _scene.Changed += Refresh;
        Refresh();
    }

    public void Refresh()
    {
        PreviewObject? item = _scene.Selected;
        if (item is null)
        {
            Show(null);
            return;
        }

        var snapshot = new RightInspectorSnapshot(
            item.Name, "网格对象", "预览场景", "Nodes/mesh",
            [
                new RightInspectorSection("基本信息",
                [
                    new("名称", item.Name),
                    new("对象类型", "网格实例")
                ]),
                new RightInspectorSection("网格数据",
                [
                    new("资产", $"{item.Mesh.AssetId}"),
                    new("顶点数", $"{item.Mesh.VertexCount}"),
                    new("三角面数", $"{item.Mesh.TriangleCount}"),
                    new("拓扑", "TriangleList")
                ])
            ],
            item.Visible);
        Show(snapshot, visible => _scene.SetVisible(item.Id, visible));
    }
}
