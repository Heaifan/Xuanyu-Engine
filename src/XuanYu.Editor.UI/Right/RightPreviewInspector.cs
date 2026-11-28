using Avalonia.Controls;
using XuanYu.Core.Geodesy;
using XuanYu.World.Ecs;
using XuanYu.Editor.Preview;
using XuanYu.Editor.UI.Left;
using XuanYu.World;

namespace XuanYu.Editor.UI.Right;

public sealed class RightPreviewInspector : RightWorkspacePanel
{
    private readonly PreviewScene _scene;
    private readonly Func<Guid, (double Orthometric, double? AboveGround)?>? _heights;
    private readonly Func<Guid, GeoCoordinate, bool, string?>? _move;
    private RightGeographicPositionEditor? _geoEditor;
    private Guid? _shownId;
    private string? _shownName;
    private bool _shownVisible;

    public RightPreviewInspector(PreviewScene scene,
        Func<Guid, (double Orthometric, double? AboveGround)?>? heights = null,
        Func<Guid, GeoCoordinate, bool, string?>? move = null)
        : base((key, size) => LeftSvgIcon.Create(key, size))
    {
        _scene = scene;
        _heights = heights;
        _move = move;
        _scene.Changed += Refresh;
        Refresh();
    }

    internal void RefreshGeographicPosition() => _geoEditor?.RefreshPosition();

    internal void RefreshTransform()
    {
        _shownId = null;
        Refresh();
    }

    public void Refresh()
    {
        PreviewObject? item = _scene.Selected;
        if (item is null)
        {
            if (_shownId is null)
                return;
            _shownId = null;
            _geoEditor = null;
            Show(null);
            return;
        }
        if (_shownId == item.Id && _shownName == item.Name &&
            _shownVisible == item.Visible)
            return;
        _shownId = item.Id;
        _shownName = item.Name;
        _shownVisible = item.Visible;

        var snapshot = new RightInspectorSnapshot(
            item.Name, "网格对象", item.IsGeographic ? "全球空间" : "本地空间", "Nodes/mesh",
            [
                new RightInspectorSection("基本信息",
                [
                    new("名称", item.Name),
                    new("对象类型", "网格实例")
                ]),
                new RightInspectorSection("网格数据",
                [
                    new("资产", item.Mesh.AssetId),
                    new("顶点数", $"{item.Mesh.VertexCount}"),
                    new("三角面数", $"{item.Mesh.TriangleCount}"),
                    new("拓扑", "TriangleList")
                ])
            ],
            item.Visible);
        Transform current = _scene.World.Find(item.Id)!.Transform;
        var orientation = new RightTransformEditor(current, next =>
            _scene.SetTransform(item.Id, item.IsGeographic
                ? Transform.FromSpatial(_scene.World.GetSpatialPosition(item.Id), next.Rotation, next.Scale) : next));
        Control editor = orientation;
        _geoEditor = null;
        if (item.IsGeographic)
        {
            _geoEditor = new RightGeographicPositionEditor(
                () => _scene.World.GetSpatialPosition(item.Id).ToGeo(),
                () => _heights?.Invoke(item.Id),
                (target, aboveGround) => _move is { } move ? move(item.Id, target, aboveGround)
                    : "当前没有可用的 G07 共享视口。");
            var group = new RightPropertyGroup("地理位置 · WGS84", (key, size) => LeftSvgIcon.Create(key, size));
            group.AddControl(_geoEditor);
            editor = new StackPanel { Children = { group, orientation,
                new TextBlock { Text = "视口快捷键：G 平移 · R 旋转 · S 缩放 · B 底部贴地 · F 聚焦",
                    FontSize = 11, TextWrapping = Avalonia.Media.TextWrapping.Wrap } } };
        }
        Show(snapshot, visible => _scene.SetVisible(item.Id, visible), editor);
    }
}