using System.Text;
using System.Text.Json;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static class GeoTileMonitorLogExporter
{
    private static readonly JsonSerializerOptions Options = new()
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static async Task WriteAsync(Stream output,
        IReadOnlyList<GeoTileMonitorFrame> frames)
    {
        using var writer = new StreamWriter(output, new UTF8Encoding(false),
            bufferSize: 4096, leaveOpen: true);
        await writer.WriteLineAsync(JsonSerializer.Serialize(new
        {
            kind = "header", schema = "xye.tile-monitor.v1",
            exportedAtUtc = DateTimeOffset.UtcNow,
            recordCount = frames.Count,
            maxRetainedFrames = GeoTileMonitorHistory.Capacity,
            notes = "渲染计算后快照；目标不等于实绘，实绘变化不等于磁盘 I/O；不含 GPU 耗时"
        }, Options));

        GeoTileMonitorFrame? previous = null;
        HashSet<GeoTileId>? previousDrawn = null;
        for (int i = 0; i < frames.Count; i++)
        {
            GeoTileMonitorFrame frame = frames[i];
            var drawn = frame.Snapshot.Tiles.Where(t => t.Drawn)
                .Select(t => t.Id).ToHashSet();
            var changes = previous is null ? null : new
            {
                drawnAdded = drawn.Except(previousDrawn!).Select(id => id.ToString()).ToArray(),
                drawnRemoved = previousDrawn!.Except(drawn).Select(id => id.ToString()).ToArray(),
                nativeDelta = frame.NativeDrawCount - previous.NativeDrawCount,
                meshEvictedDelta = frame.Mesh.EvictedCount - previous.Mesh.EvictedCount,
                hgtLoadedDelta = frame.Source.LoadCount - previous.Source.LoadCount,
                hgtEvictedDelta = frame.Source.EvictedCount - previous.Source.EvictedCount,
                backgroundLoadsDelta = frame.BackgroundLoads - previous.BackgroundLoads
            };
            await writer.WriteLineAsync(JsonSerializer.Serialize(new
            {
                kind = "frame", index = i, frame.CapturedAt,
                snapshot = frame.Snapshot,
                meshCache = frame.Mesh, hgtCache = frame.Source,
                frame.NativeDrawCount, frame.MiddleCount,
                frame.MiddleCapacity, frame.BackgroundLoads,
                parentFallbackCount = frame.Snapshot.Tiles.Count(t =>
                    t.Targeted && !t.Drawn && t.Fallback is not null),
                changes
            }, Options));
            previous = frame;
            previousDrawn = drawn;
        }
        await writer.FlushAsync();
    }
}