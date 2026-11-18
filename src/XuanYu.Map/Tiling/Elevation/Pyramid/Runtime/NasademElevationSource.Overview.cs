namespace XuanYu.World.Maps;

public sealed partial class NasademElevationSource
{
    private readonly NasademOverviewDataset? _overview;
    private readonly NasademOverviewLevel _overviewLevel;
    private readonly NasademElevationSource? _far;
    private readonly NasademElevationSource? _middle;

    private NasademElevationSource(NasademOverviewDataset dataset, NasademOverviewLevel level)
    {
        _overview = dataset;
        _overviewLevel = level;
    }

    public bool HasOverview => _far is not null;
    public int OverviewCount => _far?._overview?.Count ?? 0;
    public long OverviewBytes => _far?._overview?.SampleBytes ?? 0;
    public int OverviewCacheHits => _far?._overview?.CacheHits ?? 0;

    public int OverviewMiddleCount => _far?._overview?.MiddleCount ?? 0;
    public int OverviewMiddleCapacity => _far?._overview?.MiddleCapacity ?? 0;

    public NasademElevationSource CoarseFor(int level) =>
        (level <= 9 ? _far : _middle) ?? this;

    private static double? InterpolateOverview(NasademOverviewGrid grid,
        double latitude, double longitude)
    {
        int cells = grid.Side - 1;
        double x = Math.Clamp(SnapGrid((longitude - grid.Info.WestLongitude) * cells), 0d, cells);
        double y = Math.Clamp(SnapGrid((latitude - grid.Info.SouthLatitude) * cells), 0d, cells);
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
        int x1 = Math.Min(x0 + 1, cells), y1 = Math.Min(y0 + 1, cells);
        return Mix(Mix(grid.HeightAt(x0, y0), grid.HeightAt(x1, y0), x - x0),
            Mix(grid.HeightAt(x0, y1), grid.HeightAt(x1, y1), x - x0), y - y0);
    }
}