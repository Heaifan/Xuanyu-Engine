using XuanYu.Render;

namespace XuanYu.Runtime;

public readonly record struct RepresentationSnapshot(EntityRepresentationKind Active,
    RenderMesh? Symbol, BillboardStyle Style);