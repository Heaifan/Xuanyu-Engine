using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public static class GeoUnifiedPosition
{
    public static EnuPosition At(EnuFrame anchor, int x, int y, short height,
        short geoidCentimeters = short.MinValue)
    {
        var geo = new GeoCoordinate(y / 3600d - 90, x / 3600d - 180, 0);
        if (geoidCentimeters != short.MinValue)
            return anchor.ToEnu(Wgs84.ToEcef(new GeoCoordinate(geo.LatitudeDegrees,
                geo.LongitudeDegrees, height + geoidCentimeters / 100d)));
        // 旧诊断及尚未迁移的视口保留原几何合同。
        var p = anchor.ToEnu(Wgs84.ToEcef(geo));
        return new(p.East, p.North, p.Up + height);
    }
}