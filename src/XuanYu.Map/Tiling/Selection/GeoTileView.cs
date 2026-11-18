using XuanYu.Core.Geodesy;
namespace XuanYu.World.Maps;

public readonly record struct GeoTileView
{
    public EcefPosition Position { get; }
    public double ForwardX { get; }
    public double ForwardY { get; }
    public double ForwardZ { get; }
    public double ConeHalfAngleRadians { get; }
    public double ProjectionScalePixels { get; }

    public GeoTileView(EcefPosition position, EcefPosition target,
        double verticalFovRadians, int viewportWidth, int viewportHeight)
    {
        if (!double.IsFinite(position.X) || !double.IsFinite(position.Y) ||
            !double.IsFinite(position.Z) || !double.IsFinite(target.X) ||
            !double.IsFinite(target.Y) || !double.IsFinite(target.Z))
            throw new ArgumentOutOfRangeException(nameof(position));
        if (!double.IsFinite(verticalFovRadians) ||
            verticalFovRadians <= 0d || verticalFovRadians >= Math.PI)
            throw new ArgumentOutOfRangeException(nameof(verticalFovRadians));
        if (viewportWidth <= 0 || viewportHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(viewportWidth));

        double dx = target.X - position.X;
        double dy = target.Y - position.Y;
        double dz = target.Z - position.Z;
        double length = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        if (length == 0d)
            throw new ArgumentException("相机位置与目标不能相同。", nameof(target));

        Position = position;
        ForwardX = dx / length;
        ForwardY = dy / length;
        ForwardZ = dz / length;
        double tangent = Math.Tan(verticalFovRadians * 0.5d);
        double aspect = (double)viewportWidth / viewportHeight;
        // 对矩形视锥使用包围圆锥：会多选瓦片，但不会裁掉矩形视锥内部的瓦片。
        ConeHalfAngleRadians = Math.Atan(tangent * Math.Sqrt(1d + aspect * aspect));
        ProjectionScalePixels = viewportHeight / (2d * tangent);
    }
}