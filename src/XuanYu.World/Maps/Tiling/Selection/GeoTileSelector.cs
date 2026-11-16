namespace XuanYu.World.Maps;

public sealed class GeoTileSelector
{
    private readonly double _rootErrorMeters;
    private readonly double _maxScreenErrorPixels;
    private readonly double _heightMarginMeters;
    private readonly int _maxLevel;
    private readonly int _maxTiles;

    public GeoTileSelector(double rootErrorMeters, double maxScreenErrorPixels,
        int maxLevel = 12, int maxTiles = 128, double heightMarginMeters = 12_000d)
    {
        if (!double.IsFinite(rootErrorMeters) || rootErrorMeters <= 0d)
            throw new ArgumentOutOfRangeException(nameof(rootErrorMeters));
        if (!double.IsFinite(maxScreenErrorPixels) || maxScreenErrorPixels <= 0d)
            throw new ArgumentOutOfRangeException(nameof(maxScreenErrorPixels));
        if (maxLevel is < 0 or > GeoTileId.MaxLevel)
            throw new ArgumentOutOfRangeException(nameof(maxLevel));
        if (maxTiles < 2)
            throw new ArgumentOutOfRangeException(nameof(maxTiles));
        if (!double.IsFinite(heightMarginMeters) || heightMarginMeters < 0d)
            throw new ArgumentOutOfRangeException(nameof(heightMarginMeters));
        _rootErrorMeters = rootErrorMeters;
        _maxScreenErrorPixels = maxScreenErrorPixels;
        _heightMarginMeters = heightMarginMeters;
        _maxLevel = maxLevel;
        _maxTiles = maxTiles;
    }

    public IReadOnlyList<GeoTileId> Select(GeoTileView view,
        GeoTileBounds? coverage = null)
    {
        var pending = new Queue<GeoTileId>();
        var selected = new List<GeoTileId>();
        for (int x = 0; x < 2; x++)
        {
            var root = new GeoTileId(0, x, 0);
            if (IsVisible(root, view) && InCoverage(root, coverage))
                pending.Enqueue(root);
        }

        while (pending.Count > 0)
        {
            GeoTileId id = pending.Dequeue();
            GeoTileSphere sphere = GeoTileSphere.For(id, _heightMarginMeters);
            if (id.Level < _maxLevel && GeoTileSse.EstimatePixels(
                view, sphere, id.Level, _rootErrorMeters) > _maxScreenErrorPixels)
            {
                GeoTileId[] children = GeographicTileScheme.Children(id)
                    .Where(child => IsVisible(child, view) &&
                        InCoverage(child, coverage)).ToArray();
                if (children.Length > 0 &&
                    selected.Count + pending.Count + children.Length <= _maxTiles)
                {
                    foreach (GeoTileId child in children) pending.Enqueue(child);
                    continue;
                }
            }
            selected.Add(id);
        }
        return selected;
    }

    private bool IsVisible(GeoTileId id, GeoTileView view) =>
        GeoTileVisibility.IntersectsView(view,
            GeoTileSphere.For(id, _heightMarginMeters));

    private static bool InCoverage(GeoTileId id, GeoTileBounds? coverage)
    {
        if (coverage is not { } area) return true;
        GeoTileBounds tile = GeographicTileScheme.Bounds(id);
        return tile.WestLongitudeDegrees < area.EastLongitudeDegrees &&
            tile.EastLongitudeDegrees > area.WestLongitudeDegrees &&
            tile.SouthLatitudeDegrees < area.NorthLatitudeDegrees &&
            tile.NorthLatitudeDegrees > area.SouthLatitudeDegrees;
    }
}