namespace XuanYu.World.Maps;

public sealed partial class NasademElevationSource
{
    public double? Sample(double latitudeDegrees, double longitudeDegrees)
    {
        if (!double.IsFinite(latitudeDegrees) || latitudeDegrees is < -90d or > 90d)
            throw new ArgumentOutOfRangeException(nameof(latitudeDegrees));
        if (!double.IsFinite(longitudeDegrees) || longitudeDegrees is < -180d or > 180d)
            throw new ArgumentOutOfRangeException(nameof(longitudeDegrees));
        double longitude = longitudeDegrees == 180d ? -180d : longitudeDegrees;
        int south = Math.Min(89, (int)Math.Floor(latitudeDegrees));
        int west = (int)Math.Floor(longitude);
        int previousWest = west == -180 ? 179 : west - 1;
        bool onSouthEdge = latitudeDegrees == south && south > -90;
        bool onWestEdge = longitude == west;

        if (TryRead(south, west, latitudeDegrees, longitude, out double? height)) return height;
        if (onSouthEdge && TryRead(south - 1, west, latitudeDegrees, longitude, out height)) return height;
        if (onWestEdge && TryRead(south, previousWest, latitudeDegrees, longitude, out height)) return height;
        if (onSouthEdge && onWestEdge &&
            TryRead(south - 1, previousWest, latitudeDegrees, longitude, out height)) return height;
        return null;
    }

    // 标签只读取已驻留 HGT；绝不触发流式缓存同步解码。
    public double? SampleResident(double latitudeDegrees, double longitudeDegrees)
    {
        int south = Math.Min(89, (int)Math.Floor(latitudeDegrees));
        int west = (int)Math.Floor(longitudeDegrees);
        if (TryGetResidentTile(south, west) is { } tile)
            return Interpolate(tile, latitudeDegrees, longitudeDegrees);
        if (latitudeDegrees == south && south > -90 &&
            TryGetResidentTile(south - 1, west) is { } below)
            return Interpolate(below, latitudeDegrees, longitudeDegrees);
        if (longitudeDegrees == west &&
            TryGetResidentTile(south, west == -180 ? 179 : west - 1) is { } left)
            return Interpolate(left, latitudeDegrees, longitudeDegrees);
        return null;
    }

    private bool TryRead(int south, int west, double latitude, double longitude, out double? height)
    {
        double lon = longitude < west ? longitude + 360d : longitude;
        if (_overview is not null)
        {
            var grid = _overview.Get(south, west, _overviewLevel);
            height = grid is null ? null : InterpolateOverview(grid, latitude, lon);
            return grid is not null;
        }
        NasademTile? tile = GetTile(south, west);
        height = tile is null ? null : Interpolate(tile, latitude, lon);
        return tile is not null;
    }
}