
using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public sealed class NasademGeoGrid
{
    public const int CellCount = NasademTile.Side - 1;

    public int SouthLatitude { get; }
    public int WestLongitude { get; }
    public GeoCoordinate SouthWestAnchor =>
        new(SouthLatitude, WestLongitude, 0d);

    public NasademGeoGrid(int southLatitude, int westLongitude)
    {
        if (southLatitude is < -90 or >= 90)
            throw new ArgumentOutOfRangeException(nameof(southLatitude));
        if (westLongitude is < -180 or >= 180)
            throw new ArgumentOutOfRangeException(nameof(westLongitude));

        SouthLatitude = southLatitude;
        WestLongitude = westLongitude;
    }

    public GeoCoordinate AtSample(int column, int rowFromSouth,
        double ellipsoidHeightMeters)
    {
        if ((uint)column >= NasademTile.Side)
            throw new ArgumentOutOfRangeException(nameof(column));
        if ((uint)rowFromSouth >= NasademTile.Side)
            throw new ArgumentOutOfRangeException(nameof(rowFromSouth));

        // 原始 NASADEM 正高不能直接作为 WGS84 椭球高传入。
        return new GeoCoordinate(
            SouthLatitude + (double)rowFromSouth / CellCount,
            WestLongitude + (double)column / CellCount,
            ellipsoidHeightMeters);
    }
}
