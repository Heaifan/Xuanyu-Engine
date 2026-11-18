using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    public GeoTerrainRenderSession(params NasademTile[] tiles)
        : this(new NasademElevationSource(tiles), tiles.Select(t =>
            new NasademHgtInfo(t.TileId, t.SouthLatitude, t.WestLongitude)).ToArray()) { }

    public GeoTerrainRenderSession(NasademTileCatalog catalog, int cacheCapacity = 8,
        bool useOverview = false, string? overviewCacheDirectory = null,
        bool streamRaw = false)
        : this(new NasademElevationSource(catalog, cacheCapacity, useOverview, overviewCacheDirectory),
            catalog.Entries.Select(e => e.Info).ToArray())
    {
        if (streamRaw && (!useOverview || cacheCapacity < 4))
            throw new ArgumentException("流式 Native 需要分级高程及至少 4 张原始缓存。", nameof(streamRaw));
        StreamingEnabled = streamRaw;
    }

    private GeoTerrainRenderSession(NasademElevationSource source, NasademHgtInfo[] tiles)
    {
        if (tiles.Length == 0) throw new ArgumentException("至少需要一张 HGT。", nameof(tiles));
        _source = source;
        SourceCount = tiles.Length;
        _coverage = CreateCoverage(tiles);
        _regions = CreateRegions(tiles, _coverage);
        if (_regions.Length > 1 || source.HasOverview) _footprint = new GeoTerrainFootprintGrid(tiles, _coverage);
        _nativeSourceId = tiles.Length <= 2 ? string.Join("+", tiles.Select(t => t.TileId))
            : $"NASADEM · {tiles.Length} 片";
        var center = new GeoCoordinate(
            (_coverage.SouthLatitudeDegrees + _coverage.NorthLatitudeDegrees) / 2d,
            (_coverage.WestLongitudeDegrees + _coverage.EastLongitudeDegrees) / 2d, 0d);
        _anchor = new EnuFrame(center);
        _selector = new GeoTileSelector(200_000d, 4d,
            maxLevel: 13, maxTiles: 256);
        _coverRoots = GeographicTileScheme.Cover(_coverage, 0).Where(InCoverage).ToArray();
        foreach (GeoTileId root in _coverRoots)
            _meshes.Add(root, BuildCoarseMesh(root));
        Camera = new GlobalCamera(center, InitialDistanceMeters);
        Camera.SetTerrainCollision(SampleGroundHeight);
    }

    private long _nextSourceReport;
    public NasademSourceResidency SourceResidency => _source.Residency;
    public int OverviewCount => _source.OverviewCount;
    public long OverviewBytes => _source.OverviewBytes;
    public int OverviewCacheHits => _source.OverviewCacheHits;
    public int OverviewMiddleCount => _source.OverviewMiddleCount;
    public int OverviewMiddleCapacity => _source.OverviewMiddleCapacity;

    public NasademTileSummary? ReadSourceSummary(int south, int west) =>
        _source.ReadSummary(south, west);

    private void ReportSourceResidency()
    {
        NasademSourceResidency s = SourceResidency;
        if (!s.IsFileBacked || s.LoadCount < _nextSourceReport) return;
        // 小数据集逐次报告实际解码变化；大数据集保留原有节流。
        _nextSourceReport = s.LoadCount + (SourceCount <= s.Capacity ? 1 : 16);
        Notice?.Invoke($"G-05D-02：索引 {SourceCount} 片，高程驻留 {s.ResidentCount}/{s.Capacity} 片，" +
            $"数组 {s.ResidentBytes / 1048576d:F1} MiB，解码 {s.LoadCount} 次，淘汰 {s.EvictedCount} 次；" +
            $"后台完成 {BackgroundLoadCount} 次，Middle {OverviewMiddleCount}/{OverviewMiddleCapacity} 片");
    }
}