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
        const double normalStep = 1d / 3600d;
        int south = Math.Max(-90, (int)Math.Ceiling(bounds.SouthLatitudeDegrees - normalStep) - 1);
        int north = Math.Min(89, (int)Math.Floor(bounds.NorthLatitudeDegrees + normalStep));
        int west = (int)Math.Ceiling(bounds.WestLongitudeDegrees - normalStep) - 1;
        int east = (int)Math.Floor(bounds.EastLongitudeDegrees + normalStep);
        // 原始法线差分要读取边界外一角秒，沿用现有预取与日期变更线回绕。
        for (int y = south; y <= north; y++)
            for (int x = west; x <= east; x++)
            {
                int lon = x < -180 ? x + 360 : x >= 180 ? x - 360 : x;
                if (!PrefetchRaw(y, lon)) return false;
            }
        return true;
    }
}