namespace XuanYu.World.Maps;

public sealed class GeoUnifiedSamples
{
    private readonly HashSet<(int South, int West)> _coverage;
    private readonly GeoUnifiedTileLoader _loader;
    internal GeoUnifiedTileLoader Loader => _loader;
    private readonly string _version;
    private readonly long _generation;
    internal (Guid, long) Identity => (_loader.Identity, _generation);

    public GeoUnifiedSamples(NasademTileCatalog catalog, string version)
        : this(catalog, version, new GeoUnifiedTileLoader(catalog, long.MaxValue)) { }

    public GeoUnifiedSamples(NasademTileCatalog catalog, string version, GeoUnifiedTileLoader loader)
    {
        if (!ReferenceEquals(catalog, loader.Catalog))
            throw new ArgumentException("采样器与加载器必须使用同一数据目录。", nameof(loader));
        _coverage = catalog.Coordinates.ToHashSet();
        _version = version;
        _loader = loader;
        _generation = loader.Generation;
    }

    public void Add(NasademTile tile)
    {
        using var pin = _loader.PinSample(_generation);
        var key = (tile.SouthLatitude, tile.WestLongitude);
        if (!_coverage.Contains(key)) throw new ArgumentException("瓦片不在数据目录中。", nameof(tile));
        _loader.Install(tile);
    }

    public GeoUnifiedSample Sample(int x, int y)
    {
        using var pin = _loader.PinSample(_generation);
        if (x is < 0 or > 1_296_000 || y is < 0 or > 648_000)
            return Result(GeoSampleStatus.OutsideCoverage);
        if (x == 1_296_000) x = 0;
        int west = x / 3600 - 180, south = Math.Min(y / 3600 - 90, 89);
        Span<int> westCandidates = stackalloc int[2] { west, west == -180 ? 179 : west - 1 };
        Span<int> southCandidates = stackalloc int[2] { south, south - 1 };
        int westCount = x % 3600 == 0 ? 2 : 1;
        int southCount = y % 3600 == 0 && y > 0 && y < 648_000 ? 2 : 1;
        bool covered = false;
        foreach (int s in southCandidates[..southCount])
            foreach (int w in westCandidates[..westCount])
            {
                var key = (s, w);
                if (!_coverage.Contains(key)) continue;
                covered = true;
                if (!_loader.TryGet(s, w, out var tile)) return Result(GeoSampleStatus.NotLoaded);
                int col = x - (w + 180) * 3600;
                if (col < 0) col += 1_296_000;
                int row = y - (s + 90) * 3600;
                if (tile.HeightAt(col, row) is { } value)
                    return new(GeoSampleStatus.Valid, value, _version);
            }
        return Result(covered ? GeoSampleStatus.NoData : GeoSampleStatus.OutsideCoverage);
    }

    private GeoUnifiedSample Result(GeoSampleStatus status) => new(status, null, _version);

    internal IDisposable PinResident() => _loader.PinResident(_generation);
}
