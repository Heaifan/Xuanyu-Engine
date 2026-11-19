namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    public GeoTileMonitorHistory MonitorHistory { get; } = new();

    private void CaptureMonitorHistory()
    {
        if (!MonitorHistory.IsRecording) return;
        MonitorHistory.Append(new GeoTileMonitorFrame(
            ReadMonitor(), MeshCacheStats, SourceResidency,
            NativeDrawCount, OverviewMiddleCount, OverviewMiddleCapacity,
            BackgroundLoadCount, DateTimeOffset.Now));
    }
}