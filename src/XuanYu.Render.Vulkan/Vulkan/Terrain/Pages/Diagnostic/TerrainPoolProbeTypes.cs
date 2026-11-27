using System.Numerics;
using XuanYu.Core.Geodesy;

namespace XuanYu.Render.Vulkan;

public readonly record struct TerrainPoolUpload(long Key, ReadOnlyMemory<short> Heights);
public readonly record struct TerrainPoolDraw(long Key, int South, int West, int Step,
    GeoCoordinate Origin, Matrix4x4 Mvp, int Mask = 0);
public sealed record TerrainPoolFrame(IReadOnlyList<TerrainPoolUpload> Uploads,
    IReadOnlyList<long> ProtectedKeys, IReadOnlyList<long> EvictKeys,
    IReadOnlyList<long> EvictBeforePublish, IReadOnlyList<TerrainPoolDraw> Draws, bool PublishBeforeDraw = true);
public sealed record TerrainPoolFrameResult(byte[] Pixels, int SubmittedPages, int PendingBeforePublish,
    int ResidentBeforePublish, int ResidentForDraw, int PendingForDraw, int TotalBatches, int TotalUploadedPages,
    bool[] Evicted, bool[] EvictedBeforePublish);
public sealed record TerrainPoolProbeResult(ulong HeightAllocationBytes, TerrainPoolFrameResult[] Frames);