using XuanYu.Core.Geodesy;

namespace XuanYu.World.Ecs;

public readonly record struct SpatialPosition
{
    public SpatialSpace Space { get; }
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    private SpatialPosition(SpatialSpace space, double x, double y, double z)
    {
        Space = space;
        X = x;
        Y = y;
        Z = z;
    }

    public static SpatialPosition FromLocal(double x, double y, double z) =>
        new(SpatialSpace.Local, x, y, z);

    public static SpatialPosition FromGeo(GeoCoordinate position) =>
        FromEcef(Wgs84.ToEcef(position));

    public static SpatialPosition FromEcef(EcefPosition position) =>
        new(SpatialSpace.Ecef, position.X, position.Y, position.Z);

    public static SpatialPosition FromEnu(EnuFrame frame, EnuPosition position) =>
        FromEcef(frame.ToEcef(position));

    public EcefPosition ToEcef()
    {
        if (Space != SpatialSpace.Ecef)
            throw new InvalidOperationException("本地空间位置没有地理参考。");
        return new EcefPosition(X, Y, Z);
    }

    public GeoCoordinate ToGeo() => Wgs84.ToGeo(ToEcef());

    public EnuPosition ToEnu(EnuFrame frame) => frame.ToEnu(ToEcef());

    // 本地坐标沿场景轴；ECEF 差值沿地心固定轴，不等同于 ENU。
    public (double X, double Y, double Z) RelativeTo(SpatialPosition origin)
    {
        if (Space != origin.Space)
            throw new InvalidOperationException("不能直接相减不同空间的位置。");
        return (X - origin.X, Y - origin.Y, Z - origin.Z);
    }
}