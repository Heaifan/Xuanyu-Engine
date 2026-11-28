using XuanYu.Editor.Preview.Persistence;
using XuanYu.Runtime;
using XuanYu.Render;

namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    private SceneRepresentationData? CaptureRepresentation(PreviewObject item)
    {
        if (!SupportsRepresentation(item)) return null;
        var kind = GetRepresentation(item.Id);
        var asset = GetSymbolAsset(item.Id);
        if (asset is not null && !File.Exists(asset.SourcePath))
            throw new InvalidOperationException($"场景引用的 SVG 不存在：{asset.SourcePath}");
        if (asset is null && kind == (item.Mesh is null ? EntityRepresentationKind.None : EntityRepresentationKind.Mesh)) return null;
        return new(kind, asset?.SourcePath, asset is null ? BillboardStyle.Default : GetSymbolStyle(item.Id));
    }

    private Dictionary<Guid, RepresentationSnapshot> PrepareRepresentations(SceneDocument document)
    {
        Dictionary<Guid, RepresentationSnapshot> result = [];
        foreach (var part in document.Entities)
        {
            if (part.Representation is not { } data) continue;
            RenderMesh? mesh = null;
            if (data.SvgPath is { } path)
            {
                if (!File.Exists(path)) throw new InvalidDataException($"场景引用的 SVG 不存在：{path}");
                mesh = SvgAssets.Resolve(SvgAssets.Register(path).Id);
            }
            result.Add(part.Entity.Id, new(data.Kind, mesh, data.Style));
        }
        return result;
    }

    private void RestoreRepresentation(Guid id, RepresentationSnapshot state)
    {
        if (state.Active == EntityRepresentationKind.Point)
            _bridge.BindRepresentation(id, state.Active, PointMesh);
        if (state.Active == EntityRepresentationKind.Proxy)
            _bridge.BindRepresentation(id, state.Active, CubeProxyMesh);
        _bridge.RestoreRepresentation(id, state);
    }
}