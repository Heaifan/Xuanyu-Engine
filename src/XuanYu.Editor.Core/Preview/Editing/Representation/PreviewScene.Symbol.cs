using System.Numerics;
using XuanYu.Render;
using XuanYu.Runtime;
using XuanYu.World.Ecs;

namespace XuanYu.Editor.Preview;

public sealed partial class PreviewScene
{
    private static readonly RenderMesh PointMesh = new("builtin/point", new float[]
    {
        0,-.5f,0,1,1,0,1, .5f,0,0,1,1,0,1, 0,.5f,0,1,1,0,1,
        0,-.5f,0,1,1,0,1, 0,.5f,0,1,1,0,1, -.5f,0,0,1,1,0,1
    });
    public SvgAssetCatalog SvgAssets { get; } = new();
    public UnitIconLibrary UnitIcons { get; }

    public PreviewObject CreateSymbolEntity(string prefix, string path, SpatialPosition position = default)
    {
        var asset = SvgAssets.Register(path);
        var mesh = SvgAssets.Resolve(asset.Id);
        var entity = World.Create($"{prefix}_{NextNumber(prefix):000}", position);
        var item = RegisterObject(entity.Id, null);
        _bridge.BindRepresentation(item.Id, EntityRepresentationKind.Symbol, mesh);
        UnitIcons.Remember(asset.SourcePath);
        Select(item.Id);
        return item;
    }

    public SvgAssetEntry BindSymbol(Guid id,string path)
    {
        var item=_objects.First(x=>x.Id==id);
        var before = _bridge.CaptureRepresentation(id);
        if (!SupportsRepresentation(item)) throw new InvalidOperationException("当前资产不支持二维表现。");
        var asset=SvgAssets.Register(path);
        var mesh=SvgAssets.Resolve(asset.Id);
        _bridge.BindRepresentation(id,EntityRepresentationKind.Symbol,mesh);
        _bridge.SetRepresentation(id,EntityRepresentationKind.Symbol);
        UnitIcons.Remember(asset.SourcePath);
        RecordRepresentation(id, before);
        Publish();
        return asset;
    }

    public void ClearSymbol(Guid id)
    {
        var before = _bridge.CaptureRepresentation(id);
        _bridge.ClearSymbol(id);
        RecordRepresentation(id, before);
        Publish();
    }

    public SvgAssetEntry? GetSymbolAsset(Guid id) =>
        _bridge.GetResource(id,EntityRepresentationKind.Symbol) is { } mesh ? SvgAssets.Get(mesh.AssetId) : null;

    public BillboardStyle GetSymbolStyle(Guid id) => _bridge.GetSymbolStyle(id);

    public void SetSymbolStyle(Guid id,BillboardStyle style)
    {
        var before = _bridge.CaptureRepresentation(id);
        _bridge.SetSymbolStyle(id,style);
        RecordRepresentation(id, before);
        Publish();
    }

    private void RecordRepresentation(Guid id, RepresentationSnapshot before)
    {
        var after = _bridge.CaptureRepresentation(id);
        if (before != after) RecordEdit(() => _bridge.RestoreRepresentation(id, before),
            () => _bridge.RestoreRepresentation(id, after));
    }

    internal Matrix4x4 LocalDrawMatrix(Guid id, Matrix4x4 projection,uint width,uint height) =>
        _bridge.LocalModelToClip(id,projection,width,height);

    internal Matrix4x4 GeographicDrawMatrix(Guid id,GeoRenderFrame frame,uint width,uint height) =>
        _bridge.GeographicModelToClip(id,frame,width,height);
}