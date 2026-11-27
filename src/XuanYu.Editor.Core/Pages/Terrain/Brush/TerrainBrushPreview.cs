using System.Numerics;

namespace XuanYu.Editor.TerrainEditing;

public sealed record TerrainBrushPreview(
    Vector3 CenterMeters,
    IReadOnlyList<TerrainBrushOutline.Segment> Outer,
    IReadOnlyList<TerrainBrushOutline.Segment> Inner,
    Matrix4x4 MapToScene,
    float Strength)
{
    public Vector3? CursorMeters { get; init; }
    public float RadiusMeters { get; init; }
    public float InnerRadiusMeters { get; init; }
}