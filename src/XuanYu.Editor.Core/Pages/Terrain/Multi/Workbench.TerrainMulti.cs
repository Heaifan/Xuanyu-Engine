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
        NasademSourceResidency s = geo.SourceResidency;
        Log.Write($"G-05D-02：已索引 {catalog.Count} 张 HGT，原始总量 {catalog.Count * NasademTile.ByteLength / 1048576d:F1} MiB，耗时 {watch.Elapsed.TotalSeconds:F2} 秒");
        Log.Write($"G-05D-02-R3：派生准备 {geo.OverviewCount} 片，Far+Middle 数组 {geo.OverviewBytes / 1048576d:F2} MiB，L0-9 Far，L10-13 Middle，Native 原始精度");
        Log.Write($"G-05D-03A：派生缓存命中 {geo.OverviewCacheHits}/{geo.OverviewCount} 片，重建解码 {geo.OverviewCount - geo.OverviewCacheHits} 次；目录 {cacheDirectory}");
        Log.Write($"G-05D-03：流式原始高程已启用，Middle {geo.OverviewMiddleCount}/{geo.OverviewMiddleCapacity} 片，后台最多 1 个解码");
        Log.Write($"G-05D-02：首屏准备，高程驻留 {s.ResidentCount}/{s.Capacity} 片，数组 {s.ResidentBytes / 1048576d:F1} MiB，解码 {s.LoadCount} 次，淘汰 {s.EvictedCount} 次");
        Log.Write($"G-05BR：瓦片 {string.Join(", ", entries.Select(t => t.TileId))}");
        Log.Write($"[G06/SWB] 已发现掩膜={entries.Count(t => t.HasWaterMask)}/{entries.Count}；已读取={geo.LoadedWaterMaskCount}/{entries.Count}；{geo.WaterStatus}");
        Log.Write("G-05BR：仅预览已导入范围；多 HGT 暂不支持保存 XYMAP。");
        var report = TerrainDemCoverageReport.Check(catalog.Coordinates);
        Log.Write($"G-05BR2：外包矩形内未导入 {report.MissingCount} 片（不等于渲染缺片）");
        Log.Write(report.Holes.Count == 0 ? "G-05BR2：未发现被包围的内部缺口" :
            $"G-05BR2：内部缺口 {report.Holes.Count} 片：{string.Join(", ", report.Holes.Take(20))}");
    }
}