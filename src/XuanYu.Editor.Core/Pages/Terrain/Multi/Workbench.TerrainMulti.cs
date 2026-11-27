using System.Diagnostics;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public void ImportTerrainFiles(IReadOnlyList<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        if (paths.Count == 0) throw new ArgumentException("至少选择一个 HGT 文件。", nameof(paths));
        if (paths.Count == 1)
        {
            ImportTerrain(paths[0]);
            return;
        }
        long started = Stopwatch.GetTimestamp();
        var watch = Stopwatch.StartNew();
        var catalog = new NasademTileCatalog(paths);
        var entries = Array.AsReadOnly(catalog.Entries.Select(TerrainDemEntry.Create)
            .OrderBy(item => item.TileId, StringComparer.Ordinal).ToArray());
        string cacheDirectory = Path.Combine(Environment.CurrentDirectory,
            "artifacts", "terrain", "pyramid");
        var geo = new GeoTerrainRenderSession(catalog, cacheCapacity: 16,
            useOverview: true, overviewCacheDirectory: cacheDirectory, streamRaw: true);
        geo.LoadWater(catalog, entries.Where(t => t.HasWaterMask).Select(t => t.Info).ToArray());
        // 多 HGT 仅使用此轻量兼容文档承载原有场景 UI，不参与正式地理渲染或保存。
        MapDocument map = MapDocument.CreateDefault("NASADEM 多瓦片预览");
        SetTerrain(map, null, paths[0], null, geo, entries);
        TrackG07Import(geo, started);
        Log.Write($"G-05D-02：已导入 {catalog.Count} 张 HGT，原始总量 {catalog.Count * NasademTile.ByteLength / 1048576d:F1} MiB，准备耗时 {watch.Elapsed.TotalSeconds:F2} 秒；" +
            $"派生缓存命中 {geo.OverviewCacheHits}/{geo.OverviewCount} 片，SWB={geo.LoadedWaterMaskCount}/{entries.Count}");
        var report = TerrainDemCoverageReport.Check(catalog.Coordinates);
        if (report.Holes.Count > 0)
            Log.Warn($"G-05BR2：内部缺口 {report.Holes.Count} 片：{string.Join(", ", report.Holes.Take(20))}", "地图导入");
    }
}