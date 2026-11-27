using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Render.Vulkan;

public readonly record struct TerrainPoolUpload(long Key, ReadOnlyMemory<short> Heights);
public readonly record struct TerrainGridStyle(bool Geographic, double StepX, double StepY,
    long CenterX, long CenterY)
{
    public double NextRatio { get; init; }
    public float Blend { get; init; }
}

public sealed record TerrainVisualStyle
{
    public IReadOnlyList<Vector4> Palette { get; }
    public bool Hillshade { get; }
    public TerrainGridStyle? Grid { get; init; }
    public Vector4 Contours { get; init; }
    public Vector4? Selection { get; init; }
    public IReadOnlyList<Vector4> LabelGaps { get; init; } = [];

    public TerrainVisualStyle(IReadOnlyList<Vector4> palette, bool hillshade)
    {
        if (palette.Count != 7) throw new ArgumentException("G07 色阶需要七个节点。", nameof(palette));
        Palette = palette;
        Hillshade = hillshade;
    }
}
public readonly record struct TerrainPoolDraw(long Key, int South, int West, int Step,
    GeoCoordinate Origin, Matrix4x4 Mvp, int Mask = 0, IReadOnlyList<Vector4>? ClipRects = null)
{
    public TerrainVisualStyle? Style { get; init; }
}
public sealed record TerrainPoolFrame(IReadOnlyList<TerrainPoolUpload> Uploads,
    IReadOnlyList<long> ProtectedKeys, IReadOnlyList<long> EvictKeys,
    IReadOnlyList<long> EvictBeforePublish, IReadOnlyList<TerrainPoolDraw> Draws, bool PublishBeforeDraw = true);
public sealed record TerrainPoolFrameResult(byte[] Pixels, int SubmittedPages, int PendingBeforePublish,
    int ResidentBeforePublish, int ResidentForDraw, int PendingForDraw, int TotalBatches, int TotalUploadedPages,
    bool[] Evicted, bool[] EvictedBeforePublish);
public sealed record TerrainPoolProbeResult(ulong HeightAllocationBytes, TerrainPoolFrameResult[] Frames);