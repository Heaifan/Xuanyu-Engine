using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public static class GeoUnifiedPosition
{
    public static EnuPosition At(EnuFrame anchor, int x, int y, short height)
    {
        var geo = new GeoCoordinate(y / 3600d - 90, x / 3600d - 180, 0);
        var p = anchor.ToEnu(Wgs84.ToEcef(geo));
        return new(p.East, p.North, p.Up + height);
    }
}