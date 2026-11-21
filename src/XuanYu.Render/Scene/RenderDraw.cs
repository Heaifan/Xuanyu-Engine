using System.Numerics;

namespace XuanYu.Render;

public readonly record struct RenderDraw(
    RenderMesh Mesh,
    Matrix4x4 ModelToClip,
    bool Visible,
    bool Selected)
{
    public bool OutlineThroughOcclusion { get; init; }
    public float ContourIntervalMeters { get; init; }
    public float ContourNextIntervalMeters { get; init; }
    public float ContourIntervalBlend { get; init; }
    public float ContourOpacity { get; init; }
    public RenderDemTile? ResidentDem { get; init; }
}