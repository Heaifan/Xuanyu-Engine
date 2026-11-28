using XuanYu.Render;

namespace XuanYu.Runtime;

internal readonly record struct EntityRenderBinding(RenderMesh? Mesh,
    RenderMesh? Proxy, EntityRepresentationKind Active)
{
    public RenderMesh? Point { get; init; }
    public RenderMesh? Symbol { get; init; }
    public BillboardStyle SymbolStyle { get; init; } = BillboardStyle.Default;

    public RenderMesh? Resource(EntityRepresentationKind kind) => kind switch
    {
        EntityRepresentationKind.Mesh => Mesh,
        EntityRepresentationKind.Proxy => Proxy,
        EntityRepresentationKind.Point => Point,
        EntityRepresentationKind.Symbol => Symbol,
        _ => null
    };

    public RenderMesh? ActiveMesh => Resource(Active);
}