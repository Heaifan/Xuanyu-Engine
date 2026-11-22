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
    public ReadOnlyMemory<float> DemUV { get; init; }
    public RenderWaterAtlas? WaterAtlas { get; init; }
    // 跨 HGT 渲染分片只升级晕渲，保留原来跨界等高线的 Legacy 语义。
    public bool LegacyContourOnly { get; init; }
    public Vector4 ContourLabelGap { get; init; }
}