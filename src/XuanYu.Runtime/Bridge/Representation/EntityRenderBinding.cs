using XuanYu.Render;

namespace XuanYu.Runtime;

internal readonly record struct EntityRenderBinding(RenderMesh? Mesh,
    RenderMesh? Proxy, EntityRepresentationKind Active)
{
    public RenderMesh? Resource(EntityRepresentationKind kind) => kind switch
    {
        EntityRepresentationKind.Mesh => Mesh,
        EntityRepresentationKind.Proxy => Proxy,
        _ => null
    };

    public RenderMesh? ActiveMesh => Resource(Active);
}