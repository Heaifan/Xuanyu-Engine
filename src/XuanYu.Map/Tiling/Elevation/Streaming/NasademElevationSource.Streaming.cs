namespace XuanYu.World.Maps;

public sealed partial class NasademElevationSource
{
    public long BackgroundLoadCount => _cache?.BackgroundLoads ?? 0;
    public bool RawPrefetchPending => _cache?.HasPending ?? false;
    public void PumpRawPrefetch() => _cache?.Pump();

    public bool PrefetchRaw(int south, int west) => _cache?.Request(south, west) ?? true;

    public bool PrepareNativeRegion(GeoTileBounds bounds)
    {
        if (_cache is null) return true;
        int south = Math.Max(-90, (int)Math.Ceiling(bounds.SouthLatitudeDegrees) - 1);
        int north = Math.Min(89, (int)Math.Floor(bounds.NorthLatitudeDegrees));
        int west = (int)Math.Ceiling(bounds.WestLongitudeDegrees) - 1;
        int east = (int)Math.Floor(bounds.EastLongitudeDegrees);
        // 边界点优先东/北源，缺失时回退西/南源；因此包含恰好接触的源片。
        for (int y = south; y <= north; y++)
            for (int x = west; x <= east; x++)
            {
                int lon = x < -180 ? x + 360 : x >= 180 ? x - 360 : x;
                if (!PrefetchRaw(y, lon)) return false;
            }
        return true;
    }
}