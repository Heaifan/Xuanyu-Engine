using XuanYu.Render;
using XuanYu.Runtime;
using XuanYu.World;

namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    private static readonly RenderMesh CubeProxyMesh = PreviewCubeProxyMesh.Create();

    public static bool SupportsRepresentation(PreviewObject item) =>
        item.Mesh.AssetId == "builtin/cube";

    public EntityRepresentationKind GetRepresentation(Guid id) =>
        _bridge.GetRepresentation(id);

    public RenderMesh? GetActiveMesh(Guid id) => _bridge.GetActiveMesh(id);

    public void SetRepresentation(Guid id, EntityRepresentationKind kind)
    {
        var item = _objects.First(x => x.Id == id);
        if (!SupportsRepresentation(item))
            throw new InvalidOperationException("当前仅支持立方体的表现切换。");
        if (_bridge.GetRepresentation(id) == kind) return;
        if (kind == EntityRepresentationKind.Proxy)
            _bridge.BindRepresentation(id, kind, CubeProxyMesh);
        _bridge.SetRepresentation(id, kind);
        Publish();
    }

    internal WorldRenderBridge CreateRuntimeBridge(WorldScene runtime) =>
        _bridge.CopyFor(runtime);
}