using XuanYu.Editor.Preview;

namespace XuanYu.Editor.UI.Right;

public sealed partial class RightPreviewInspector
{
    private static RightInspectorSnapshot CreateSnapshot(PreviewObject item)
    {
        var mesh = item.Mesh;
        string type = mesh is null ? "无网格实体" : "网格对象";
        List<RightInspectorSection> sections =
        [new("基本信息", [new("名称", item.Name), new("对象类型", mesh is null ? type : "网格实例")])];
        if (mesh is null)
            sections.Add(new("表现数据", [new("主网格", "无")]));
        else
            sections.Add(new("网格数据", [new("资产", mesh.AssetId),
                new("顶点数", $"{mesh.VertexCount}"), new("三角面数", $"{mesh.TriangleCount}"),
                new("拓扑", "TriangleList")]));
        return new(item.Name, type, item.IsGeographic ? "全球空间" : "本地空间",
            mesh is null ? "Nodes/file" : "Nodes/mesh", sections, item.Visible);
    }
}