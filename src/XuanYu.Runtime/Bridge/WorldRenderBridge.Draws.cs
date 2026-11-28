using System.Numerics;
using XuanYu.Render;
using XuanYu.World;
using XuanYu.World.Ecs;

namespace XuanYu.Runtime;

public sealed partial class WorldRenderBridge
{
    public RenderDraw[] Snapshot(Matrix4x4 worldToClip, Guid? selectedId = null,
        uint width = 0, uint height = 0)
    {
        List<RenderDraw> draws = [];
        foreach (WorldEntity entity in _world.Entities)
        {
            if (entity.Transform.Spatial.Space != SpatialSpace.Local ||
                !_bindings.TryGetValue(entity.Id,out var binding) || binding.ActiveMesh is not { } mesh) continue;
            var matrix = LocalMatrix(entity.Transform, binding, worldToClip, width, height);
            bool billboard = IsBillboard(binding.Active);
            draws.Add(new(mesh, matrix, entity.IsVisible && (!billboard || IsBillboardInDepth(matrix)), entity.Id == selectedId)
                { IsBillboard = billboard, OutlineThroughOcclusion = billboard });
        }
        return [.. draws];
    }

    public RenderDraw[] SnapshotGeographic(GeoRenderFrame frame, Guid? selectedId = null,
        uint width = 0, uint height = 0)
    {
        List<RenderDraw> draws = [];
        foreach (WorldEntity entity in _world.Entities)
        {
            if (entity.Transform.Spatial.Space != SpatialSpace.Ecef ||
                !_bindings.TryGetValue(entity.Id,out var binding) || binding.ActiveMesh is not { } mesh) continue;
            var matrix = GeographicMatrix(entity.Transform,binding,frame,width,height);
            bool billboard = IsBillboard(binding.Active);
            bool visible = entity.IsVisible && (!billboard || (IsBillboardInDepth(matrix) && frame.IsAboveHorizon(entity.Transform.Spatial.ToEcef())));
            draws.Add(new(mesh,matrix,visible,entity.Id==selectedId)
                { IsBillboard = billboard, OutlineThroughOcclusion = billboard });
        }
        return [.. draws];
    }

    public Matrix4x4 LocalModelToClip(Guid id, Matrix4x4 projection,uint width,uint height) =>
        LocalMatrix(_world.Find(id)!.Transform,_bindings[id],projection,width,height);

    public Matrix4x4 GeographicModelToClip(Guid id, GeoRenderFrame frame,uint width,uint height) =>
        GeographicMatrix(_world.Find(id)!.Transform,_bindings[id],frame,width,height);

    private static Matrix4x4 LocalMatrix(Transform transform, EntityRenderBinding binding,
        Matrix4x4 projection,uint width,uint height) => IsBillboard(binding.Active)
        ? BillboardMatrix.Create(transform.Position,projection,Style(binding),width,height)
        : transform.ToMatrix()*projection;

    private static Matrix4x4 GeographicMatrix(Transform transform,EntityRenderBinding binding,
        GeoRenderFrame frame,uint width,uint height) => IsBillboard(binding.Active)
        ? BillboardMatrix.Create(frame.ToRelative(transform.Spatial.ToEcef()),frame.RelativeProjection,
            Style(binding),width,height)
        : frame.ModelToClip(transform);

    private static BillboardStyle Style(EntityRenderBinding binding) =>
        binding.Active==EntityRepresentationKind.Point ? BillboardStyle.Point : binding.SymbolStyle;

    public static bool IsBillboardInDepth(Matrix4x4 m) => m.M44 > 0 && m.M43 >= 0 && m.M43 <= m.M44;

    public static bool IsBillboard(EntityRepresentationKind kind) =>
        kind is EntityRepresentationKind.Point or EntityRepresentationKind.Symbol;

    public RenderMesh? GetResource(Guid id,EntityRepresentationKind kind) =>
        _bindings.TryGetValue(id,out var binding) ? binding.Resource(kind) : null;

    public BillboardStyle GetSymbolStyle(Guid id) => _bindings[id].SymbolStyle;

    public void SetSymbolStyle(Guid id,BillboardStyle style)
    {
        if (!float.IsFinite(style.Size) || style.Size<=0)
            throw new ArgumentOutOfRangeException(nameof(style),"二维尺寸必须为有限正数。");
        if (GetResource(id,EntityRepresentationKind.Symbol) is null)
            throw new InvalidOperationException("实体尚未绑定 SVG 资产。");
        _bindings[id]=_bindings[id] with { SymbolStyle=style };
    }
}