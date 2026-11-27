using System.Numerics;

namespace XuanYu.Render;

public readonly record struct RenderDraw(
    RenderMesh Mesh,
    Matrix4x4 ModelToClip,
    bool Visible,
    bool Selected)
{
    public bool OutlineThroughOcclusion { get; init; }
    public bool DisableTerrainHillshade { get; init; }
    public float ContourIntervalMeters { get; init; }
    public float ContourNextIntervalMeters { get; init; }
    public float ContourIntervalBlend { get; init; }
    public float ContourOpacity { get; init; }
    public RenderDemTile? ResidentDem { get; init; }
    public ReadOnlyMemory<float> DemUV { get; init; }
    public RenderWaterAtlas? WaterAtlas { get; init; }
    // 跨 HGT 分片缺少原始 DEM 时仍允许回退旧等高线。
    public bool LegacyContourOnly { get; init; }
    public Vector4 ContourLabelGap { get; init; }
    public ReadOnlyMemory<Vector4> ContourLabelGaps { get; init; }
}